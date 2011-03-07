using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public static class MikeyAddresses
	{
		public const ushort HTIMBKUP = 0xFD00; // "Timer 0 backup value"
		public const ushort TIM7CTLB = 0xFD1F; // "Timer 7 dynamic control"
		public const ushort INTRST = 0xFD80; // "INTRST.Interrupt Poll 0, (R/W)"
		public const ushort INTSET = 0xFD81; // "INTSET. Interrupt Poll 1, (R/W)"
		public const ushort SYSCTL1 = 0xFD87; // "SYSCTL1.Control Bits.(W)"
		public const ushort IODIR = 0xFD8A; // "Mikey Parallel I/O Data Direction (W)"
		public const ushort IODAT = 0xFD8B; // "Mikey Parallel Data (sort of a R/W)"
		public const ushort MIKEYSREV = 0xFD89; // "Mikey Software Revision, (W)"
		public const ushort SDONEACK = 0xFD90; // "Suzy Done Acknowledge, (W)"
		public const ushort CPUSLEEP = 0xFD91; // "CPU Bus Request Disable(W)"
		public const ushort DISPCTL = 0xFD92; // "Video Bus Request Enable, (W)"
		public const ushort PBKUP = 0xFD93; // "Magic 'P' count, (W)"
		public const ushort DISPADRL = 0xFD94; // "Start Address of Video Display, (W)"
		public const ushort DISPADRH  = 0xFD95; // "Start Address of Video Display, (W)"
	}
}
