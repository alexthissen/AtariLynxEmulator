namespace KillerApps.Emulation.Processors
{
	public partial class Cmos65SC02
	{
		// ($abcd)
		public void AbsoluteIndirect()
		{
			Address = Memory.PeekWord(PC);
			PC += 2;
			Address = Memory.PeekWord(Address);

			// Fetch low byte, high byte, low byte, high byte
			// Additional clock cycle for fixing 6502 bug
			//SystemClock.CycleCount += 4 * MemoryReadCycle + 1;
		}

		// Indirect zero page ($zp)
		private void ZeroPageIndirect()
		{
			Address = Memory.Peek(PC);
			PC++;
			Address = Memory.PeekWord(Address);

			// Fetch low byte, low byte, high byte
			//SystemClock.CycleCount += 5 * MemoryReadCycle;
        }

		// Absolute Indexed Indirect ($abcd,X)
		private void AbsoluteIndexedIndirectX()
		{
			Address = Memory.PeekWord(PC);
			PC += 2;
			Address += X;
			Address = Memory.PeekWord(Address);

			// Fetch low byte, high byte, low byte, high byte
			// Additional clock cycle to fix page crossing?
			//SystemClock.CycleCount += 4 * MemoryReadCycle + 1;
		}
	}
}
