using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class StaticControlBits: TimerControlBase
	{
		public StaticControlBits(byte initialValue)
		{
			this.ByteData = initialValue;
		}

		public bool EnableInterrupt { get { return (ByteData & EnableInterruptMask) == EnableInterruptMask; } }
		public bool MagMode { get { return (ByteData & MagModeMask) == MagModeMask; } }

		public const byte EnableInterruptMask = 0x80;
		public const byte MagModeMask = 0x20;
	}
}