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

		public Word SCBNEXT;
		public Word SPRDLINE;
		public Word HPOSSTRT, VPOSSTRT;
		public Word SPRHSIZ, SPRVSIZ;
		
		public ushort NextSCBAddress { get; private set; }
		public ushort SpriteDataAddress { get; private set; }
		public short StartingHPos { get; private set; } // "(2) 16 bits of starting H Pos"
		public short StartingVPos { get; private set; } // "(2) 16 bits of starting V Pos"
		public ushort HSizeBits { get; private set; } // "(2) 16 bits of H size bits"
		public ushort VSizeBits { get; private set; } // "(2) 16 bits of V size bits"

		public byte CollisionDepository { get; private set; }
		
		public static SpriteControlBlock Create(byte[] ram, ushort startAddress, out ushort bytesRead)
		{
			SpriteControlBlock scb = new SpriteControlBlock();
			bytesRead = scb.ParseDataStructure(ram, startAddress);
			return scb;
		}
			
		internal ushort ParseDataStructure(byte[] ramMemory, ushort startAddress)
		{
			int address = startAddress;
			
			// Static area
			SPRCTL0 = new SpriteControlBits0(ramMemory[address++]);
			SPRCTL1 = new SpriteControlBits1(ramMemory[address++]);
			SPRCOLL = new SpriteCollisionNumber(ramMemory[address++]);
			SCBNEXT.Value = BitConverter.ToUInt16(ramMemory, address); address += 2;

			if (!SPRCTL1.SkipSprite)
			{
				SPRDLINE.Value = BitConverter.ToUInt16(ramMemory, address); address += 2;
				HPOSSTRT.Value = BitConverter.ToUInt16(ramMemory, address); address += 2;
				VPOSSTRT.Value = BitConverter.ToUInt16(ramMemory, address); address += 2;
			}
			
			return (ushort)(address - startAddress);
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
