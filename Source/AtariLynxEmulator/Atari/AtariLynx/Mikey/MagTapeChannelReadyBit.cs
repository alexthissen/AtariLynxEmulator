using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class MagTapeChannelReadyBit
	{
		public byte ByteData
		{
			get { return (byte)(Edge ? EdgeMask : 0); }
		}

		// "B7=edge (1) Reset upon read."
		public bool Edge { get; internal set; }

		private const byte EdgeMask = 0x80;

		public MagTapeChannelReadyBit(byte value)
		{
			this.Edge = (value & EdgeMask) == EdgeMask;
		} 
	}
}
