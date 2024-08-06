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
			// pointed to by the BREAK vector is called after the processor status byte and the return 
			// address are pushed on the stack."
			PushOnStack((byte)(this.PC >> 8)); // Push high byte of program counter
			PushOnStack((byte)(this.PC & 0xff)); // Push low byte of program counter

			// "The only difference in the BRK instruction on the 65C02 and the 6502 is that the 65C02 clears 
			// the D (decimal) flag on the 65C02, whereas the D flag is not affected on the 6502. 
			// On both, the value of processor status (P) register is pushed onto the stack (after the high 
			// and low bytes of the return address have been pushed) with bit 4 (the B "flag") set (i.e. one), 
			// then the I flag is set."
			PushOnStack((byte)(ProcessorStatus | 0x10)); // Push processor status with B flag set

			// "BRK does set the interrupt-disable I flag like an IRQ does, and if you have the 
			// CMOS 6502 (65C02), it will also clear the decimal D flag."
			// "On the 65C02, the IRQ, NMI, and RESET hardware interrupts also clear the D flag 
			// (after pushing the P register)"
			
			// TODO: Override this method in Cmos65SC02
			// Nmos6502 does not reset the D-flag 
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
			ushort highByte = PullFromStack();
			highByte <<= 8;
			PC |= highByte;
		}

		/// <summary>
		/// ReTurn from Subroutine
		/// </summary>
		/// <remarks>
		/// The RTS instruction is used at the end of a subroutine to return to the calling 
		/// routine. It pulls the program counter (minus one) from the stack.
		/// </remarks>
		public void RTS()
		{
			ushort value;
			PC = PullFromStack();
			value = PullFromStack();
			value <<= 8;
			PC |= value;
			PC++;
		}

		/// <summary>
		/// JuMP
		/// </summary>
		/// <remarks>
		/// JMP transfers program execution to the following address (absolute) or to the 
		/// location contained in the following address (indirect). 
		/// An original 6502 has does not correctly fetch the target address if the indirect 
		/// vector falls on a page boundary (e.g. $xxFF where xx is and value from $00 to $FF). 
		/// In this case fetches the LSB from $xxFF as expected but takes the MSB from $xx00. 
		/// This is fixed in some later chips like the 65SC02 so for compatibility always 
		/// ensure the indirect vector is not at the end of the page.
		/// </remarks>
		public void JMP()
		{
			PC = Address;
		}

		/// <summary>
		/// Jump to SubRoutine
		/// </summary>
		/// <remarks>
		/// JSR pushes the address-1 of the next operation on to the stack before transferring 
		/// program control to the following address. Subroutines are normally terminated by 
		/// a RTS opcode. 
		/// </remarks>
		public void JSR()
		{
			ushort address = PC;
			address--;
			PushOnStack((byte)(address >> 8));
			PushOnStack((byte)(address & 0xff));
			PC = this.Address;
		}

		/// <summary>
		/// No OPeration
		/// </summary>
		/// <remarks>
		/// The NOP instruction causes no changes to the processor other than the normal 
		/// incrementing of the program counter to the next instruction.
		/// </remarks>
		public void NOP()
		{
		}

		/// <summary>
		/// DECrement memory
		/// </summary>
		public void DEC()
		{
			FetchData();
			Data--;
			Memory.Poke(Address, Data);

			// Increase data, write data to address 
			//SystemClock.CycleCount += MemoryWriteCycle + 1;
			UpdateNegativeZeroFlags(Data);
		}

		/// <summary>
		/// INCrement memory
		/// </summary>
		public void INC()
		{
			FetchData();
			Data++;
			Memory.Poke(Address, Data);

			// Increase data, Write data to address
			//SystemClock.CycleCount += MemoryWriteCycle + 1;
			UpdateNegativeZeroFlags(Data);
		}

		protected void UpdateNegativeZeroFlags(byte value)
		{
			N = (value & 0x80) == 0x80;
			Z = (value == 0);
		}

		protected void UpdateZeroFlag(byte value)
		{
			Z = (value == 0);
		}
	}
}