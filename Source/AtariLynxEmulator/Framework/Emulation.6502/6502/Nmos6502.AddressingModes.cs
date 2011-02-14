using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Processors
{
	public partial class Nmos6502
	{
		// $abcd
		public void Absolute()
		{
			Operand = Memory.PeekWord(PC);
			PC += 2;
		}

		// ($abcd)
		public void AbsoluteIndirectWithBug()
		{
			Operand = Memory.PeekWord(PC);
			PC += 2;
			ushort address = PC;
			if ((address & 0x00ff) == 0x00) address -= 0x0100;
			Operand = Memory.PeekWord(address);
		}

		// $abcd,X
		public void AbsoluteX()
		{
			Operand = (ushort)(Memory.PeekWord(PC) + X);
			PC += 2;
		}

		// ($abcd,X)
		public void AbsoluteIndirectX()
		{
			Operand = Memory.PeekWord(PC);
			PC += 2;
			Operand += X;
			Operand &= 0xffff;
			Operand = Memory.PeekWord(Operand);
		}

		// $abcd,Y
		public void AbsoluteY()
		{
			Operand = Memory.PeekWord(PC);
			Operand += Y;
			PC += 2;
		}

		// ($abcd,X)
		public void AbsoluteIndirectY()
		{
			Operand = Memory.PeekWord(PC);
			PC += 2;
			Operand += Y;
			Operand &= 0xffff;
			Operand = Memory.PeekWord(Operand);
		}

		public void Implied() { }

		// #42
		public void Immediate()
		{
			Operand = PC++;
		}

		// $ZP
		public void ZeroPage()
		{
			Operand = Memory.Peek(PC);
			PC++;
		}

		// $ZP,X
		public void ZeroPageX()
		{
			Operand = (ushort)((Memory.Peek(PC++) + X) & 0x00ff);
		}

		// $ZP,Y
		public void ZeroPageY()
		{
			Operand = (ushort)((Memory.Peek(PC++) + Y) & 0x00ff);
		}

		// ($ZP,X)
		public void ZeroPageIndexedIndirectX()
		{
			Operand = Memory.Peek(PC++);
			Operand += X;
			Operand &= 0x00ff;
		}

		// ($ZP), Y
		public void ZeroPageIndirectIndexedY()
		{
			Operand = Memory.Peek(PC++); // Read zero page address 
			Operand = (ushort)((Memory.PeekWord(Operand) + Y) & 0xffff); // Read higher byte of address
		}
	}
}
