using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Processors
{
	public partial class Nmos6502
	{
		/// <summary>
		/// Add with Carry
		/// </summary>
		/// <remarks>
		/// This instruction adds the contents of a memory location to the accumulator together 
		/// with the carry bit. If overflow occurs the carry bit is set, this enables multiple 
		/// byte addition to be performed.
		/// http://www.6502.org/tutorials/vflag.html#2.4
		/// </remarks>
		public void ADC()
		{
			int value = Memory.Peek(Operand);
			if (D) // Decimal mode
			{
				int c = C ? 1 : 0;
				int lo = (A & 0x0f) + (value & 0x0f) + c;
				int hi = (A & 0xf0) + (value & 0xf0);
				V = false;
				C = false;
				if (lo > 0x09)
				{
					hi += 0x10;
					lo += 0x06;
				}
				if ((~(A ^ value) & (A ^ hi) & 0x80) == 0x80) V = true;
				if (hi > 0x90) hi += 0x60;
				if ((hi & 0xff00) == 0xf00) C = true;
				A = (byte)((lo & 0x0f) + (hi & 0xf0));
			}
			else
			{
				int c = C ? 1 : 0;
				int sum = A + value + c;
				V = false;
				C = false;
				if ((~(A ^ value) & (A ^ sum) & 0x80) == 0x80) V = true;
				if ((sum & 0xff00) != 0) C = true;
				A = (byte)(sum & 0xff); // LX: Cap result to never be bigger than 0xff value
			}
			UpdateNegativeZeroFlags(A);
		}

		public void SBC()
		{
			int value = Memory.Peek(Operand);
			if (D)
			{
				int c = C ? 0 : 1;
				int sum = A - value - c;
				int lo = (A & 0x0f) - (value & 0x0f) - c;
				int hi = (A & 0xf0) - (value & 0xf0);
				V = false;
				C = false;
				if (((A ^ value) & (A ^ sum) & 0x80) != 0) V = true;
				if ((lo & 0xf0) == 0xf0) lo -= 6;
				if ((lo & 0x80) == 0x80) hi -= 0x10;
				if ((hi & 0x0f00) == 0x0f00) hi -= 0x60;
				if ((sum & 0xff00) == 0) C = true;
				A = (byte)((lo & 0x0f) + (hi & 0xf0));
			}
			else
			{
				int c = C ? 0 : 1;
				int sum = A - value - c;
				V = false;
				C = false;
				if (((A ^ value) & (A ^ sum) & 0x80) != 0) V = true;
				if ((sum & 0xff00) == 0) C = true;
				A = (byte)(sum & 0xff);
			}
			UpdateNegativeZeroFlags(A);
		}
	}
}
