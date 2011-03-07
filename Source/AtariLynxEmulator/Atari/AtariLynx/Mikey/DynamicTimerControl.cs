using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class DynamicTimerControl
	{
		public byte ByteData { get; set; }

		public bool TimerDone { get; set; }
		public bool LastClock { get; set; }
		public bool BorrowIn { get; set; }
		public bool BorrowOut { get; set; }
	}
}
