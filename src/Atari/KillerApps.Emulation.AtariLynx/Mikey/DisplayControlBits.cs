using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	public class DisplayControlBits
	{
		private byte byteData = 0;

		public DisplayControlBits(byte initialValue)
		{
			byteData = initialValue;
		}
		
		public byte ByteData { set { byteData = (byte)(value & 0x0F); } }

		public bool Color { get { return (byteData & ColorMask) == ColorMask; } }
		public bool FourBitMode { get { return (byteData & FourBitModeMask) == FourBitModeMask; } }
		public bool Flip { get { return (byteData & FlipMask) == FlipMask; } }
		public bool EnableVideoDma { get { return (byteData & EnableVideoDmaMask) == EnableVideoDmaMask; } }
		
		public const byte ColorMask = 0x08;
		public const byte FourBitModeMask = 0x04;
		public const byte FlipMask = 0x02;
		public const byte EnableVideoDmaMask = 0x01;
	}
}
