using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	public class AudioControlBits : TimerControlBase
	{
		public AudioControlBits(byte initialValue)
		{
			this.ByteData = initialValue;
		}

		public bool FeedbackBit7 { get { return (ByteData & FeedbackBit7Mask) == FeedbackBit7Mask; } }

		// "1 bit of waveshape selector."
		public bool EnableIntegrateMode { get { return (ByteData & EnableIntegrateModeMask) == EnableIntegrateModeMask; } }

		public const byte FeedbackBit7Mask = 0x80;
		public const byte EnableIntegrateModeMask = 0x20;
	}
}