using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public static class SuzyAddresses
	{
		public const ushort SUZYHREV = 0xFC88; // Suzy Hardware Revision (R)
		public const ushort RCART0 = 0xFCB2; // RCART(R/W)
		public const ushort RCART1 = 0xFCB3; // RCART(R/W)
	}
}
