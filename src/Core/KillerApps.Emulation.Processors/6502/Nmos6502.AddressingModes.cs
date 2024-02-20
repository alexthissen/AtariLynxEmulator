namespace KillerApps.Emulation.Processors
{
	// TODO: Optimize zero page addressing to use memory directly instead of via Peek and Poke
	// TODO: Move timings into memory to allow for different cycles per memory access
	public partial class Nmos6502
	{
		// $abcd
		public void Absolute()
		{
			Address = Memory.PeekWord(PC);
			PC += 2;
			
			// Fetch low byte, high byte
			//SystemClock.CycleCount += 2 * MemoryReadCycle;
		}

		// ($abcd)
		public void AbsoluteIndirectWithBug()
		{
			Address = Memory.PeekWord(PC);
			PC += 2;
			ushort address = Address;
			Address = Memory.Peek(address); // Read low byte of address

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
			Address |= (ushort)(Memory.Peek(address) << 8);

			// Fetch low byte, high byte of indirect address, low byte, high byte of direct address
			//SystemClock.CycleCount += 4 * MemoryReadCycle;
		}

		// $abcd,X
		public void AbsoluteX()
		{
			Address = Memory.PeekWord(PC);
			PC += 2;
			Address += X;
			
			// Fetch low byte, high byte
			// Addition of X to low byte is performed during fetch of high byte
			// TODO: Increase clock cycles by one if page boundary is crossed
			//SystemClock.CycleCount += 2 * MemoryReadCycle;
		}

		// $abcd,Y
		public void AbsoluteY()
		{
			Address = Memory.PeekWord(PC);
			Address += Y;
			PC += 2;

			// Fetch low byte, high byte
			// Addition of Y to low byte is performed during fetch of high byte
			// TODO: Increase clock cycles by one if page boundary is crossed
			//SystemClock.CycleCount += 2 * MemoryReadCycle;
		}

		// #42
		public void Immediate()
		{
			// Value of operand should be of type byte in these cases
			Address = PC++;
			
			// No cycle update here
		}

		// $ZP
		public void ZeroPage()
		{
			Address = (ushort)Memory.Peek(PC++);

			// Fetch low byte
			//SystemClock.CycleCount += MemoryReadCycle;
		}

		// $ZP,X
		public void ZeroPageX()
		{
			// "The address calculation wraps around if the sum of the base address and the register exceed $FF."
			Address = Memory.Peek(PC++);
			Address += X;
			Address &= 0x00ff;
			
			// Fetch low byte plus addition
			//SystemClock.CycleCount += MemoryReadCycle + 1;
		}

		// $ZP,Y
		public void ZeroPageY()
		{
			// "The address calculation wraps around if the sum of the base address and the register exceed $FF."
			Address = Memory.Peek(PC++);
			Address += Y;
			Address &= 0x00ff;
			
			// Fetch low byte plus addition
			//SystemClock.CycleCount += MemoryReadCycle + 1;
		}

		// ($ZP,X)
		public void ZeroPageIndexedIndirectX()
		{
			Address = (ushort)Memory.Peek(PC++);

			// "The address is taken from the instruction and the X register added to it 
			// (with zero page wrap around) to give the location of the least significant byte of 
			// the target address."
			Address += X;
			Address &= 0x00ff;
			Address = Memory.PeekWord(Address);
			
			// Fetch low byte, perform addition (1 clock cycle), fetch low byte, high byte
			// Addition can only be performed after address is fetched and before indirection is made,
			// so an additional clock cycle is spent
			//SystemClock.CycleCount += 3 * MemoryReadCycle + 1;
		}

		// ($ZP), Y
		public void ZeroPageIndirectIndexedY()
		{
			// "The instruction contains the zero page location of the least significant byte of 
			// 16 bit address. The Y register is dynamically added to this value to generated the 
			// actual target address for operation."
			Address = (ushort)Memory.Peek(PC++); // Read zero page address 
			Address = Memory.PeekWord(Address);
			Address += Y;
			
			// Fetch low byte zp, low byte, high byte
			// Addition is performed on low byte during fetch of high byte, so no clock cycle is spent
			// TODO: Increase clock cycles by one if page boundary is crossed
			//SystemClock.CycleCount += 3 * MemoryReadCycle;
		}
	}
}
