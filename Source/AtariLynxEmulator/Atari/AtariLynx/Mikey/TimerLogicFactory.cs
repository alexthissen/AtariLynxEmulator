using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class TimerLogicFactory
	{
		public static ITimerLogic CreateTimerLogic(Timer owner, StaticTimerControl control)
		{
			if (control.SourcePeriod == ClockSelect.Linking)
			{
				return new LinkingTimerLogic(owner);
			}

			return new ClockedTimerLogic(owner);
		}
	}
}
