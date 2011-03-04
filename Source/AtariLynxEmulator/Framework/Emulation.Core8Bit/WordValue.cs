using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace KillerApps.Emulation.Core
{
	[StructLayout(LayoutKind.Explicit)]
	public struct WordValue
	{
		[FieldOffset(0)]
		public ushort Word;
		[FieldOffset(0)]
		public byte LowByte;
		[FieldOffset(1)]
		public byte HighByte;
	}
}
