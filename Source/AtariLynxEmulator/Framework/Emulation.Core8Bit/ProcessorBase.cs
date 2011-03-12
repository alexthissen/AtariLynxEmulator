using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Core
{
	public abstract class ProcessorBase
	{
		public abstract void Initialize();
		public abstract void Reset();
		public abstract ulong Execute(int cyclesToExecute);
		public abstract object SignalInterrupt(InterruptType interrupt, params object[] details);
	}
}
