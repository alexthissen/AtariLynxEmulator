using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KillerApps.Emulation.Atari.Lynx
{
	public static class BinaryReaderExtensions
	{
		public static ushort ReadUInt16(this BinaryReader reader, bool reversed)
		{
			if (!reversed) return reader.ReadUInt16();

			byte[] bytes = reader.ReadBytes(2);
			Array.Reverse(bytes);
			ushort value = BitConverter.ToUInt16(bytes, 0);
			return value;
		}
	}
}
