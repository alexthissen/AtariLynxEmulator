using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using KillerApps.Emulation.Processors;

namespace KillerApps.Emulation.AtariLynx
{
	// "The data input is a ROM cartridge with 1 Mbyte of address space and can contain writable elements."
	// "The data input system is either a ROM cartridge or a magnetic tape reader. 
	// The system hardware will support both, but units will be made with either one or the other."
	public class RomCart : ICartridge
	{
		// "In a ROM Cart unit, the addresses for the ROM Cart are provided by an 8 bit shift 
		// register and a 11 bit counter."
		protected int shiftRegister = 0;
		protected int counter = 0;
		protected bool currentStrobe = false;
		protected bool addressData = false;

		private RomCartMemoryBank Bank0 { get; set; }
		private RomCartMemoryBank Bank1 { get; set; }

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
			// "Since some types of ROM do not have a useful power down mode, we provide a switched power pin 
			// to the cartridge. This pin is controlled by the state of the 'CartAddressData' signal from Mikey. 
			// Yes, this is the same pin that we use as a data source while clocking the address shift register 
			// and therefor, we will be switching ROM power on and off while loading that register. 
			// Unless the software is poorly arranged, that interval of power switching will be short. 
			// The switched power pin is powered up by setting the 'CartAddressData' signal low. 
			// It is suggested that the pin be powered up for the read of any ROM cart since carts that do not 
			// need it will not be wired to that pin. 
			// Additionally, information in that ROM cart can tell the software if it needs to further 
			// manipulate the pin."
			addressData = data;

			//Debug.WriteLineIf(GeneralSwitch.TraceVerbose, String.Format("CartAddressData (${0:x})", data));
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

		public virtual bool AuxiliaryDigitalInOut { get; set; }

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

		// "The length of the strobe is 562.5 ns, the data is stable for 125 ns prior to the strobe and 
		// for 62.5 ns after the strobe. This is a 'blind' write from the CPU and must not be interrupted 
		// by another access to Suzy until it is finished. 
		// The CPU must not access Suzy for 12 ticks after the completion of the 'blind' write cycle."
		protected virtual void Poke(RomCartMemoryBank bank, byte value)
		{
			// "The ROM Cart can also be written to. The addressing scheme is the same as for reads."
			if (bank.WriteEnabled) bank.Poke(shiftRegister, counter, value);

			// "The strobe is also self timed."
			if (!currentStrobe)
			{
				counter++;
				// "... a 11 bit counter"
				counter &= 0x07ff;
			}
		}

		protected virtual byte Peek(RomCartMemoryBank bank)
		{
			byte data = bank.Peek(shiftRegister, counter);
			if (!currentStrobe)
			{
				counter++;
				// "... a 11 bit counter"
				counter &= 0x07ff;
			}
			return data;
		}
	}
}
