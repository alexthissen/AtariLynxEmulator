using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Core
{
	public interface IMemoryAccess16BitBus
	{
		void PokeByte(ushort address, byte value);
		byte PeekByte(ushort address);
		void PokeWord(ushort address, ushort value);
		ushort PeekWord(ushort address);
	}
}
