using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.Core;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class SpriteControlBlock
	{
		// "a 16 bit wide sprite control block register set"
		public SpriteControlBits0 SPRCTL0 { get; private set; }
		public SpriteControlBits1 SPRCTL1 { get; private set; }
		public SpriteCollisionNumber SPRCOLL { get; private set; }

		// "The SCBs are linked by pointers in 'Painters Order'."
		public Word SCBNEXT;
		public Word SPRDLINE;
		public Word HPOSSTRT, VPOSSTRT;
		public Word SPRHSIZ, SPRVSIZ;
		
		public byte CollisionDepository { get; private set; }

		public SpriteControlBlock()
		{
			SPRCTL0 = new SpriteControlBits0(0);
			SPRCTL1 = new SpriteControlBits1(0);
			SPRCOLL = new SpriteCollisionNumber(0);
		}

		public QuadrantOrder StartQuadrant
		{
			get
			{
				QuadrantOrder quadrant;
				if (SPRCTL1.StartDrawingLeft)
				{
					quadrant = SPRCTL1.StartDrawingUp ? QuadrantOrder.UpLeft : QuadrantOrder.DownLeft;
				}
				else
				{
					quadrant = SPRCTL1.StartDrawingUp ? QuadrantOrder.UpRight : QuadrantOrder.DownRight;
				}
				return quadrant;
			}
		}
	}
}
