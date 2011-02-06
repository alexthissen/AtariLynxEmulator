using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class MemoryManagementUnit: IMemoryAccess16BitBus
	{
		public IMemoryAccess16BitBus Ram { get; private set; }
		public IMemoryAccess16BitBus Rom { get; private set; }
		public IMemoryAccess16BitBus Suzy { get; private set; }
		public IMemoryAccess16BitBus Mikey { get; private set; }

		public MemoryManagementUnit(IMemoryAccess16BitBus rom, IMemoryAccess16BitBus ram, IMemoryAccess16BitBus mikey, IMemoryAccess16BitBus suzy)
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
