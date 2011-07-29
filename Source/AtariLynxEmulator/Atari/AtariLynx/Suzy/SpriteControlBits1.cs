using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class SpriteControlBits1
	{
		public byte ByteData { get; set; }

		public SpriteControlBits1(byte sprctl1)
		{
			this.ByteData = sprctl1;
		}
		
		// "B7 = literal attribute, 0=normal, 1=totally literal"
		public bool TotallyLiteral 
		{ 
			get { return (ByteData & TotallyLiteralMask) == TotallyLiteralMask; }
		}

		// "B6 = Sizing algorithm choice, 0=adder (algo 4), 1=shifter (algo 3)
		// SET IT T0 ZERO!!!!, algo 3 is broke"
		public SizingAlgorithm SizingAlgorithm 
		{
			get 
			{
				SizingAlgorithm algorithm = ((ByteData & SizingAlgorithmMask) == SizingAlgorithmMask) ? SizingAlgorithm.Shifter : SizingAlgorithm.Adder;
				//Debug.WriteLineIf(algorithm == SizingAlgorithm.Shifter, "Warning: Shifter algorithm is broken.");
				return algorithm;
			}
		}

		// "B5,B4 = Reloadable depth."
		public ReloadableDepth ReloadableDepth 
		{
			get { return (ReloadableDepth)((ByteData & 0x30) >> 4); }
		}

		// "B3 = Palette re-load. 0=reload the palette, 1 use existing palette."
		public bool ReusePalette
		{
			get { return (ByteData & ReloadPaletteMask) == ReloadPaletteMask; }
		}

		// "The processing of an actual sprite can be 'skipped' on a sprite by sprite basis."
		// "B2 = Skipsprite. 1 skip this sprite, 0=use this sprite."
		public bool SkipSprite
		{
			get { return (ByteData & SkipSpriteMask) == SkipSpriteMask; }
		}
		
		// "B1 = Start drawing up (1=up, 0=down)"
		public bool StartDrawingUp 
		{ 
			get { return (ByteData & StartDrawingUpMask) == StartDrawingUpMask; }
		}

		// "B0 = Start drawing left (1 left, 0=right) "
		public bool StartDrawingLeft 
		{ 
			get { return (ByteData & StartDrawingLeftMask) == StartDrawingLeftMask; }
		}

		private const byte TotallyLiteralMask = 0x80;
		private const byte SizingAlgorithmMask = 0x40;
		private const byte ReloadPaletteMask = 0x08;
		private const byte SkipSpriteMask = 0x04;
		private const byte StartDrawingUpMask = 0x02;
		private const byte StartDrawingLeftMask = 0x01;
	}
}
