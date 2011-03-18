using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	// "B1 = reserved"
	public class SpriteProcessStart
	{
		public byte ByteData { get; set; }

		// "B0 = Sprite process enabled, 0=disabled."
		public bool SpriteProcessEnabled 
		{
			get { return (ByteData & SPRITE_GOMask) == SPRITE_GOMask; } 
		}
		
		// "B2= enable everon detector. 1 = enabled."
		public bool EveronDetectorEnabled 
		{
			get { return (ByteData & EVER_ONMask) == EVER_ONMask; } 
		}

		public const byte EVER_ONMask = 0x04;
		public const byte SPRITE_GOMask = 0x01;
	}
}
