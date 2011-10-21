using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class ShiftRegisterFeedbackEnable
	{
		public ShiftRegisterFeedbackEnable(byte data)
		{
			ByteData = data;
		}

		// "B7= feedback bit 11
		// B6=feedback bit 10
		// B5=feedback bit 5
		// B4=feedback bit 4
		// B3=feedback bit 3
		// B2=feedback bit 2
		// B1=feedback bit 1
		// B0=feedback bit 0"

		public bool FeedbackBit11 { get { return (ByteData & FeedbackBit11Mask) == FeedbackBit11Mask; } }
		public bool FeedbackBit10 { get { return (ByteData & FeedbackBit10Mask) == FeedbackBit10Mask; } }
		public bool FeedbackBit5 { get { return (ByteData & FeedbackBit5Mask) == FeedbackBit5Mask; } }
		public bool FeedbackBit4 { get { return (ByteData & FeedbackBit4Mask) == FeedbackBit4Mask; } }
		public bool FeedbackBit3 { get { return (ByteData & FeedbackBit3Mask) == FeedbackBit3Mask; } }
		public bool FeedbackBit2 { get { return (ByteData & FeedbackBit2Mask) == FeedbackBit2Mask; } }
		public bool FeedbackBit1 { get { return (ByteData & FeedbackBit1Mask) == FeedbackBit1Mask; } }
		public bool FeedbackBit0 { get { return (ByteData & FeedbackBit0Mask) == FeedbackBit0Mask; } }

		public byte ByteData { get; set; }

		private const byte FeedbackBit11Mask = 0x80;
		private const byte FeedbackBit10Mask = 0x40;
		private const byte FeedbackBit5Mask = 0x20;
		private const byte FeedbackBit4Mask = 0x10;
		private const byte FeedbackBit3Mask = 0x08;
		private const byte FeedbackBit2Mask = 0x04;
		private const byte FeedbackBit1Mask = 0x02;
		private const byte FeedbackBit0Mask = 0x01;
	}
}
