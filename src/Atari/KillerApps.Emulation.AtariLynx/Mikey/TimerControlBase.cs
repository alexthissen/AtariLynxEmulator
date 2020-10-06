using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	public class TimerControlBase
	{
		public byte ByteData { get; set; }

		public bool ResetTimerDone { get { return (ByteData & ResetTimerDoneMask) == ResetTimerDoneMask; } }
		public bool EnableReload { get { return (ByteData & EnableReloadMask) == EnableReloadMask; } }
		public bool EnableCount { get { return (ByteData & EnableCountMask) == EnableCountMask; } }

		public const byte ResetTimerDoneMask = 0x40;
		public const byte EnableReloadMask = 0x10;
		public const byte EnableCountMask = 0x08;

		// "There are 8 independent timers. Each has the same construction as an audio channel, 
		// a 3 bit source period selector and an 8 bit down counter with a backup register."
		public ClockSelect SourcePeriod
		{
			get { return (ClockSelect)(ByteData & 0x07); }
		}
	}
}
