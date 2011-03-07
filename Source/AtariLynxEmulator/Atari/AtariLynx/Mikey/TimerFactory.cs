using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class TimerFactory
	{
		public static TimerBase CreateTimer(byte interruptMask, StaticTimerControl control)
		{
			if (control.SourcePeriod == ClockSelect.Linking)
			{
				return new LinkedTimer(interruptMask, control);
			}

			return new ClockedTimer(interruptMask, control);
		}
	}
}
