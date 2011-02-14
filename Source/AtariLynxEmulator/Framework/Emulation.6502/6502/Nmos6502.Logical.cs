﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Processors
{
	public partial class Nmos6502
	{
		/// <summary>
		/// Bitwise OR with Accumulator
		/// </summary>
		public void ORA()
		{
			A |= Memory.Peek(Operand);
			UpdateNegativeZeroFlags(A);
		}

		/// <summary>
		/// Arithmetic Shift Left Accumulator
		/// </summary>
		/// <remarks>
		/// This operation shifts all the bits of the accumulator or memory contents one bit 
		/// left. Bit 0 is set to 0 and bit 7 is placed in the carry flag. The effect of this 
		/// operation is to multiply the memory contents by 2 (ignoring 2's complement 
		/// considerations), setting the carry if the result will not fit in 8 bits.
		/// </remarks>
		private void ASLA()
		{
			C = (A & 0x80) == 0x80;
			A <<= 1;
			UpdateNegativeZeroFlags(A);
		}

		/// <summary>
		/// Bitwise logical AND with accumulator
		/// </summary>
		/// <remarks>
		/// A logical AND is performed, bit by bit, on the accumulator contents using the 
		/// contents of a byte of memory.
		/// </remarks>
		public void AND()
		{
			A &= Memory.Peek(Operand);
			UpdateNegativeZeroFlags(A);
		}

		/// <summary>
		/// CoMPare accumulator
		/// </summary>
		/// <remarks>
		/// Compare sets flags as if a subtraction had been carried out. If the value in the accumulator is 
		/// equal or greater than the compared value, the Carry will be set. 
		/// The equal (Z) and sign (S) flags will be set based on equality or lack thereof and 
		/// the sign (i.e. A>=$80) of the accumulator. 
		/// </remarks>
		public void CMP()
		{
			byte value = Memory.Peek(Operand);
			C = A >= value;
			UpdateNegativeZeroFlags((byte)(A - value));
		}

		/// <summary>
		/// ComPare X register
		/// </summary>
		/// <remarks>
		/// Operation and flag results are identical to equivalent mode accumulator CMP ops. 
		/// </remarks>
		public void CPX()
		{
			byte value = Memory.Peek(Operand);
			C = X >= value;
			UpdateNegativeZeroFlags((byte)(X - value));
		}

		/// <summary>
		/// ComPare Y register
		/// </summary>
		/// <remarks>
		/// Operation and flag results are identical to equivalent mode accumulator CMP ops. 
		/// </remarks>
		public void CPY()
		{
			byte value = Memory.Peek(Operand);
			C = Y >= value;
			UpdateNegativeZeroFlags((byte)(Y - value));
		}

		/// <summary>
		/// Test bits
		/// </summary>
		/// <remarks>
		/// This instructions is used to test if one or more bits are set in a target memory location. 
		/// The mask pattern in A is ANDed with the value in memory to set or clear the zero flag, but the 
		/// result is not kept. Bits 7 and 6 of the value from memory are copied into the N and V flags.
		/// </remarks>
		public void BIT()
		{
			byte value = Memory.Peek(Operand);
			value &= A;
			UpdateZeroFlag(value);
		}
	}
}