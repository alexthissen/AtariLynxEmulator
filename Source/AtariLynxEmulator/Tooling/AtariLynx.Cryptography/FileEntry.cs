using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx.Cryptography
{
	[StructLayout(LayoutKind.Explicit)]
	[Serializable]
	public struct FileEntry
	{
		[FieldOffset(0)] public byte PageOffset;
		[FieldOffset(1)] public ushort ByteOffset;
		[FieldOffset(3)] public byte Flag;
		[FieldOffset(4)] public ushort RamDestination;
		[FieldOffset(6)] public ushort FileSize;
	}
}
