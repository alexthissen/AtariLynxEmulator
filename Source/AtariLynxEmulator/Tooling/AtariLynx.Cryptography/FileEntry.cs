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
		public const int ROMDIR_PAGE = 0;
		public const int ROMDIR_OFFSET = 1;
		public const int ROMDIR_FLAG = 3;
		public const int ROMDIR_DEST = 4;
		public const int ROMDIR_SIZE = 6;
		public const int ROMDIR_ENTRY_SIZE = 8;

		[FieldOffset(ROMDIR_PAGE)] public byte PageOffset;
		[FieldOffset(ROMDIR_OFFSET)] public ushort ByteOffset;
		[FieldOffset(ROMDIR_FLAG)] public byte Flag;
		[FieldOffset(ROMDIR_DEST)] public ushort RamDestination;
		[FieldOffset(ROMDIR_SIZE)] public ushort FileSize;

		public static FileEntry FromByteArray(byte[] data)
		{
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			IntPtr ptr = handle.AddrOfPinnedObject();
			FileEntry entry = (FileEntry)Marshal.PtrToStructure(ptr, typeof(FileEntry));
			if (handle.IsAllocated)
			{
				handle.Free();
			}
			return entry;
		}

		public byte[] ToByteArray()
		{
			byte[] data = new byte[Marshal.SizeOf(typeof(FileEntry))];
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			IntPtr ptr = handle.AddrOfPinnedObject();
			Marshal.StructureToPtr(this, ptr, true);
			if (handle.IsAllocated)
			{
				handle.Free();
			}
			return data;
		}

	}
}
