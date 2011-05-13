using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace KillerApps.Emulation.Atari.Lynx
{
	[Serializable]
	public class SpriteCollisionNumber
	{
		// "B7,B6 = 0"
		// "B4 = 0"
		public byte ByteData 
		{
			get { return (byte)(byteData & 0x2F); }
			set
			{ 
				byteData = value;
				byteData &= 0x2F;
			}
		}

		public SpriteCollisionNumber(byte sprcoll)
		{
			this.ByteData = sprcoll;
		}

		// "B5 = dont collide. 1=dont collide with this sprite."
		public bool DontCollide 
		{
			get { return (ByteData & DontCollideMask) == DontCollideMask; }
			set { if (value) ByteData |= DontCollideMask; else ByteData &= (DontCollideMask ^ 0xFF); }
		}
		
		// "B3,B2,B1,B0 = number"
		public int Number 
		{ 
			get { return (ByteData & 0x0f); }
			set 
			{
				Debug.WriteLineIf(value > 0x0f, "Collision number should be 0 to 15.");
				ByteData |= (byte)(value & 0x0f);
			}
		}

		private const byte DontCollideMask = 0x20;
private  byte byteData;
	}
}
