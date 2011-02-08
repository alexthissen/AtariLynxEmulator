using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class SuzyChipset : IMemoryAccess<ushort, byte>
	{
		public const int SUZYHARDWARE_READ = 5;
		public const int SUZYHARDWARE_WRITE = 5;
		public const int AVAILABLEHARDWARE_READWRITE = 5;

		public ulong PaintSprites() { return 0; }

		public void Poke(ushort address, byte value)
		{
			throw new NotImplementedException();
		}

		public byte Peek(ushort address)
		{
			throw new NotImplementedException();
		}
	}
}
