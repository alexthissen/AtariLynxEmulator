using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Processors
{
	public partial class Cmos65SC02
	{
		// ($abcd)
		public void AbsoluteIndirect()
		{
			Operand = Memory.PeekWord(PC);
			PC += 2;
			Operand = Memory.PeekWord(Operand);
		}

		// Indirect zero page ($zp)
		private void ZeroPageIndirect()
		{
			Operand = Memory.Peek(PC);
			PC++;
			Operand = Memory.PeekWord(Operand);
		}

		// Absolute Indexed Indirect ($abcd,X)
		private void AbsoluteIndexedIndirectX()
		{
			Operand = Memory.PeekWord(PC);
			PC += 2;
			Operand += X;
			Operand = Memory.PeekWord(Operand);
		}
	}
}
