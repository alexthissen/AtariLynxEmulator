using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using KillerApps.Emulation.Core;

namespace KillerApps.Emulation.Atari.Lynx
{
	// "The data input is a ROM cartridge with 1 Mbyte of address space and can contain writable elements."
	// "The data input system is either a ROM cartridge or a magnetic tape reader. 
	// The system hardware will support both, but units will be made with either one or the other."
	public class RomCart : ICartridge
	{
		// "In a ROM Cart unit, the addresses for the ROM Cart are provided by an 8 bit shift 
		// register and a 11 bit counter. "
		private int shiftRegister = 0;
		private int counter = 0;
		private bool currentStrobe = false;
		private bool addressData = false;

		// TODO: Change back to private
		public RomCartMemoryBank Bank0 { get; set; }
		private RomCartMemoryBank Bank1 { get; set; }

		//private static TraceSwitch GeneralSwitch = new TraceSwitch("General", "General trace switch", "Error");

		public RomCart(int bank0Size, int bank1Size)
		{
			Bank0 = new RomCartMemoryBank(bank0Size);
			Bank1 = new RomCartMemoryBank(bank1Size);
		}

		public void LoadFromStream(Stream stream)
		{
			Bank0.Load(stream);
			if (Bank1.CartType != RomCartType.Unused) Bank1.Load(stream);
		}

		// TODO: Find out what writeable elements of cartridge are.
		// Assume only bank 1 can be write-enabled
		public bool WriteEnabled 
		{
			get { return Bank1.WriteEnabled; }
			set { Bank1.WriteEnabled = value; }
		}

		// "CartAddressData is the data input to the shift register ..."
		public void CartAddressData(bool data)
		{
			//Debug.WriteLineIf(GeneralSwitch.TraceVerbose, String.Format("CartAddressData (${0:x})", data));
			addressData = data;
		}

		// "... and CartAddressStrobe is the clock to the shift register. "
		public void CartAddressStrobe(bool strobe)
		{
			// "The CartAddressStrobe line is also the reset signal for the 11 bit counter. 
			// The counter is reset whenever the line is high (1)."
			if (strobe) counter = 0;

			// "The shift register accepts data from the CartAddressData line on rising (0 to 1) 
			// transitions of the CartAddressStrobe line."
			if (strobe && !currentStrobe)
			{
				// Clock a bit into the shifter
				shiftRegister = shiftRegister << 1;
				// "Data is shifted into the register most significant bit first."
				shiftRegister += addressData ? 1 : 0;
				shiftRegister &= 0xff; // Maximum of 8 bits for shift register
			}

			currentStrobe = strobe;
			//Debug.WriteLineIf(GeneralSwitch.TraceVerbose, String.Format("CartAddressStrobe (strobe={0}) shifter=${1:x}", strobe, shiftRegister));
		}

		public void Poke0(byte value)
		{
			Poke(Bank0, value);
		}

		public void Poke1(byte value)
		{
			Poke(Bank1, value);
		}

		public byte Peek0()
		{
			return Peek(Bank0);
		}

		public byte Peek1()
		{
			return Peek(Bank1);
		}

		private void Poke(RomCartMemoryBank bank, byte value)
		{
			if (!bank.WriteEnabled) return;

			// "The ROM Cart can also be written to. The addressing scheme is the same as for reads. 
			// The strobe is also self timed."
			bank.Poke(shiftRegister, counter, value);
			if (!currentStrobe)
			{
				counter++;
				counter &= 0x07ff;
			}
		}

		private byte Peek(RomCartMemoryBank bank)
		{
			byte data = bank.Peek(shiftRegister, counter);
			if (!currentStrobe)
			{
				counter++;
				counter &= 0x07ff;
			}
			return data;
		}
	}
}
