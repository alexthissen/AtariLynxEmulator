using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Processors
{
	public partial class Nmos6502
	{
		/// <summary>
		/// Force Interrupt
		/// </summary>
		/// <remarks>
		/// BRK causes a non-maskable interrupt and increments the program counter by one. 
		/// Therefore an RTI will go to the address of the BRK +2 so that BRK may be used to replace a two-byte instruction 
		/// for debugging and the subsequent RTI will be correct.
		/// More information at http://www.6502.org/tutorials/interrupts.html#2.2
		/// </remarks>
		public void BRK()
		{
			// Increase program counter one extra
			PC++;

			// "When a hardware interrupt occurs or the CPU executes a BRK instruction, the software routine 
			// pointed to by the BREAK vector is called atter the processor status byte and the return 
			// address are pushed on the stack."
			PushOnStack((byte)(this.PC >> 8)); // Push high byte of program counter
			PushOnStack((byte)(this.PC & 0xff)); // Push low byte of program counter
			PushOnStack((byte)(this.ProcessorStatus | 0x10)); // Push processor status with Break set

			// "BRK does set the interrupt-disable I flag like an IRQ does, and if you have the 
			// CMOS 6502 (65C02), it will also clear the decimal D flag. "
			this.D = false;
			this.I = true;

			// Set program counter to value at interrupt vector address
			// "The vector used is at $FFFE-$FFFF, the same one used by IRQ."
			this.PC = Memory.PeekWord(VectorAddresses.IRQ_VECTOR);
		}

		/// <summary>
		/// ReTurn from Interrupt
		/// </summary>
		/// <remarks>
		/// RTI retrieves the Processor Status Word (flags) and the Program Counter from the 
		/// stack in that order (interrupts push the PC first and then the PS). 
		/// Note that unlike RTS, the return address on the stack is the actual address rather 
		/// than the address-1.
		/// </remarks>
		public void RTI()
		{
			// "RTI takes 6 clocks and does the reverse process to put the program counter and the processor status register back."
			byte status = PullFromStack();
			ProcessorStatus = status;
			
			// "The ISR's RTI is similar to the subroutine's RTS. The primary difference is that RTI 
			// restores the status register P too, not just the address to get back to."
			PC = PullFromStack();
			byte highByte = PullFromStack();
			PC |= (byte)(highByte << 8);
		}
	}
}