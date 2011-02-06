using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class MikeyChipset: IMemoryAccess16BitBus
	{
		public const int AUDIO_DPRAM_READWRITE_MIN = 5;
		public const int AUDIO_DPRAM_READWRITE_MAX = 20;
		public const int COLORPALETTE_DPRAM_READWRITE = 5;
		public const int AVAILABLEHARDWARE_READWRITE = 5;

		public void Update()
		{

		}

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
