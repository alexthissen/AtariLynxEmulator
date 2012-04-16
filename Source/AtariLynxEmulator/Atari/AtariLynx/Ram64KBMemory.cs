using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class Ram64KBMemory: IDirectMemoryAccess<ushort, byte>, IResetable
	{
		public const byte DEFAULT_RAM_CONTENT = 0xFF;
		public const int MEMORY_SIZE = 0xFFFF + 1;

		private byte[] ram = null;

		public Ram64KBMemory()
		{
			ram = new byte[MEMORY_SIZE];
		}

		public Ram64KBMemory(byte[] memory)
		{
			if (memory == null)
				throw new ArgumentNullException("memory", "Memory cannot be null.");
			if (memory.Length != 0x10000)
				throw new ArgumentException("Length of memory should be 0x10000.", "memory");

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

		public void Reset()
		{
			for (int index = 0; index < MEMORY_SIZE; index++) ram[index] = DEFAULT_RAM_CONTENT;
		}
	}
}
