using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class TimerLogicFactory
	{
		public static ITimerLogic CreateTimerLogic(TimerBase owner, ClockSelect sourcePeriod, ITimerLogic currentTimerLogic)
		{
			if (sourcePeriod == ClockSelect.Linking)
			{
				return new LinkingTimerLogic(owner);
			}

			ClockedTimerLogic timer = new ClockedTimerLogic(owner);
			timer.InitializeFrom(currentTimerLogic as ClockedTimerLogic);
			return timer;
		}
	}
}
