using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Processors
{
	public partial class Cmos65SC02
	{
		/// <summary>
		/// Test and Reset Bit
		/// </summary>
		/// <remarks>
		/// TSB, like TRB, has the same effect on the Z flag that a BIT instruction does. 
		/// Specifically, it is based on whether the result of a bitwise AND of the accumulator with the 
		/// contents of the memory location specified in the operand is zero. 
		/// Also, like BIT (and TRB), the accumulator is not affected. 
		/// </remarks>
		public void TRB()
		{
			byte value = Memory.Peek(Operand);
			value &= A;
			UpdateZeroFlag(value);
			value &= (byte)(A ^ 0xff);
			Memory.Poke(Operand, value);
		}

		/// <summary>
		/// Test and Set Bits
		/// </summary>
		/// <remarks>
		/// TSB, like TRB, has the same effect on the Z flag that a BIT instruction does. 
		/// Specifically, it is based on whether the result of a bitwise AND of the accumulator with the 
		/// contents of the memory location specified in the operand is zero. 
		/// Also, like BIT (and TRB), the accumulator is not affected. 
		/// </remarks>
		public void TSB()
		{
			byte value = Memory.Peek(Operand);
			UpdateZeroFlag((byte)(value & A));
			value |= A;
			Memory.Poke(Operand, value);
		}

		public void BRA()
		{
			sbyte offset = (sbyte)Memory.Peek(PC);
			PC++;
			PC = (ushort)(PC + offset);
		}

		/// <summary>
		/// PusH X register
		/// </summary>
		/// <remarks>
		/// Pushes a copy of the X register on to the stack.
		/// </remarks>
		public void PHX()
		{
			PushOnStack(X);
		}

		/// <summary>
		/// STore Zero
		/// </summary>
		/// <remarks>
		/// STZ is fairly straightforward. It stores $00 in the memory location specified in the operand.
		/// </remarks>
		public void STZ()
		{
			Memory.Poke(Operand, 0);
		}

		/// <summary>
		/// WAit for Interrupt 
		/// </summary>
		public void WAI()
		{
			IsAsleep = true;
		}

		/// <summary>
		/// STop the Processor 
		/// </summary>
		/// <remarks>
		/// STP stops the clock input of the 65C02, effectively shutting down the 65C02 until a hardware 
		/// reset occurs (i.e. the RES pin goes low). 
		/// This puts the 65C02 into a low power state. This is useful for applications (circuits) that 
		/// require low power consumption, but STP is rarely seen otherwise. 
		/// </remarks>
		public void STP()
		{
			IsAsleep = true;
		}

		/// <summary>
		/// PuLl Accumulator
		/// </summary>
		/// <remarks>
		/// Pulls an 8 bit value from the stack and into the X register. The zero and negative 
		/// flags are set as appropriate.
		/// </remarks>
		public void PLX()
		{
			X = PullFromStack();
			UpdateNegativeZeroFlags(X);
		}

		/// <summary>
		/// Test bits
		/// </summary>
		/// <remarks>
		/// This instructions is used to test if one or more bits are set in a target memory location. 
		/// The mask pattern in A is ANDed with the value in memory to set or clear the zero flag, but the 
		/// result is not kept. Bits 7 and 6 of the value from memory are copied into the N and V flags.
		/// </remarks>
		public void BIT2()
		{
			byte value = Memory.Peek(Operand);
			value &= A;
			UpdateZeroFlag(value);

			// "BIT has three additional addressing modes. ... The immediate addressing mode only affects the Z flag."
			if (Opcode != 0x89)
			{
				N = (value & 0x80) == 0x80;
				V = (value & 0x40) == 0x40;
			}
		}
	}
}
