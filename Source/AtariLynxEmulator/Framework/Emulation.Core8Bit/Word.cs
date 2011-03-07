using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace KillerApps.Emulation.Core
{
	[StructLayout(LayoutKind.Explicit)]
	public struct Word
	{
		[FieldOffset(0)]
		public ushort Value;
		[FieldOffset(0)]
		public byte LowByte;
		[FieldOffset(1)]
		public byte HighByte;
	}
}
