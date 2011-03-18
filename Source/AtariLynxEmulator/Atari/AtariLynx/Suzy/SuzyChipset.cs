using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	// "... and Suzy is only a sprite generation engine. Some non-sprite functions (the switch readers 
	// and the ROM reader) are in Suzy due to pin limitations. In addition the math functions are part 
	// of Suzys sprite engine."
	public class SuzyChipset : IMemoryAccess<ushort, byte>, IResetable
	{
		public const int SUZY_BASEADDRESS = 0xfc00;
		public const int SUZY_SIZE = 0x100;
		public const int SCREEN_WIDTH = 160;
		public const int SCREEN_HEIGHT = 102;
		public const int SPRITE_READWRITE_CYCLE = 3;

		// "Writes to Suzy are handled as 'always available' whether or not Suzy is actually available."
		// "All writes to Suzy are 'Blind' in that the write cycle is always 5 ticks long and does not get a DTACK. 
		// Suzy will accept the data immediately and then place it internally as required. 
		// The maximum time required for this internal placement is 6 ticks (except for writes to the game cart)."
		public const int SUZYHARDWARE_READ = 9; // "Suzy Hardware(read)               Min 9        Max 15"
		public const int SUZYHARDWARE_WRITE = 5; // "Suzy Hardware(write)               Min 5          Max 5"
		public const int AVAILABLEHARDWARE_READWRITE = 5; // "Available Hardware(r/w)            Min 5          Max 5"

		// "The video and refresh generators in Mikey and the sprite engine in Suzy see the entire 64K byte range as RAM."
		private IMemoryAccess<ushort, byte> Ram;
		private ILynxDevice device;

		public SpriteControlBits0 SPRCTL0 { get; private set; }
		public SpriteControlBits1 SPRCTL1 { get; private set; }
		public SpriteProcessStart SPRGO { get; private set; }
		public SpriteSystemControl SPRSYS { get; private set; }
		public SpriteInitializationBits SPRINIT { get; private set; }
		public SuzyBusEnable SUZYBUSEN { get; private set; }

		public byte[] MathEFGH = new byte[4];
		public byte[] MathJKLM = new byte[4];
		public byte[] MathABCD = new byte[4];
		public byte[] MathNP = new byte[2];

		private static TraceSwitch GeneralSwitch = new TraceSwitch("General", "General trace switch", "Error");

		public SuzyChipset(ILynxDevice lynx)
		{
			this.device = lynx;
			this.Ram = lynx.Ram;

			SPRCTL0 = new SpriteControlBits0(0);
			SPRCTL1 = new SpriteControlBits1(0);
			SPRGO = new SpriteProcessStart();
			SPRSYS = new SpriteSystemControl();
			SPRINIT = new SpriteInitializationBits(0);
			SUZYBUSEN = new SuzyBusEnable();
		}

		// "We have a 16 by 16 to 32 unsigned and signed multiply with accumulate and a ..."
		// "The results of a multiply are a 32 bit product, a 32 bit accumulate and an accumulator overflow bit."
		// "The accumulator is 32 bits and accumulates the result of multiply operations."
		// "The basic method of performing one of these math operations is to write the starting values into 
		// Suzy registers (all of the addresses are different) and then polling for completion prior to 
		// reading the results. The act of writing to the last register starts the math process."
		// "Each letter represents a different byte address. These addresses are identified in the hardware address Appx 2. Each grouping represents the kind of math operation available.
		//		AB   
		//	* CD 
		// -----
		//  EFGH
		// Accumulate in JKLM 
		// "Therefore, if you only have 8 bits in a particular number, there is no need to write 
		// the upper byte to '0'. (except for signed multiplies)"
		// "The actual steps required to perform some of the functions are:
		// 16 x 16 multiply:
		// Write LSB to D, MSB to C
		// Write LSB to B, MS8 to A
		// Poll MULTSTAT until done (or just wait for 54 ticks)
		// Read answer (LSB->MSB) from H,G,F,E
		// Accumulate:
		// To initialize the accumulator, write a '0' to K and M (This will put 0 in J and L). 
		// The write to 'M' will clear the accumulator overflow bit. Note that you can actually initialize 
		// the accumulator to any value by writing to all 4 bytes (J,K,L,M)."
		public ulong Multiply16By16()
		{
			uint EFGH;
			SPRSYS.MathWarning = false;
			SPRSYS.MathInProcess = true;
			int signAB = 0, signCD = 0, signEFGH = 0;

			ushort AB = (ushort)((MathABCD[3] << 8) + MathABCD[2]);
			ushort CD = (ushort)((MathABCD[1] << 8) + MathABCD[0]);
			
			if (SPRSYS.SignedMath)
			{
				Debug.WriteLineIf(GeneralSwitch.TraceInfo, "SuzyChipset::Multiply16By16 - Signed math multiply operation.");
				
				// "When signed multiply is enabled, the hardware will convert the number provided by the CPU 
				// into a positive number and save the sign of the original number."
				AB = ConvertSignedMathValue(AB, out signAB);
				CD = ConvertSignedMathValue(CD, out signCD);

				// "The conversion that is performed on the CPU provided starting numbers is done when the 
				// upper byte is sent by the CPU."

				// "The resultant positive number is placed in the same Suzy location as the original number 
				// and therefore the original number is lost."
				MathABCD[2] = (byte)(AB & 0xff);
				MathABCD[3] = (byte)(AB >> 8);
				MathABCD[0] = (byte)(CD & 0xff);
				MathABCD[1] = (byte)(CD >> 8); 
			}

			EFGH = (uint)AB * (uint)CD;

			if (SPRSYS.SignedMath)
			{
				// "At the end of a multiply, the signs of the original numbers are examined and 
				// if required, the multiply result is converted to a negative number."
				signEFGH = signAB + signCD; // Add the sign bits. Zero means negative result	
				if (signEFGH == 0)
				{
					EFGH ^= 0xffffffff; // Calculate 2-s complement
					EFGH++;
				}
			}

			MathEFGH = BitConverter.GetBytes(EFGH);

			if (SPRSYS.Accumulate)
			{
				uint JKLM = BitConverter.ToUInt32(MathJKLM, 0);
				uint accumulate = JKLM + EFGH;

				long overflow = (long)JKLM + (long)EFGH;
				if (overflow > 0xFFFFFFFF || overflow < 0)
				{
					// "... and an accumulator overflow bit."
					SPRSYS.LastCarry = true;
					Debug.WriteLineIf(GeneralSwitch.TraceWarning, "SuzyChipset::Multiply16By16() - Overflow detected");
				}
				else
				{
					SPRSYS.LastCarry = false;
				}

				// TODO: "BIG NOTE: Unsafe access is broken for math operations. Please reset it after every math operation or it will not be useful for sprite operations. "
				// Save accumulated result
				MathJKLM = BitConverter.GetBytes(accumulate);
			}

			SPRSYS.MathInProcess = false;

			// "Multiplies without sign or accumulate take 44 ticks to complete.
			// Multiplies with sign and accumulate take 54 ticks to complete"
			ulong cyclesUsed = 44;
			if (SPRSYS.SignedMath && SPRSYS.Accumulate) cyclesUsed = 54;
			return cyclesUsed;
		}

		internal ushort ConvertSignedMathValue(ushort value, out int sign)
		{
			// "In signed multiply, the hardware thinks that 8000 is a positive number."

			// "In signed multiply, the hardware thinks that 0 is a negative number. This is not an 
			// immediate problem for a multiply by zero, since the answer will be re-negated to the 
			// correct polarity of zero. However, since it will set the sign flag, you can not depend 
			// on the sign flag to be correct if you just load the lower byte after a multiply by zero."

			// Do conversion if value is negative. Subtract 1 to account for 0 being negative
			sign = 1;
			if (((value - 1) & 0x8000) == 0x8000)
			{
				ushort conversion = (ushort)(value ^ 0xFFFF);
				conversion++; // Add 1 for earlier correction
				sign = -1;
				value = conversion;
			}
			return value;
		}

		// "32 x 16 divide:
		// Write LSB to P, MSB to N,
		// Write LSB -> MSB to H,G,F,E
		// Poll DIVSTAT until done (or just wait 400 ticks max)
		// Read answer (LSB->MSB) from D,C,B,A
		// Read remainder (LSB->MSB) from M,L"

		// "Each letter represents a different byte address. These addresses are identified in the hardware address Appx 2. Each grouping represents the kind of math operation available.
		//	EFGH 
		// /	NP 
		//	----
		//	ABCD  
		//Remainder in (JK)LM"
		public ulong Divide32By16()
		{
			// "In divide, the remainder will have 2 possible errors, depending on its actual value. 
			// No point in explaining the errors here, just don't use it. Thank You VTI."
			// LX: VTI is manufacturer of chipset

			// TODO: "BIG NOTE: Unsafe access is broken for math operations. Please reset it after every math operation or it will not be useful for sprite operations."
			
			SPRSYS.MathInProcess = true;

			// "Mathbit. If mult, 1=accumulator overflow. If div, 1=div by zero attempted."
			SPRSYS.MathWarning = false;

			// KW: "Divide is ALWAYS unsigned arithmetic..."
			ushort NP = (ushort)((MathNP[1] << 8) + MathNP[0]);
			if (NP != 0)
			{
				Debug.WriteLineIf(GeneralSwitch.TraceInfo, "SuzyChipset::Divide32By16() - Unsigned math");
				uint ABCD, JKLM;
				uint EFGH = BitConverter.ToUInt32(MathEFGH, 0);
				ABCD = EFGH / NP;
				JKLM = EFGH % NP;

				MathABCD = BitConverter.GetBytes(ABCD);
				MathJKLM = BitConverter.GetBytes(JKLM);

				// "As a courtesy, the hardware will set J,K to zero so that the software can treat the remainder
				// as a 32 bit number."
				MathJKLM[3] = 0; // J
				MathJKLM[2] = 0; // K
			}
			else
			{
				Debug.WriteLineIf(GeneralSwitch.TraceWarning, "SuzyChipset::Divide32By16() - Divide by zero detected");

				// "The number in the dividend as a result of a divide by zero is 'FFFFFFFF (BigNum)."
				for (int index = 0; index < 4; index++)
				{
					MathABCD[index] = 0xFF;
					MathJKLM[index] = 0x00;
				}

				// "Mathbit. If mult, 1=accumulator overflow. If div, 1=div by zero attempted."
				SPRSYS.MathWarning = true;
			}

			SPRSYS.MathInProcess = false;

			// "Divides take 176 + 14*N ticks where N is the number of most significant zeros in the divisor."
			// TODO: Think of a smart way to count leading zeros in EFGH
			//int zeros = count number of zeros from E -> H
			ulong cyclesUsed = 176; // + 14 * zeros;
			return cyclesUsed;
		}


		public ulong PaintSprites()
		{
			Debug.WriteLineIf(GeneralSwitch.TraceInfo, "SuzyChipset::PaintSprites");
			return 0; 
		}

		public void Poke(ushort address, byte value)
		{
			switch (address)
			{
				case SuzyAddresses.MATHA:
					MathABCD[3] = value;
					// "Writing to A will start a 16 bit multiply."
					Multiply16By16();
					// TODO: Add clock cycles for multiplication when switching from compatible to precise clock count
					//device.SystemClock.CompatibleCycleCount += Multiply16By16();
					break;
				case SuzyAddresses.MATHB:
					MathABCD[2] = value;
					// "Writing to B,D,F,H,K, or M will force a '0' to be written to A,C,E,G,J, or L, respectively."
					MathABCD[3] = 0;
					break;
				case SuzyAddresses.MATHC:
					MathABCD[1] = value;
					break;
				case SuzyAddresses.MATHD:
					MathABCD[0] = value;
					// "Writing to B,D,F,H,K, or M will force a '0' to be written to A,C,E,G,J, or L, respectively."
					MathABCD[1] = 0;
					break;
				case SuzyAddresses.MATHE:
					MathEFGH[3] = value;
					// "Writing to E will start 8 16 bit divide."
					Divide32By16();
					// TODO: Add clock cycles for multiplication when switching from compatible to precise clock count
					//device.SystemClock.CompatibleCycleCount += Divide32By16();
					break;
				case SuzyAddresses.MATHF:
					MathEFGH[2] = value;
					// "Writing to B,D,F,H,K, or M will force a '0' to be written to A,C,E,G,J, or L, respectively."
					MathEFGH[3] = 0;
					break;
				case SuzyAddresses.MATHG:
					MathEFGH[1] = value;
					break;
				case SuzyAddresses.MATHH:
					MathEFGH[0] = value;
					// "Writing to B,D,F,H,K, or M will force a '0' to be written to A,C,E,G,J, or L, respectively."
					MathEFGH[1] = 0;
					break;
				case SuzyAddresses.MATHJ:
					MathJKLM[3] = value;
					break;
				case SuzyAddresses.MATHK:
					MathJKLM[2] = value;
					// "Writing to B,D,F,H,K, or M will force a '0' to be written to A,C,E,G,J, or L, respectively."
					MathJKLM[3] = 0;
					break;
				case SuzyAddresses.MATHL:
					MathJKLM[1] = value;
					break;
				case SuzyAddresses.MATHM:
					MathJKLM[0] = value;
					// "The write to 'M' will clear the accumulator overflow bit."
					SPRSYS.LastCarry = false;
					// "Writing to B,D,F,H,K, or M will force a '0' to be written to A,C,E,G,J, or L, respectively."
					MathJKLM[1] = 0;
					break;
				case SuzyAddresses.MATHN:
					MathNP[1] = value;
					break;
				case SuzyAddresses.MATHP:
					MathNP[0] = value;
					break;

				case SuzyAddresses.SPRGO:
					SPRGO.ByteData = value;
					// "Write a 1 to start the process, at completion of process this bit will be reset to 0. 
					// Either setting or clearing this bit will clear the Stop At End Of Current Sprite bit."
					SPRSYS.StopAtEndOfCurrentSprite = false;
					break;

				case SuzyAddresses.RCART0: 
					// "FCB2 uses 'CART0/' as the strobe."
					// "Read or write 8 bits of data."
					device.Cartridge.Poke0(value);
					break;

				case SuzyAddresses.RCART1:
					// "FCB3 uses 'CART1/’ as the strobe."
					// "Read or write 8 bits of data."
					device.Cartridge.Poke1(value);
					break;

				case SuzyAddresses.SUZYHREV:
					throw new LynxException(String.Format("Suzy::Poke: Writing to read-only address at {0:X4}", address));

				// "Set to '$F3' after at least 100ms after power up and before any sprites are drawn."
				case SuzyAddresses.SPRINIT:
					SPRINIT.ByteData = value;
					break;

				case SuzyAddresses.SPRSYS:
					SPRSYS.ByteData = value;
					break;

				default:
					break;
			}
		}

		public byte Peek(ushort address)
		{
			byte value = 0;

			switch (address)
			{
				case SuzyAddresses.SUZYHREV:
					return 0x01;

				case SuzyAddresses.RCART0:
					// "FCB2 uses 'CART0/' as the strobe."
					// "Read or write 8 bits of data."
					return device.Cartridge.Peek0();

				case SuzyAddresses.RCART1:
					// "FCB3 uses 'CART1/’ as the strobe."
					// "Read or write 8 bits of data."
					return device.Cartridge.Peek1();

				case SuzyAddresses.MATHD:
				case SuzyAddresses.MATHC:
				case SuzyAddresses.MATHB:
				case SuzyAddresses.MATHA:
					value = MathABCD[address - SuzyAddresses.MATHD];
					break;

				case SuzyAddresses.MATHP:
				case SuzyAddresses.MATHN:
					value = MathNP[address - SuzyAddresses.MATHP];
					break;

				case SuzyAddresses.MATHH:
				case SuzyAddresses.MATHG:
				case SuzyAddresses.MATHF:
				case SuzyAddresses.MATHE:
					value = MathEFGH[address - SuzyAddresses.MATHH];
					break;

				case SuzyAddresses.MATHM:
				case SuzyAddresses.MATHL:
				case SuzyAddresses.MATHK:
				case SuzyAddresses.MATHJ:
					value = MathJKLM[address - SuzyAddresses.MATHM];
					break;

				case SuzyAddresses.SPRCOL:
				case SuzyAddresses.SPRINIT:
				case SuzyAddresses.SPRCTL0:
				case SuzyAddresses.SPRCTL1:
				case SuzyAddresses.SUZYBUSEN:
					Debug.WriteLineIf(GeneralSwitch.TraceWarning, String.Format("Suzy::Peek - Peeking at write-only address ${0:X4}", address));
					break;

				default:
					break;
			}

			return value;
		}

		public void Reset()
		{
			SPRGO.ByteData = 0; // "reset = 0"

			// "FC92 = SPRSYS
			// (read) reset 0,0,0,x,x,0,x,0"
			SPRSYS.MathInProcess = false;
			SPRSYS.MathWarning = false;
			SPRSYS.LastCarry = false;
			SPRSYS.UnsafeAccess = false;
			SPRSYS.SpriteProcessStarted = false;

			SUZYBUSEN.ByteData = 0; // "reset = 0"

			Debug.WriteLineIf(GeneralSwitch.TraceInfo, "Suzy::Reset");
		}
	}
}
