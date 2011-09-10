using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Processors
{
	public static class VectorAddresses
	{
		public const ushort BOOT_VECTOR = 0xFFFC;
		public const ushort IRQ_VECTOR = 0xFFFE;
		public const ushort NMI_VECTOR = 0xFFFA;
	}
}
