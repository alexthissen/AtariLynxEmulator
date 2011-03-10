using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class TimerExpirationEventArgs: EventArgs
	{
		public byte InterruptMask { get; set; }
	}
}
