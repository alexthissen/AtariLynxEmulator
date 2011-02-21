using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Processors
{
	// TODO: Optimize zero page addressing to use memory directly instead of via Peek and Poke
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
			ushort address = Operand;
			Operand = Memory.Peek(address); // Read low byte of address

			// http://www.textfiles.com/apple/6502.bugs.txt
			// "On the 6502, JMP (abs) had a bug when the low byte of the operand was $FF, e.g. JMP ($12FF)."
			// "An indirect JMP (xxFF) will fail because the MSB will be fetched from address xx00 instead of page xx+1."
			if ((address & 0x00ff) == 0xff)
			{
				address -= 0x00ff;
			}
			else
			{
				address += 1;
			}
			Operand += (ushort)(Memory.Peek(address) << 8);
		}

		// $abcd,X
		public void AbsoluteX()
		{
			Operand = Memory.PeekWord(PC);
			Operand += X;
			PC += 2;
		}

		// ($abcd,X)
		public void AbsoluteIndirectX()
		{
			Operand = Memory.PeekWord(PC);
			Operand += X;
			Operand = Memory.PeekWord(Operand);
			PC += 2;
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
			Operand += Y;
			Operand = Memory.PeekWord(Operand);
			PC += 2;
		}

		public void Implied() { }

		// #42
		public void Immediate()
		{
			// TODO: Move immediate inside opcodes, as this is no actual addressing mode
			// Value of operand should be of type byte in these cases
			Operand = PC++;
		}

		// $ZP
		public void ZeroPage()
		{
			Operand = (ushort)Memory.Peek(PC++);
		}

		// $ZP,X
		public void ZeroPageX()
		{
			// "The address calculation wraps around if the sum of the base address and the register exceed $FF."
			byte address = Memory.Peek(PC++);
			address += X;
			Operand = (ushort)address;
		}

		// $ZP,Y
		public void ZeroPageY()
		{
			// "The address calculation wraps around if the sum of the base address and the register exceed $FF."
			byte address = Memory.Peek(PC++);
			address += Y;
			Operand = (ushort)address;
		}

		// ($ZP,X)
		public void ZeroPageIndexedIndirectX()
		{
			Operand = (ushort)Memory.Peek(PC++);

			// "The address is taken from the instruction and the X register added to it 
			// (with zero page wrap around) to give the location of the least significant byte of 
			// the target address."
			Operand += X;
			Operand &= 0x00ff;
			Operand = Memory.PeekWord(Operand);
		}

		// ($ZP), Y
		public void ZeroPageIndirectIndexedY()
		{
			// "The instruction contains the zero page location of the least significant byte of 
			// 16 bit address. The Y register is dynamically added to this value to generated the 
			// actual target address for operation."
			Operand = (ushort)Memory.Peek(PC++); // Read zero page address 
			Operand = Memory.PeekWord(Operand);
			Operand += Y;
		}
	}
}
