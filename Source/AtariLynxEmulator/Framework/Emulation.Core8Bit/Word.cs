using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace KillerApps.Emulation.Core
{
	[DebuggerDisplay("Word ({Value})")]
	[StructLayout(LayoutKind.Explicit)]
	[Serializable]
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
