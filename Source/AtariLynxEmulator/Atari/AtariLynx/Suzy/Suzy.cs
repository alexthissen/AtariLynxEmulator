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
	public partial class Suzy : IMemoryAccess<ushort, byte>, IResetable
	{
		public const int SUZY_BASEADDRESS = 0xFC00;
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
		internal Ram64KBMemory Ram;
		private ILynxDevice device;

		private SpriteContext context = new SpriteContext();

		public SpriteProcessStart SPRGO { get; private set; }
		public SpriteSystemControl SPRSYS { get; private set; }
		public SpriteInitializationBits SPRINIT { get; private set; }
		public SuzyBusEnable SUZYBUSEN { get; private set; }
		public Joystick JOYSTICK { get; private set; }

		public byte[] MathEFGH = new byte[4];
		public byte[] MathJKLM = new byte[4];
		public byte[] MathABCD = new byte[4];
		public byte[] MathNP = new byte[2];

		private SpriteControlBlock SCB = null;
		private SpriteEngine Engine = null;

		//private static TraceSwitch GeneralSwitch = new TraceSwitch("General", "General trace switch", "Error");

		public Suzy(ILynxDevice lynx)
		{
			device = lynx;
			Ram = lynx.Ram;

			SCB = new SpriteControlBlock();
			SPRGO = new SpriteProcessStart();
			SPRSYS = new SpriteSystemControl();
			SPRINIT = new SpriteInitializationBits(0);
			SUZYBUSEN = new SuzyBusEnable();
			JOYSTICK = new Joystick();
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
		int signAB = 0, signCD = 0, signEFGH = 0;
		
		public ulong Multiply16By16()
		{
			uint EFGH;
			SPRSYS.MathWarning = false;
			SPRSYS.MathInProcess = true;

			ushort AB = BitConverter.IsLittleEndian ? (ushort)((MathABCD[3] << 8) + MathABCD[2]) : (ushort)((MathABCD[2] << 8) + MathABCD[3]);
			ushort CD = BitConverter.IsLittleEndian ? (ushort)((MathABCD[1] << 8) + MathABCD[0]) : (ushort)((MathABCD[0] << 8) + MathABCD[1]);

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
			if (!BitConverter.IsLittleEndian) MathEFGH = MathEFGH.Reverse().ToArray();

			if (SPRSYS.Accumulate)
			{
				if (!BitConverter.IsLittleEndian) MathJKLM = MathJKLM.Reverse().ToArray();
				uint JKLM = BitConverter.ToUInt32(MathJKLM, 0);
				uint accumulate = JKLM + EFGH;

				long overflow = (long)(int)JKLM + (long)(int)EFGH;
				if (overflow > 0xFFFFFFFF || overflow < 0)
				{
					// "... and an accumulator overflow bit."
					SPRSYS.MathWarning = true;
					//Debug.WriteLineIf(GeneralSwitch.TraceWarning, "Suzy::Multiply16By16() - Overflow detected");
				}
				else
				{
					SPRSYS.MathWarning = false;
				}

				// TODO: "BIG NOTE: Unsafe access is broken for math operations.
				// Please reset it after every math operation or it will not be useful for sprite operations."
				// Save accumulated result
				MathJKLM = BitConverter.GetBytes(accumulate);
				if (!BitConverter.IsLittleEndian) MathJKLM = MathJKLM.Reverse().ToArray();
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
			ushort NP = BitConverter.IsLittleEndian ? (ushort)((MathNP[1] << 8) + MathNP[0]) : (ushort)((MathNP[0] << 8) + MathNP[1]);
			if (NP != 0)
			{
				//Debug.WriteLineIf(GeneralSwitch.TraceInfo, "Suzy::Divide32By16() - Unsigned math");
				uint ABCD, JKLM;
				
				// Reverse array for big endian
				if (!BitConverter.IsLittleEndian) MathEFGH = MathEFGH.Reverse().ToArray();
				uint EFGH = BitConverter.ToUInt32(MathEFGH, 0);
				ABCD = EFGH / NP;
				JKLM = EFGH % NP;

				MathABCD = BitConverter.GetBytes(ABCD);
				MathJKLM = BitConverter.GetBytes(JKLM);
				if (!BitConverter.IsLittleEndian)
				{
					MathABCD = MathABCD.Reverse().ToArray();
					MathJKLM = MathJKLM.Reverse().ToArray();
				}

				// "As a courtesy, the hardware will set J,K to zero so that the software can treat the remainder
				// as a 32 bit number."
				MathJKLM[3] = 0; // J
				MathJKLM[2] = 0; // K

				Debug.WriteLine("CSusie::DoMathDivide() EFGH=${0:X8} / NP={1:X4}", EFGH, NP);
				Debug.WriteLine("CSusie::DoMathDivide() Results (div) ABCD=${0:X8}", ABCD);
				Debug.WriteLine("CSusie::DoMathDivide() Results (mod) JKLM=${0:X8}", JKLM);
			}
			else
			{
				//Debug.WriteLineIf(GeneralSwitch.TraceWarning, "Suzy::Divide32By16() - Divide by zero detected");

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

		public void Initialize()
		{
			Engine = new SpriteEngine(context, Ram.GetDirectAccess(), this.SCB);
		}

		public ulong RenderSprites()
		{
			//Debug.WriteLineIf(GeneralSwitch.TraceInfo, "Suzy::RenderSprites");

			if (!SUZYBUSEN.BusEnabled || !SPRGO.SpriteProcessEnabled) return 0;
			
				// Start rendering sprites
			SPRSYS.SpriteProcessStarted = true;

			// Delegate to engine
			context.DontCollide = SPRSYS.DontCollide;
			context.VStretch = SPRSYS.VStretch;
			context.EveronEnabled = SPRGO.EveronDetectorEnabled;
			device.SystemClock.CompatibleCycleCount += (ulong)Engine.RenderSprites();
			
			// "When the engine finishes processing the sprite list, or if it has been requested 
			// to stop at the end of the current sprite, or if it has been forced off by 
			// writing a 00 to SPRGO, the SPRITESEN flip flop will be reset."
			SPRGO.SpriteProcessEnabled = false;
			SPRSYS.SpriteProcessStarted = false;	

			// TODO: Return actual number of clock cycles that passed
			return 10000; 
		}

		public void Poke(ushort address, byte value)
		{
			switch (address)
			{
				case Addresses.TMPADRL:
					Engine.TMPADR.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					Engine.TMPADR.HighByte = 0;
					break;
				case Addresses.TMPADRH:
					Engine.TMPADR.HighByte = value;
					break;
				case Addresses.TILTACUML:
					Engine.TILTACUM.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					Engine.TILTACUM.HighByte = 0;
					break;
				case Addresses.TILTACUMH:
					Engine.TILTACUM.HighByte = value;
					break;
				case Addresses.HOFFL:
					context.HOFF.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					context.HOFF.HighByte = 0;
					break;
				case Addresses.HOFFH:
					context.HOFF.HighByte = value;
					break;
				case Addresses.VOFFL:
					context.VOFF.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					context.VOFF.HighByte = 0;
					break;
				case Addresses.VOFFH:
					context.VOFF.HighByte = value;
					break;
				case Addresses.VIDBASL:
					context.VIDBAS.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					context.VIDBAS.HighByte = 0;
					break;
				case Addresses.VIDBASH:
					context.VIDBAS.HighByte = value;
					break;
				case Addresses.COLLBASL:
					context.COLLBAS.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					context.COLLBAS.HighByte = 0;
					break;
				case Addresses.COLLBASH:
					context.COLLBAS.HighByte = value;
					break;
				case Addresses.VIDADRL:
					Engine.VIDADR.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					Engine.VIDADR.HighByte = 0;
					break;
				case Addresses.VIDADRH:
					Engine.VIDADR.HighByte = value;
					break;
				case Addresses.COLLADRL:
					Engine.COLLADR.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					Engine.COLLADR.HighByte = 0;
					break;
				case Addresses.COLLADRH:
					Engine.COLLADR.HighByte = value;
					break;
				case Addresses.SCBNEXTL:
					SCB.SCBNEXT.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					SCB.SCBNEXT.HighByte = 0;
					break;
				case Addresses.SCBNEXTH:
					SCB.SCBNEXT.HighByte = value;
					break;
				case Addresses.SPRDLINEL:
					SCB.SPRDLINE.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					SCB.SPRDLINE.HighByte = 0;
					break;
				case Addresses.SPRDLINEH:
					SCB.SPRDLINE.HighByte = value;
					break;
				case Addresses.HPOSSTRTL:
					SCB.HPOSSTRT.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					SCB.HPOSSTRT.HighByte = 0;
					break;
				case Addresses.HPOSSTRTH:
					SCB.HPOSSTRT.HighByte = value;
					break;
				case Addresses.VPOSSTRTL:
					SCB.VPOSSTRT.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					SCB.VPOSSTRT.HighByte = 0;
					break;
				case Addresses.VPOSSTRTH:
					SCB.VPOSSTRT.HighByte = value;
					break;
				case Addresses.SPRHSIZL:
					SCB.SPRHSIZ.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					SCB.SPRHSIZ.HighByte = 0;
					break;
				case Addresses.SPRHSIZH:
					SCB.SPRHSIZ.HighByte = value;
					break;
				case Addresses.SPRVSIZL:
					SCB.SPRVSIZ.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					SCB.SPRVSIZ.HighByte = 0;
					break;
				case Addresses.SPRVSIZH:
					SCB.SPRVSIZ.HighByte = value;
					break;
				case Addresses.STRETCHL:
					Engine.STRETCH.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					Engine.STRETCH.HighByte = 0;
					break;
				case Addresses.STRETCHH:
					Engine.STRETCH.HighByte = value;
					break;
				case Addresses.TILTL:
					Engine.TILT.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					Engine.TILT.HighByte = 0;
					break;
				case Addresses.TILTH:
					Engine.TILT.HighByte = value;
					break;
				case Addresses.SPRDOFFL:
					Engine.SPRDOFF.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					Engine.SPRDOFF.HighByte = 0;
					break;
				case Addresses.SPRDOFFH:
					Engine.SPRDOFF.HighByte = value;
					break;
				case Addresses.SPRVPOSL:
					Engine.SPRVPOS.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					Engine.SPRVPOS.HighByte = 0;
					break;
				case Addresses.SPRVPOSH:
					Engine.SPRVPOS.HighByte = value;
					break;
				case Addresses.COLLOFFL:
					context.COLLOFF.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					context.COLLOFF.HighByte = 0;
					break;
				case Addresses.COLLOFFH:
					context.COLLOFF.HighByte = value;
					break;
				case Addresses.VSIZACUML:
					Engine.VSIZACUM.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					Engine.VSIZACUM.HighByte = 0;
					break;
				case Addresses.VSIZACUMH:
					Engine.VSIZACUM.HighByte = value;
					break;
				case Addresses.HSIZOFFL:
					context.HSIZOFF.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					context.HSIZOFF.HighByte = 0;
					break;
				case Addresses.HSIZOFFH:
					context.HSIZOFF.HighByte = value;
					break;
				case Addresses.VSIZOFFL:
					context.VSIZOFF.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					context.VSIZOFF.HighByte = 0;
					break;
				case Addresses.VSIZOFFH:
					context.VSIZOFF.HighByte = value;
					break;
				case Addresses.SCBADRL:
					Engine.SCBADR.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					Engine.SCBADR.HighByte = 0;
					break;
				case Addresses.SCBADRH:
					Engine.SCBADR.HighByte = value;
					break;
				case Addresses.PROCADRL:
					Engine.PROCADR.LowByte = value;
					// "Any CPU write to an LSB will set the MSB to 0."
					Engine.PROCADR.HighByte = 0;
					break;
				case Addresses.PROCADRH:
					Engine.PROCADR.HighByte = value;
					break;

				case Addresses.MATHA:
					MathABCD[3] = value;
					signAB = 0;

					// "The conversion that is performed on the CPU provided starting numbers is done when the 
					// upper byte is sent by the CPU."
					// Starting numbers meaning AB and CD
					if (SPRSYS.SignedMath)
					{
						//Debug.WriteLineIf(GeneralSwitch.TraceInfo, "Suzy::Multiply16By16 - Signed math multiply operation.");

						// "When signed multiply is enabled, the hardware will convert the number provided by the CPU 
						// into a positive number and save the sign of the original number."
						ushort AB = (ushort)((MathABCD[3] << 8) + MathABCD[2]);
						AB = ConvertSignedMathValue(AB, out signAB);

						// "The resultant positive number is placed in the same Suzy location as the original number 
						// and therefore the original number is lost."
						MathABCD[2] = (byte)(AB & 0xff);
						MathABCD[3] = (byte)(AB >> 8);
					}

					// "Writing to A will start a 16 bit multiply."
					Multiply16By16();

					// TODO: Add clock cycles for multiplication when switching from compatible to precise clock count
					//device.SystemClock.CompatibleCycleCount += Multiply16By16();
					break;
				case Addresses.MATHB:
					MathABCD[2] = value;
					// "Writing to B,D,F,H,K, or M will force a '0' to be written to A,C,E,G,J, or L, respectively."
					MathABCD[3] = 0;
					break;
				case Addresses.MATHC:
					MathABCD[1] = value;
					signCD = 0;
					// "The conversion that is performed on the CPU provided starting numbers is done when the 
					// upper byte is sent by the CPU."
					// Starting numbers meaning AB and CD
					if (SPRSYS.SignedMath)
					{
						//Debug.WriteLineIf(GeneralSwitch.TraceInfo, "Suzy::Multiply16By16 - Signed math multiply operation.");

						// "When signed multiply is enabled, the hardware will convert the number provided by the CPU 
						// into a positive number and save the sign of the original number."
						ushort CD = (ushort)((MathABCD[1] << 8) + MathABCD[0]);
						CD = ConvertSignedMathValue(CD, out signCD);

						// "The resultant positive number is placed in the same Suzy location as the original number 
						// and therefore the original number is lost."
						MathABCD[0] = (byte)(CD & 0xff);
						MathABCD[1] = (byte)(CD >> 8);
					}
					break;
				case Addresses.MATHD:
					MathABCD[0] = value;
					// "Writing to B,D,F,H,K, or M will force a '0' to be written to A,C,E,G,J, or L, respectively."
					MathABCD[1] = 0;
					break;
				case Addresses.MATHE:
					MathEFGH[3] = value;
					// "Writing to E will start 8 16 bit divide."
					Divide32By16();
					// TODO: Add clock cycles for multiplication when switching from compatible to precise clock count
					//device.SystemClock.CompatibleCycleCount += Divide32By16();
					break;
				case Addresses.MATHF:
					MathEFGH[2] = value;
					// "Writing to B,D,F,H,K, or M will force a '0' to be written to A,C,E,G,J, or L, respectively."
					MathEFGH[3] = 0;
					break;
				case Addresses.MATHG:
					MathEFGH[1] = value;
					break;
				case Addresses.MATHH:
					MathEFGH[0] = value;
					// "Writing to B,D,F,H,K, or M will force a '0' to be written to A,C,E,G,J, or L, respectively."
					MathEFGH[1] = 0;
					break;
				case Addresses.MATHJ:
					MathJKLM[3] = value;
					break;
				case Addresses.MATHK:
					MathJKLM[2] = value;
					// "Writing to B,D,F,H,K, or M will force a '0' to be written to A,C,E,G,J, or L, respectively."
					MathJKLM[3] = 0;
					break;
				case Addresses.MATHL:
					MathJKLM[1] = value;
					break;
				case Addresses.MATHM:
					MathJKLM[0] = value;
					// "The write to 'M' will clear the accumulator overflow bit."
					SPRSYS.MathWarning = false;
					// "Writing to B,D,F,H,K, or M will force a '0' to be written to A,C,E,G,J, or L, respectively."
					MathJKLM[1] = 0;
					break;
				case Addresses.MATHN:
					MathNP[1] = value;
					break;
				case Addresses.MATHP:
					MathNP[0] = value;
					// Although not documented, writing to P will force a '0' to be written to N
					// Fixes a nasty bug on divides
					MathNP[1] = 0;
					break;

				case Addresses.SPRGO:
					SPRGO.ByteData = value;
					// "Writing a 01 or 05 to SPRGO sets the 'SPRITESEN' flip flop which causes the 
					// sprite engine to start its operation."
					// "Write a 1 to start the process, at completion of process this bit will be reset to 0. 
					// Either setting or clearing this bit will clear the Stop At End Of Current Sprite bit."
					SPRSYS.StopAtEndOfCurrentSprite = false;
					break;

				case Addresses.RCART0: 
					// "FCB2 uses 'CART0/' as the strobe."
					// "Read or write 8 bits of data."
					device.Cartridge.Poke0(value);
					break;

				case Addresses.RCART1:
					// "FCB3 uses 'CART1/’ as the strobe."
					// "Read or write 8 bits of data."
					device.Cartridge.Poke1(value);
					break;

				case Addresses.SUZYHREV:
					throw new LynxException(String.Format("Suzy::Poke: Writing to read-only address at {0:X4}", address));

				// "Set to '$F3' after at least 100ms after power up and before any sprites are drawn."
				case Addresses.SPRINIT:
					SPRINIT.ByteData = value;
					break;

				case Addresses.SPRSYS:
					SPRSYS.ByteData = value;
					JOYSTICK.LeftHanded = SPRSYS.LeftHanded;
					break;

				case Addresses.SUZYBUSEN:
					SUZYBUSEN.ByteData = value;
					break;

				default:
					//Trace.WriteLineIf(GeneralSwitch.TraceWarning, String.Format("Suzy::Poke: Unknown address ${0:X4} specified (value={1:X2}).", address, value));
					break;
			}
		}

		public byte Peek(ushort address)
		{
			byte value = 0;

			switch (address)
			{
				case Addresses.TMPADRL: return Engine.TMPADR.LowByte;
				case Addresses.TMPADRH: return Engine.TMPADR.HighByte;
				case Addresses.TILTACUML: return Engine.TILTACUM.LowByte;
				case Addresses.TILTACUMH: return Engine.TILTACUM.HighByte;
				case Addresses.HOFFL: return context.HOFF.LowByte;
				case Addresses.HOFFH: return context.HOFF.HighByte;
				case Addresses.VOFFL: return context.VOFF.LowByte;
				case Addresses.VOFFH: return context.VOFF.HighByte;
				case Addresses.VIDBASL: return context.VIDBAS.LowByte;
				case Addresses.VIDBASH: return context.VIDBAS.HighByte;
				case Addresses.COLLBASL: return context.COLLBAS.LowByte;
				case Addresses.COLLBASH: return context.COLLBAS.HighByte;
				case Addresses.VIDADRL: return Engine.VIDADR.LowByte;
				case Addresses.VIDADRH: return Engine.VIDADR.HighByte;
				case Addresses.COLLADRL: return Engine.COLLADR.LowByte;
				case Addresses.COLLADRH: return Engine.COLLADR.HighByte;
				case Addresses.SCBNEXTL: return SCB.SCBNEXT.LowByte;
				case Addresses.SCBNEXTH: return SCB.SCBNEXT.HighByte;
				case Addresses.SPRDLINEL: return SCB.SPRDLINE.LowByte;
				case Addresses.SPRDLINEH: return SCB.SPRDLINE.HighByte;
				case Addresses.HPOSSTRTL: return SCB.HPOSSTRT.LowByte;
				case Addresses.HPOSSTRTH: return SCB.HPOSSTRT.HighByte;
				case Addresses.VPOSSTRTL: return SCB.VPOSSTRT.LowByte;
				case Addresses.VPOSSTRTH: return SCB.VPOSSTRT.HighByte;
				case Addresses.SPRHSIZL: return SCB.SPRHSIZ.LowByte;
				case Addresses.SPRHSIZH: return SCB.SPRHSIZ.HighByte;
				case Addresses.SPRVSIZL: return SCB.SPRVSIZ.LowByte;
				case Addresses.SPRVSIZH: return SCB.SPRVSIZ.HighByte;
				case Addresses.STRETCHL: return Engine.STRETCH.LowByte;
				case Addresses.STRETCHH: return Engine.STRETCH.HighByte;
				case Addresses.TILTL: return Engine.TILT.LowByte;
				case Addresses.TILTH: return Engine.TILT.HighByte;
				case Addresses.SPRDOFFL: return Engine.SPRDOFF.LowByte;
				case Addresses.SPRDOFFH: return Engine.SPRDOFF.HighByte;
				case Addresses.SPRVPOSL: return Engine.SPRVPOS.LowByte;
				case Addresses.SPRVPOSH: return Engine.SPRVPOS.HighByte;
				case Addresses.COLLOFFL: return context.COLLOFF.LowByte;
				case Addresses.COLLOFFH: return context.COLLOFF.HighByte;
				case Addresses.VSIZACUML: return Engine.VSIZACUM.LowByte;
				case Addresses.VSIZACUMH: return Engine.VSIZACUM.HighByte;
				case Addresses.HSIZOFFL: return context.HSIZOFF.LowByte;
				case Addresses.HSIZOFFH: return context.HSIZOFF.HighByte;
				case Addresses.VSIZOFFL: return context.VSIZOFF.LowByte;
				case Addresses.VSIZOFFH: return context.VSIZOFF.HighByte;
				case Addresses.SCBADRL: return Engine.SCBADR.LowByte;
				case Addresses.SCBADRH: return Engine.SCBADR.HighByte;
				case Addresses.PROCADRL: return Engine.PROCADR.LowByte;
				case Addresses.PROCADRH: return Engine.PROCADR.HighByte;
				
				case Addresses.SUZYHREV:
					return 0x01;

				case Addresses.RCART0:
					// "FCB2 uses 'CART0/' as the strobe."
					// "Read or write 8 bits of data."
					return device.Cartridge.Peek0();

				case Addresses.RCART1:
					// "FCB3 uses 'CART1/’ as the strobe."
					// "Read or write 8 bits of data."
					return device.Cartridge.Peek1();

				case Addresses.MATHD:
				case Addresses.MATHC:
				case Addresses.MATHB:
				case Addresses.MATHA:
					value = MathABCD[address - Addresses.MATHD];
					break;

				case Addresses.MATHP:
				case Addresses.MATHN:
					value = MathNP[address - Addresses.MATHP];
					break;

				case Addresses.MATHH:
				case Addresses.MATHG:
				case Addresses.MATHF:
				case Addresses.MATHE:
					value = MathEFGH[address - Addresses.MATHH];
					break;

				case Addresses.MATHM:
				case Addresses.MATHL:
				case Addresses.MATHK:
				case Addresses.MATHJ:
					value = MathJKLM[address - Addresses.MATHM];
					break;

				case Addresses.JOYSTICK:
					// Set left-handedness first
					value = JOYSTICK.Value;
					break;

				case Addresses.SPRCOL:
				case Addresses.SPRINIT:
				case Addresses.SPRCTL0:
				case Addresses.SPRCTL1:
				case Addresses.SUZYBUSEN:
					//Debug.WriteLineIf(GeneralSwitch.TraceWarning, String.Format("Suzy::Peek - Peeking at write-only address ${0:X4}", address));
					break;

				case Addresses.SPRSYS:
					value = SPRSYS.ByteData;
					break;

				default:
					//Trace.WriteLineIf(GeneralSwitch.TraceWarning, String.Format("Suzy::Peek -  Unknown address ${0:X4} specified.", address));
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

			context.HSIZOFF.Value = 0x007f;
			context.VSIZOFF.Value = 0x007f;

			MathABCD = BitConverter.GetBytes(0xFFFFFFFF);
			MathJKLM = BitConverter.GetBytes(0xFFFFFFFF);
			MathEFGH = BitConverter.GetBytes(0xFFFFFFFF);
			MathNP = BitConverter.GetBytes(0xFFFF);

			signAB = signCD = signEFGH = 1;
			//Debug.WriteLineIf(GeneralSwitch.TraceInfo, "Suzy::Reset");
		}
	}
}
