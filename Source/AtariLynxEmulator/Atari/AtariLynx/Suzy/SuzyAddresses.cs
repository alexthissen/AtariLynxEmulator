using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public static class SuzyAddresses
	{
		public const ushort MATHD = 0xFC52;
		public const ushort MATHC = 0xFC53;
		public const ushort MATHB = 0xFC54;
		public const ushort MATHA = 0xFC55;
		public const ushort MATHP = 0xFC56;
		public const ushort MATHN = 0xFC57;
		public const ushort MATHH = 0xFC60;
		public const ushort MATHG = 0xFC61;
		public const ushort MATHF = 0xFC62;
		public const ushort MATHE = 0xFC63;
		public const ushort MATHM = 0xFC6C;
		public const ushort MATHL = 0xFC6D;
		public const ushort MATHK = 0xFC6E;
		public const ushort MATHJ = 0xFC6F;

		public const ushort SPRCTL0 = 0xFC80; // "FC80 = SPRCTL0 Sprite Control Bits 0 (W)"
		public const ushort SPRCTL1 = 0xFC81; // "FC81 = SPRCTL1 Sprite Control Bits 1 (W)(U)"
		public const ushort SPRCOL = 0xFC82; // "FC82 = SPRCOLL. Sprite Collision Number (W)"
		public const ushort SPRINIT = 0xFC83; // "Sprite Initialization Bits (W)(U)"
		public const ushort SUZYBUSEN = 0xFC90; // "FC90 = SUZYBUSEN. Suzy Bus Enable (W)"
		public const ushort SPRGO = 0xFC91; // "FC91 = SPRG0. Sprite Process Start Bit (W)"
		public const ushort SPRSYS = 0xFC92; // "FC92 = SPRSYS. System Control Bits (R/W)"
		
		public const ushort SUZYHREV = 0xFC88; // Suzy Hardware Revision (R)
		public const ushort RCART0 = 0xFCB2; // RCART(R/W)
		public const ushort RCART1 = 0xFCB3; // RCART(R/W)
	}
}
