using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	internal class ClockedTimerLogic: ITimerLogic
	{
		// Cycle count at moment when current value was reached
		private ulong CycleCountCurrentValue;
		protected Timer Owner;
		
		public ClockedTimerLogic(Timer owner)
		{
			this.Owner = owner;
		}
	
		// LX: 16 MHz clock means that every cyclecount is 1/16th us (microsecond)
		// put another way: for every us there are 16 cyclecounts
		public ulong CyclesPerPeriod { get { return (ulong)(1 << Multiplier); } }

		// Divide cycle count by 16 will get number of us. Clock select of xxx1 address
		// is 1 us or higher powers of two. Shift right by multiplier additional bits from clock select 
		// to get number of units in clock from cyclecount. Shift left 4 + bits will multiply 
		// units on clock to cyclecount.
		public virtual int Multiplier { get { return 4 + (int)Owner.StaticControlBits.SourcePeriod; } }

		public bool UpdateCurrentValue(ulong currentCycleCount)
		{
			byte value = CalculateValueDecrease(currentCycleCount);

			if (value > 0)
			{
				// TODO: Find out if Borrow-in is always set on count or only for clocked timers when being triggered
				Owner.DynamicControlBits.BorrowIn = true;

				// "As with the audio channels, a count of 0 is valid for 1 full cycle of the selected clock."
				if (Owner.CurrentValue < value)
				{
					CycleCountCurrentValue += (ulong)((Owner.CurrentValue + 1) << Multiplier);
					Owner.CurrentValue = 0; // Actually -1 at this point. Optional reloading happens from timer

					// Indicate expiration
					return true;
				}
				else
				{
					CycleCountCurrentValue += (ulong)(value << Multiplier);
					Owner.CurrentValue -= value;
				}
			}
			return false;
		}

		private byte CalculateValueDecrease(ulong currentCycleCount)
		{
			return (byte)((currentCycleCount - CycleCountCurrentValue) >> Multiplier);
		}

		public void Start(ulong cycleCount)
		{
			CycleCountCurrentValue = cycleCount;
		}

		public ulong PredictExpirationTime(ulong currentCycleCount)
		{
			// Negative current value means an immediate timer event
			byte value = CalculateValueDecrease(currentCycleCount);
			//return (ulong)(value > Owner.CurrentValue ? 1 : ((Owner.CurrentValue + 1) << Multiplier)) + CycleCountCurrentValue;

			// For now use Wilkins logic and predict new timer event from current system cycle count
			return (ulong)(value > Owner.CurrentValue ? 1 : ((Owner.CurrentValue + 1 - value) << Multiplier)) + currentCycleCount;
		}

		internal void InitializeFrom(ClockedTimerLogic currentTimerLogic)
		{
			if (currentTimerLogic == null) return;
			this.CycleCountCurrentValue = currentTimerLogic.CycleCountCurrentValue;
		}
	}
}
