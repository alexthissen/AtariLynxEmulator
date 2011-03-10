using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	public interface ITimerLogic
	{
		void UpdateCurrentValue(ulong currentCycleCount);
		ulong PredictTimerEvent(ulong currentCycleCount);
		void Start(ulong cycleCount);
	}
}
