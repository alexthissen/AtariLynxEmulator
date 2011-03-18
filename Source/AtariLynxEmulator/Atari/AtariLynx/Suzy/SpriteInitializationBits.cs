using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class SpriteInitializationBits
	{
		private const byte Fc1Mask = 0x80;
		private const byte Fc2Mask = 0x40;
		private const byte Fc3Mask = 0x20;
		private const byte Ac1Mask = 0x08;
		private const byte Ac2Mask = 0x04;
		private const byte Ac3Mask = 0x02;
		private const byte Ac4Mask = 0x01;

		public SpriteInitializationBits(byte initialValue)
		{
			ByteData = initialValue;
		}

		// "B7 = fc1, B6 = fc2, B5 = fc3, B4 = reserved
		// B3 = ac1, B2 = ac2, B1 = ac3, B0 = ac4"
		public bool Fc1 { get { return (ByteData & Fc1Mask) == Fc1Mask; } }
		public bool Fc2 { get { return (ByteData & Fc2Mask) == Fc1Mask; } }
		public bool Fc3 { get { return (ByteData & Fc3Mask) == Fc1Mask; } }
		public bool Ac1 { get { return (ByteData & Ac1Mask) == Ac1Mask; } }
		public bool Ac2 { get { return (ByteData & Ac2Mask) == Ac2Mask; } }
		public bool Ac3 { get { return (ByteData & Ac3Mask) == Ac3Mask; } }
		public bool Ac4 { get { return (ByteData & Ac4Mask) == Ac4Mask; } }

		public byte ByteData { get; set; }
	}
}
