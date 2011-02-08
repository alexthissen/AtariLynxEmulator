using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class SuzyChipset: IMemoryAccess16BitBus
	{
		public const int SUZYHARDWARE_READ = 5;
		public const int SUZYHARDWARE_WRITE = 5;
		public const int AVAILABLEHARDWARE_READWRITE = 5;

		public ulong PaintSprites() { return 0; }

		public void PokeByte(ushort address, byte value)
		{
			throw new NotImplementedException();
		}

		public byte PeekByte(ushort address)
		{
			throw new NotImplementedException();
		}

		public void PokeWord(ushort address, ushort value)
		{
			throw new NotImplementedException();
		}

		public ushort PeekWord(ushort address)
		{
			throw new NotImplementedException();
		}
	}
}
