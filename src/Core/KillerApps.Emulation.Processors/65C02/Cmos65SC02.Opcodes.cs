namespace KillerApps.Emulation.Processors
{
	public partial class Cmos65SC02
	{
		/// <summary>
		/// Test and Reset Bit
		/// </summary>
		/// <remarks>
		/// Logically AND together the complement of the value in the accumulator with the data at the 
		/// effective address specified by the operand. Store the result at the memory location
		/// TSB, like TRB, has the same effect on the Z flag that a BIT instruction does. 
		/// Specifically, it is based on whether the result of a bitwise AND of the accumulator with the 
		/// contents of the memory location specified in the operand is zero. 
		/// Also, like BIT (and TRB), the accumulator is not affected. 
		/// </remarks>
		public void TRB()
		{
			FetchData();
			UpdateZeroFlag((byte)(Data & A));
			Data &= (byte)(A ^ 0xff);
			Memory.Poke(Address, Data);
		}

		/// <summary>
		/// Test and Set Bits
		/// </summary>
		/// <remarks>
		/// Logically OR together the value in the accumulator with the data at the effective address 
		/// specified by the operand. Store the result at the memory location.
		/// TSB, like TRB, has the same effect on the Z flag that a BIT instruction does. 
		/// Specifically, it is based on whether the result of a bitwise AND of the accumulator with the 
		/// contents of the memory location specified in the operand is zero. 
		/// Also, like BIT (and TRB), the accumulator is not affected. 
		/// </remarks>
		public void TSB()
		{
			FetchData();
			UpdateZeroFlag((byte)(Data & A));
			Data |= A;
			Memory.Poke(Address, Data);
		}

		public void BRA()
		{
			Address = PC++;
			FetchData();
			sbyte offset = (sbyte)Data;
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
			Memory.Poke(Address, 0);
			//SystemClock.CycleCount += MemoryWriteCycle;
		}

		/// <summary>
		/// WAit for Interrupt 
		/// </summary>
		public void WAI()
		{
			IsAsleep = true;
			//SystemClock.CycleCount += 2;
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
			//SystemClock.CycleCount += 2;
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
		/// result is not kept.
		/// </remarks>
		public void BITImmediate()
		{
			FetchData();

			// "BIT has three additional addressing modes. ... The immediate addressing mode only affects the Z flag."
			UpdateZeroFlag((byte)(Data & A));
		}
	}
}
