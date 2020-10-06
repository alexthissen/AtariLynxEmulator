using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	public class TimerExpirationEventArgs: EventArgs
	{
		public TimerExpirationEventArgs(byte interruptMask)
		{
			this.InterruptMask = interruptMask;
		}

		public byte InterruptMask { get; private set; }
		public ulong CyclesInterrupt { get; set; }
	}
}
