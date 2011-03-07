using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class ClockedTimer: TimerBase
	{
		// Cycle count at moment when current value was reached
		private ulong CycleCountCurrentValue;
		public ulong TimerEvent { get; private set; }
		
		public ClockedTimer(byte interruptMask, StaticTimerControl control): base(interruptMask)
		{
			StaticControlBits = control;
		}
		
		// LX: 16 MHz clock means that every cyclecount is 1/16th us (microsecond)
		// put another way: for every us there are 16 cyclecounts
		public ulong CyclesPerPeriod { get { return (ulong)(1 << Multiplier); } }

		// Divide cycle count by 16 will get number of us. Clock select of xxx1 address
		// is 1 us or higher powers of two. Shift right by multiplier additional bits from clock select 
		// to get number of units in clock from cyclecount. Shift left 4 + bits will multiply 
		// units on clock to cyclecount.
		public int Multiplier { get { return 1 + (int)SourcePeriod; } }

		protected override void UpdateCurrentValue(ulong currentCycleCount)
		{
			byte value = (byte)((currentCycleCount - CycleCountCurrentValue) >> Multiplier);
			if (CurrentValue != value)
			{
				CycleCountCurrentValue += (ulong)((value - CurrentValue) << Multiplier);
				CurrentValue = value;
				DynamicControlBits.BorrowIn = true;
			}

			// Calculate new timer event
			TimerEvent = (ulong)(((CurrentValue & 0x80000000) == 0x80000000) ? 1 : ((CurrentValue + 1) << Multiplier)) + currentCycleCount;
		}

		public override void Start(ulong cycleCount)
		{
			CycleCountCurrentValue = cycleCount;
		}
	}
}
