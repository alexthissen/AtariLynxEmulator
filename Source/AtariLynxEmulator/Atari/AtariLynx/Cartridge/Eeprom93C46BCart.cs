using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Eeproms;

namespace KillerApps.Emulation.Atari.Lynx
{
	// 93c46 - a 128 byte eeprom
	// 93c66 - a 512 byte eeprom (and 93c56 - a 256 byte eeprom)
	// 93c86 - a 2048 byte eeprom (and 93c76 - a 1024 byte eeprom) 

	// EE_C_WRITE      =    $40
	// EE_C_READ       =    $80
	// EE_C_ERASE      =    $C0
	// EE_C_EWEN       =    $30	Write Enable
	// EE_C_EWDS       =    $00	Write Disable

	// CARD
	// PORT               ----\/----      93C46(SMD too)
	// (18)  A7   --------| CS     |- +5V
	// (11)  A1   --------| CLK    |- NC
	//                +---| DI     |- NC
	// (32) AUDIN ----+---| DO     |- GND
	//                    ----------

	public class Eeprom93C46BCart : RomCart
	{
		private const ulong A1Mask = 0x0000000000000002;
		private const ulong A7Mask = 0x0000000000000080;

		public override bool AuxiliaryDigitalInOut
		{
			get
			{
				// Output from cartridge to AUDIN comes from DataOut line
				return Eeprom.DO;
			}
			set
			{
				// Input from AUDIN to cartridge goes to DataIn line
				Eeprom.DI = value;
			}
		}

		public Eeprom93C86B Eeprom { get; set; }

		public Eeprom93C46BCart(int bank0Size, int bank1Size)
			: base(bank0Size, bank1Size)
		{
			Eeprom = new Eeprom93C86B();
		}

		protected override void Poke(RomCartMemoryBank bank, byte value)
		{
			base.Poke(bank, value);

			ulong address = bank.GetAddress(this.shiftRegister, this.counter);
			bool a1 = (address & A1Mask) == A1Mask;
			bool a7 = (address & A7Mask) == A7Mask;
			Eeprom.Update(a7, a1); //, AuxiliaryDigitalInOut);
		}
	}
}
