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
		private void ASL()
		{
			byte value = Memory.Peek(Operand);
			C = (value & 0x80) == 0x80; // Set carry flag if bit 7 is set
			value <<= 1; // Do shifting
			UpdateNegativeZeroFlags(value);
			Memory.Poke(Operand, value);
		}

		/// <summary>
		/// Bitwise Exclusive OR
		/// </summary>
		public void EOR()
		{
			A ^= Memory.Peek(Operand);
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
			byte value = Memory.Peek(Operand);
			C = (value & 0x01) == 0x01;
			value >>= 1;
			value &= 0x7f;
			Memory.Poke(Operand, value);
			UpdateNegativeZeroFlags(value);
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
			byte value = Memory.Peek(Operand);
			bool c = C;
			this.C = (value & 0x80) == 0x80;
			value <<= 1;
			value |= c ? (byte)0x01 : (byte)0x00;
			Memory.Poke(Operand, value);
			UpdateNegativeZeroFlags(value);
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
			byte value = Memory.Peek(Operand);
			bool c = C;
			C = (value & 0x01) == 0x01;
			value >>= 1;
			value &= 0x7f;
			value |= c ? (byte)0x80 : (byte)0x00;
			Memory.Poke(Operand, value);
			UpdateNegativeZeroFlags(value);
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
			UpdateNegativeZeroFlags(A);
		}
	}
}
