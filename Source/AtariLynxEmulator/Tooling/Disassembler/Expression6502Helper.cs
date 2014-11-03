using KillerApps.Emulation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Processors
{
	public static class Expression6502Helper
	{
		public static ushort CalculateBranchAddress(ushort address, byte offset)
		{
			ushort jump = (ushort)(address + 2 + offset);
			if (offset > 0x80) jump -= 256;
			return jump;
		}

		public static ushort CalculateBranchAddress(Expression6502 expression)
		{
			if (expression.Instruction.AddressingMode != AddressingMode.Relative)
				throw new ArgumentException("Expression has a non-branching instruction");
			return CalculateBranchAddress(expression.Address, (byte)expression.Operand.Value);
		}

		public static ushort CalculateIndirectJumpAddress(IMemoryAccess<ushort, byte> memory, ushort address, byte? X = null)
		{
			if (X.HasValue) address += X.Value;
			return memory.PeekWord(address);
		}
	}
}
