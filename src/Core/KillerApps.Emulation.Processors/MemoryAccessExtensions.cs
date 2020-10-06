using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Processors
{
	public static class MemoryAccessExtensions
	{
		public static ushort PeekWord(this IMemoryAccess<ushort, byte> memory, ushort address)
		{
			ushort value = memory.Peek(address++);
			value += (ushort)(memory.Peek(address) << 8);
			return value;
		}
	}
}
