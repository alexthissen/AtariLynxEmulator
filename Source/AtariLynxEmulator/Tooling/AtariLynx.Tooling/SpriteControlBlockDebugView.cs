using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx.Tooling
{
	[Serializable]
	public class SpriteControlBlockDebugView
	{
		public byte SpriteCollision;
		public byte SpriteControlBlock0;
		public byte SpriteControlBlock1;
		public ushort ScbNext;

		public SpriteControlBlockDebugView(SpriteControlBlock spriteControlBlock)
		{
			SpriteCollision = spriteControlBlock.SPRCOLL.ByteData;
			SpriteControlBlock0 = spriteControlBlock.SPRCTL0.ByteData;
			SpriteControlBlock1 = spriteControlBlock.SPRCTL1.ByteData;
			ScbNext = spriteControlBlock.SCBNEXT.Value;
		}
	}
}
