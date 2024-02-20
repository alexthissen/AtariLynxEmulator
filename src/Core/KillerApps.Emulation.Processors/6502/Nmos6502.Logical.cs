namespace KillerApps.Emulation.Processors
{
	public partial class Nmos6502
	{
		/// <summary>
		/// Bitwise Exclusive OR
		/// </summary>
		public void EOR()
		{
			FetchData();
			A ^= Data;
			UpdateNegativeZeroFlags(A);
		}

		/// <summary>
		/// Bitwise OR with Accumulator
		/// </summary>
		public void ORA()
		{
			FetchData();
			A |= Data;
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
			FetchData();
			A &= Data;
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
			FetchData();
			C = A >= Data;
			UpdateNegativeZeroFlags((byte)(A - Data));
		}

		/// <summary>
		/// ComPare X register
		/// </summary>
		/// <remarks>
		/// Operation and flag results are identical to equivalent mode accumulator CMP ops. 
		/// </remarks>
		public void CPX()
		{
			FetchData();
			C = X >= Data;
			UpdateNegativeZeroFlags((byte)(X - Data));
		}

		/// <summary>
		/// ComPare Y register
		/// </summary>
		/// <remarks>
		/// Operation and flag results are identical to equivalent mode accumulator CMP ops. 
		/// </remarks>
		public void CPY()
		{
			FetchData();
			C = Y >= Data;
			UpdateNegativeZeroFlags((byte)(Y - Data));
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
			FetchData();
			UpdateZeroFlag((byte)(Data & A));
			N = (Data & 0x80) == 0x80;
			V = (Data & 0x40) == 0x40;
		}
	}
}
