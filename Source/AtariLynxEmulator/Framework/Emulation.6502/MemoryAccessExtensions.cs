using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;

namespace KillerApps.Emulation.Processors
{
	public static class MemoryAccessExtensions
	{
		public static ushort PeekWord(this IMemoryAccess<ushort, byte> memory, ushort address)
		{
			return (ushort)(memory.Peek(address) + memory.Peek(address) << 8);
		}
	}
}
