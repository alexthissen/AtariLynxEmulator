using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	public class DynamicTimerControl
	{
		public DynamicTimerControl(byte initialValue)
		{
			this.ByteData = initialValue;
		}

		public byte ByteData { get; set; }

		public const byte TimerDoneMask = 0x08;
		public const byte LastClockMask = 0x04;
		public const byte BorrowInMask = 0x02;
		public const byte BorrowOutMask = 0x01;

		public bool TimerDone
		{
			get { return (ByteData & TimerDoneMask) == TimerDoneMask; }
			set
			{
				ByteData &= TimerDoneMask ^ 0xff;
				ByteData |= (byte)(value ? TimerDoneMask : 0);
			}
		}

		public bool LastClock { get; set; }
		public bool BorrowIn { get; set; }
		public bool BorrowOut { get; set; }
	}
}
