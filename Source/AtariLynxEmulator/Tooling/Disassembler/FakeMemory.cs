using KillerApps.Emulation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Tooling.Disassembler
{
	public class RamMemory : IMemoryAccess<ushort, byte>
	{
		byte[] memory;

		public RamMemory(byte[] memory)
		{
			this.memory = memory;
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
