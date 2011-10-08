using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class TimerLogicFactory
	{
		public static ITimerLogic CreateTimerLogic(Timer owner, StaticTimerControl control, ITimerLogic currentTimerLogic)
		{
			// Timer 4 (UART for ComLynx) has different clocking logic
			if (owner.InterruptMask == 0x10)
			{
				// TODO: Check if UART timer logic needs to be initialized from current timer
				return new UartTimerLogic(owner);
			}

			if (control.SourcePeriod == ClockSelect.Linking)
			{
				return new LinkingTimerLogic(owner);
			}

			ClockedTimerLogic timer = new ClockedTimerLogic(owner);
			timer.InitializeFrom(currentTimerLogic as ClockedTimerLogic);
			return timer;
		}
	}
}
