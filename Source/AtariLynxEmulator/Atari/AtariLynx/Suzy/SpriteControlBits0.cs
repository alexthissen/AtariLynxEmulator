using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class SpriteControlBits0
	{
		public SpriteControlBits0(byte sprctl0)
		{
			this.ByteData = sprctl0;
		}

		public byte ByteData { get; private set; }

		// "B2,B1,B0 = Sprite Type"
		public SpriteTypes SpriteType 
		{
			get { return (SpriteTypes)(ByteData & 0x07); }
		}

		// "Sprites can be horizontally and/or vertically flipped."
		// "B5 = H flip, 0 = not flipped"
		public bool HFlip 
		{
			get { return (ByteData & HFlipMask) == HFlipMask; }
		}

		// "Sprites can be horizontally and/or vertically flipped."
		// "B4 = V flip, 0 = not flipped"
		public bool VFlip
		{
			get { return (ByteData & VFlipMask) == VFlipMask; }
		}
		
		// "B7,B6 = bits/pixel-1 (1,2,3,4)"
		public byte BitsPerPixel 
		{
			get { return (byte)((ByteData >> 6) + 1); }
		}

		private const byte HFlipMask = 0x10;
		private const byte VFlipMask = 0x08;
	}
}
