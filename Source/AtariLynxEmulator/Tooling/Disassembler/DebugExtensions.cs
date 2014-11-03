using KillerApps.Emulation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Processors
{
	public static class DebugExtensions
	{
		public static IEnumerable<Expression6502> Disassemble(this IMemoryAccess<ushort, byte> memory, ushort startAddress)
		{
			return Disassemble(memory, startAddress, ushort.MaxValue);
		}

		public static IEnumerable<Expression6502> Disassemble(this IMemoryAccess<ushort, byte> memory,
			ushort startAddress, ushort toAddress)
		{
			ushort address = startAddress;
			Disassembler6502 disassembler = new Disassembler6502();
			while (true)
			{
				Expression6502 expression = new Expression6502(memory, address);
				yield return expression;

				// Check whether to continue to next instruction
				if (toAddress - expression.ByteLength <= address)
					yield break;

				address += expression.ByteLength;
			}
		}
	}
}
