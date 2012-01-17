using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public interface ICartridge
	{
		bool AuxiliaryDigitalInOut { get; set; }
		bool WriteEnabled { get; set; }
		void CartAddressData(bool data);
		void CartAddressStrobe(bool strobe);
		void Poke0(byte value);
		void Poke1(byte value);
		byte Peek0();
		byte Peek1();
	}
}
