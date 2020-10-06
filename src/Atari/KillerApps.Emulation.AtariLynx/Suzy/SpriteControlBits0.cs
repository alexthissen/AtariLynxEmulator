using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.AtariLynx
{
	public class SpriteControlBits0
	{
		public SpriteControlBits0(byte sprctl0)
		{
			this.ByteData = sprctl0;
		}

		public byte ByteData { get; set; }

		// "B2,B1,B0 = Sprite Type"
		public SpriteTypes SpriteType 
		{
			get { return (SpriteTypes)(ByteData & 0x07); }
		}

		// "Sprites can be horizontally and/or vertically flipped."
		// "B4 = V flip, 0 = not flipped"
		public bool VFlip
		{
			get { return (ByteData & VFlipMask) == VFlipMask; }
			set { ByteData = (byte)(value ? ByteData | VFlipMask : ByteData & (VFlipMask ^ 0xFF)); }
		}

		// "Sprites can be horizontally and/or vertically flipped."
		// "B5 = H flip, 0 = not flipped"
		public bool HFlip 
		{
			get { return (ByteData & HFlipMask) == HFlipMask; }
			set { ByteData = (byte)(value ? ByteData | HFlipMask : ByteData & (HFlipMask ^ 0xFF)); }
		}
		
		// "B7,B6 = bits/pixel-1 (1,2,3,4)"
		public byte BitsPerPixel 
		{
			get { return (byte)((ByteData >> 6) + 1); }
		}

		private const byte HFlipMask = 0x20;
		private const byte VFlipMask = 0x10;
	}
}
