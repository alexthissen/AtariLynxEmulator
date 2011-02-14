using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Processors
{
	public partial class Nmos6502
	{
		/// <summary>
		/// INcrement X
		/// </summary>
		public void INX()
		{
			X++;
			UpdateNegativeZeroFlags(X);
		}

		/// <summary>
		/// INcrement Y
		/// </summary>
		public void INY()
		{
			Y++;
			UpdateNegativeZeroFlags(Y);
		}

		/// <summary>
		/// LoaD X register
		/// </summary>
		/// <remarks>
		/// Loads a byte of memory into the X register setting the zero and negative flags as
		/// appropriate.
		/// </remarks>
		public void LDX()
		{
			X = Memory.Peek(Operand);
			UpdateNegativeZeroFlags(X);
		}

		/// <summary>
		/// Load Y Register
		/// </summary>
		/// <remarks>
		/// Loads a byte of memory into the Y register setting the zero and negative flags as 
		/// appropriate.
		/// </remarks>
		public void LDY()
		{
			Y = Memory.Peek(Operand);
			UpdateNegativeZeroFlags(Y);
		}

		/// <summary>
		/// STore X register
		/// </summary>
		public void STX()
		{
			Memory.Poke(Operand, X);
		}

		/// <summary>
		/// STore Y register
		/// </summary>
		public void STY()
		{
			Memory.Poke(Operand, Y);
		}

		/// <summary>
		/// Transfer A to X
		/// </summary>
		public void TAX()
		{
			X = A;
			UpdateNegativeZeroFlags(X);
		}

		/// <summary>
		/// Transfer A to Y
		/// </summary>
		public void TAY()
		{
			Y = A;
			UpdateNegativeZeroFlags(Y);
		}

		/// <summary>
		/// Transfer Stack pointer to X
		/// </summary>
		/// <remarks>
		/// Copies the current contents of the stack register into the X register and sets the 
		/// zero and negative flags as appropriate.
		/// </remarks>
		public void TSX()
		{
			X = SP;
			UpdateNegativeZeroFlags(X);
		}

		/// <summary>
		/// Transfer X to Accumulator
		/// </summary>
		/// <remarks>
		/// Copies the current contents of the X register into the accumulator and sets the 
		/// zero and negative flags as appropriate.
		/// </remarks>
		public void TXA()
		{
			A = X;
			UpdateNegativeZeroFlags(A);
		}

		/// <summary>
		/// Transfer X to processor Status
		/// </summary>
		/// <remarks>
		/// Copies the current contents of the X register into the stack register.
		/// </remarks>
		public void TXS()
		{
			SP = X;
		}

		/// <summary>
		/// Transfer Y to A
		/// </summary>
		public void TYA()
		{
			A = Y;
			UpdateNegativeZeroFlags(A);
		}

		/// <summary>
		/// DEcrease X register
		/// </summary>
		public void DEX()
		{
			X--;
			UpdateNegativeZeroFlags(X);
		}
			
		/// <summary>
		/// DEcrease Y register
		/// </summary>
		public void DEY()
		{
			Y--;
			UpdateNegativeZeroFlags(Y);
		}
	}
}
