using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Processors
{
	public partial class Nmos6502
	{
		/// <summary>
		/// Arithmetic Shift Left
		/// </summary>
		/// <remarks>
		/// ASL shifts all bits left one position.
		/// 0 is shifted into bit 0 and the original bit 7 is shifted into the Carry. 
		/// </remarks>
		public void ASL()
		{
			FetchData();
			C = (Data & 0x80) == 0x80; // Set carry flag if bit 7 is set
			Data <<= 1; // Do shifting
			UpdateNegativeZeroFlags(Data);
			Memory.Poke(Address, Data);
			// Update data and write to address
			SystemClock.CycleCount += MemoryWriteCycle + 1;
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
		public void ASLA()
		{
			C = (A & 0x80) == 0x80;
			A <<= 1;
			SystemClock.CycleCount += 1;
			UpdateNegativeZeroFlags(A);
		}

		/// <summary>
		/// Logical Shift Right
		/// </summary>
		/// <remarks>
		/// LSR shifts all bits right one position. 0 is shifted into bit 7 and the original 
		/// bit 0 is shifted into the Carry. 
		/// </remarks>
		public void LSR()
		{
			FetchData();
			C = (Data & 0x01) == 0x01;
			Data >>= 1;
			Data &= 0x7f;
			Memory.Poke(Address, Data);
			// Update data and write to address
			SystemClock.CycleCount += MemoryWriteCycle + 1;
			UpdateNegativeZeroFlags(Data);
		}

		/// <summary>
		/// Logical Shift Right Accumulator
		/// </summary>
		/// <remarks>
		/// LSR shifts all bits right one position. 0 is shifted into bit 7 and the original 
		/// bit 0 is shifted into the Carry. 
		/// </remarks>
		public void LSRA()
		{
			C = (A & 0x01) == 0x01;
			A >>= 1;
			A &= 0x7f;
			// Update data 
			SystemClock.CycleCount += 1;
			UpdateNegativeZeroFlags(A);
		}

		/// <summary>
		/// ROtate Left
		/// </summary>
		/// <remarks>
		/// ROL shifts all bits left one position. The Carry is shifted into bit 0 and 
		/// the original bit 7 is shifted into the Carry. 
		/// </remarks>
		public void ROL()
		{
			FetchData();
			bool c = C;
			this.C = (Data & 0x80) == 0x80;
			Data <<= 1;
			Data |= c ? (byte)0x01 : (byte)0x00;
			Memory.Poke(Address, Data);

			// Update data and write to address
			SystemClock.CycleCount += MemoryWriteCycle + 1;
			UpdateNegativeZeroFlags(Data);
		}

		/// <summary>
		/// ROtate Left Accumulator
		/// </summary>
		/// <remarks>
		/// ROL shifts all bits left one position. The Carry is shifted into bit 0 and 
		/// the original bit 7 is shifted into the Carry. 
		/// </remarks>
		public void ROLA()
		{
			bool c = C;
			this.C = (A & 0x80) == 0x80;
			A <<= 1;
			A |= c ? (byte)1 : (byte)0;

			// Update data 
			SystemClock.CycleCount += 1;
			UpdateNegativeZeroFlags(A);
		}

		/// <summary>
		/// ROtate Right
		/// </summary>
		/// <remarks>
		/// ROR shifts all bits right one position. The Carry is shifted into bit 7 and the 
		/// original bit 0 is shifted into the Carry. 
		/// </remarks>
		public void ROR()
		{
			FetchData();
			bool c = C;
			C = (Data & 0x01) == 0x01;
			Data >>= 1;
			Data &= 0x7f;
			Data |= c ? (byte)0x80 : (byte)0x00;
			Memory.Poke(Address, Data);
			// Update data and write to address
			SystemClock.CycleCount += MemoryWriteCycle + 1;
			UpdateNegativeZeroFlags(Data);
		}

		/// <summary>
		/// ROtate Right Accumulator
		/// </summary>
		/// <remarks>
		/// ROR shifts all bits right one position. The Carry is shifted into bit 7 and the 
		/// original bit 0 is shifted into the Carry. 
		/// </remarks>
		public void RORA()
		{
			bool c = C;
			C = (A & 0x01) == 0x01;
			A >>= 1;
			A &= 0x7f;
			A |= c ? (byte)0x80 : (byte)0x00;
			// Update data 
			SystemClock.CycleCount += 1;
			UpdateNegativeZeroFlags(A);
		}
	}
}
