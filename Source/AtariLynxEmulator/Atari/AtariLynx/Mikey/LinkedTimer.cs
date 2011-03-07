using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class LinkedTimer: TimerBase
	{
		public TimerBase PreviousTimer { get; set; }
		
		// TODO: Find out what last link carrying is meant for
		public bool LastLinkCarry { get; private set; }

		public LinkedTimer(byte interruptMask, StaticTimerControl control): base(interruptMask)
		{
			StaticControlBits = control;
		}

		protected override void UpdateCurrentValue(ulong currentCycleCount)
		{
			if (PreviousTimer.DynamicControlBits.BorrowOut)
			{
				CurrentValue--;
				DynamicControlBits.BorrowIn = true;
			}
			
			// TODO: Find out what last link carrying is meant for
			LastLinkCarry = PreviousTimer.DynamicControlBits.BorrowOut;
		}

		public override void Start(ulong cycleCount)
		{
			// Nothing to do for linked timers
		}
	}
}
