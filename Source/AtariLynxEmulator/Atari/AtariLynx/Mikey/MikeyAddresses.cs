using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public static class MikeyAddresses
	{
		public const ushort INTRST = 0xfd80; // "INTRST.Interrupt Poll 0, (R/W)"
		public const ushort INTSET = 0xfd81; // "INTSET. Interrupt Poll 1, (R/W)"
		public const ushort SYSCTL1 = 0xfd87; // "SYSCTL1.Control Bits.(W)"
		public const ushort IODIR = 0xfd8a; // "Mikey Parallel I/O Data Direction (W)"
		public const ushort IODAT = 0xfd8b; // "Mikey Parallel Data (sort of a R/W)"
		public const ushort SDONEACK = 0xfd90; // "Suzy Done Acknowledge, (W)"
		public const ushort CPUSLEEP = 0xfd91; // "CPU Bus Request Disable(W)"
	}
}
