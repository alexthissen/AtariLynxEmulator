using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class AudioIn
	{
		public AudioIn(byte data)
		{
			this.AudioInComparator = (data & AudioInComparatorMask) == AudioInComparatorMask;
		}
		public bool AudioInComparator { get; private set; }

		private const byte AudioInComparatorMask = 0x80;
	}
}
