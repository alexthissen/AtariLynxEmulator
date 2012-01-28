using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Eeproms.Tests
{
	public static class Eeprom93XXExtensions
	{
		public static void Start(this Eeprom93C46B eeprom)
		{
			eeprom.DI = true;
			eeprom.Update(true, false);//, true);
			eeprom.Update(true, true);//, true);
		}

		public static ushort Read(this Eeprom93C46B eeprom, ushort address)
		{
			eeprom.Pulse(true);
			eeprom.Pulse(false);
			address &= 0x3F;
			for (int i = 0; i < 6; i++)
			{
				eeprom.Pulse(((address >> (5 - i) & 0x01) != 0x00));
			}

			ushort value = 0;
			for (int i = 0; i < 16; i++)
			{
				eeprom.Pulse(false);
				value |= (ushort)((eeprom.DO ? 1 : 0) << (15 - i));
			}
			return value;
		}

		public static void Write(this Eeprom93C46B eeprom, ushort address, ushort data)
		{
			eeprom.Pulse(false);
			eeprom.Pulse(true);
			address &= 0x3F;
			for (int i = 0; i < 6; i++)
			{
				eeprom.Pulse(((address >> (5 - i) & 0x01) != 0x00));
			}

			for (int i = 0; i < 16; i++)
			{
				bool value = ((data >> (15 - i)) & 0x01) == 1;
				eeprom.Pulse(value);
			}
		}

		public static void WriteAll(this Eeprom93C46B eeprom, ushort data)
		{
			eeprom.Pulse(false);
			eeprom.Pulse(false);
			eeprom.Pulse(false);
			eeprom.Pulse(true);
			eeprom.Pulse(true);
			eeprom.Pulse(true);
			eeprom.Pulse(true);
			eeprom.Pulse(true);

			for (int i = 0; i < 16; i++)
			{
				bool value = ((data >> (15 - i)) & 0x01) == 1;
				eeprom.Pulse(value);
			}
		}

		public static void DisableEraseWrite(this Eeprom93C46B eeprom)
		{
			// Opcode for EWDS is 0000XXXX
			for (int i = 0; i < 8; i++)
			{
				eeprom.Pulse(false);			
			}
		}

		public static void EnableEraseWrite(this Eeprom93C46B eeprom)
		{
			// Opcode for EWEN is 0011XXXX
			eeprom.Pulse(false);
			eeprom.Pulse(false);
			eeprom.Pulse(true);
			eeprom.Pulse(true);
			eeprom.Pulse(false);
			eeprom.Pulse(false);
			eeprom.Pulse(false);
			eeprom.Pulse(false);
		}

		public static void Pulse(this Eeprom93C46B eeprom, bool dataIn)
		{
			eeprom.DI = dataIn;
			eeprom.Update(true, false);//, dataIn);
			eeprom.Update(true, true);//, dataIn);
		}
	}
}
