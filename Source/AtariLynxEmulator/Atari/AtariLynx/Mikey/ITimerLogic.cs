using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	public interface ITimerLogic
	{
		bool UpdateCurrentValue(ulong currentCycleCount);
		ulong PredictExpirationTime(ulong currentCycleCount);
		void Start(ulong cycleCount);
	}
}
