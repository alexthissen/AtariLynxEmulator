using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
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
