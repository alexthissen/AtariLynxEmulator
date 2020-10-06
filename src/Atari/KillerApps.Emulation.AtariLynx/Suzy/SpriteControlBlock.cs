using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using KillerApps.Emulation.Processors;

namespace KillerApps.Emulation.AtariLynx
{
	public class SpriteControlBlock
	{
		public static Quadrant[] Quadrants = new Quadrant[4]
			{
				new Quadrant(1, 1, QuadrantOrder.DownRight),
				new Quadrant(1, -1, QuadrantOrder.UpRight),
				new Quadrant(-1, -1, QuadrantOrder.UpLeft),
				new Quadrant(-1, 1, QuadrantOrder.DownLeft)
			};

		// "a 16 bit wide sprite control block register set"
		public SpriteControlBits0 SPRCTL0 { get; private set; }
		public SpriteControlBits1 SPRCTL1 { get; private set; }
		public SpriteCollisionNumber SPRCOLL { get; private set; }

		// "The SCBs are linked by pointers in 'Painters Order'."
		public Word SCBNEXT;

		public Word SPRDLINE;
		public Word HPOSSTRT, VPOSSTRT;
		public Word SPRHSIZ, SPRVSIZ;
		
		public SpriteControlBlock()
		{
			SPRCTL0 = new SpriteControlBits0(0);
			SPRCTL1 = new SpriteControlBits1(0);
			SPRCOLL = new SpriteCollisionNumber(0);
		}

		public Quadrant StartQuadrant
		{
			get
			{
				Quadrant quadrant;
				if (SPRCTL1.StartDrawingLeft)
				{
					quadrant = SPRCTL1.StartDrawingUp ? Quadrants[(int)QuadrantOrder.UpLeft] : Quadrants[(int)QuadrantOrder.DownLeft];
				}
				else
				{
					quadrant = SPRCTL1.StartDrawingUp ? Quadrants[(int)QuadrantOrder.UpRight] : Quadrants[(int)QuadrantOrder.DownRight];
				}
				return quadrant;
			}
		}
	}
}
