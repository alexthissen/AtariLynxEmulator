﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class Ram64KBMemory: IMemoryAccess<ushort, byte>
	{
		public const int MEMORY_SIZE = 0xffff + 1;
		private byte[] memory = new byte[MEMORY_SIZE];

		public byte[] GetDirectAccess()
		{
			return memory;
		}

		public void Poke(ushort address, byte value)
		{
			memory[address] = value;
		}

		public byte Peek(ushort address)
		{
			return memory[address];
		}
	}
}