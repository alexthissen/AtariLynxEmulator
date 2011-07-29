using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class Ram64KBMemory: IMemoryAccess<ushort, byte>
	{
		public const int MEMORY_SIZE = 0xffff + 1;
		private byte[] ram = null;

		public Ram64KBMemory()
		{
			ram = new byte[MEMORY_SIZE];
		}

		public Ram64KBMemory(byte[] memory)
		{
			Debug.Assert(memory != null);
			Debug.Assert(memory.Length == 0x10000);
			ram = memory;
		}

		public byte[] GetDirectAccess()
		{
			return ram;
		}

		public void Poke(ushort address, byte value)
		{
			ram[address] = value;
		}

		public byte Peek(ushort address)
		{
			return ram[address];
		}
	}
}
