using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class FaultyCart : ICartridge
	{
		public const byte DEFAULT_CART_CONTENTS = 0x11;

		public bool WriteEnabled
		{
			get { return false; }
			set { /* Do nothing */ }
		}

		public bool AuxiliaryDigitalInOut { get; set; }
		public void CartAddressData(bool data) { }
		public void CartAddressStrobe(bool strobe) { }
		public void Poke0(byte value) { }
		public void Poke1(byte value) { }

		public byte Peek0()
		{
			return DEFAULT_CART_CONTENTS;
		}

		public byte Peek1()
		{
			return DEFAULT_CART_CONTENTS;
		}
	}
}
