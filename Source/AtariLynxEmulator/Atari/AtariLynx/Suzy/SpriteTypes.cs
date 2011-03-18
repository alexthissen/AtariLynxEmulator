using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public enum SpriteTypes
	{
		BackgroundShadow = 0, // "0 -> background, shadow"
		BackgroundNoCollision = 1, // "1 -> background-no collision"
		BoundaryShadow = 2, // "2 -> boundary-shadow"
		Boundary = 3, // "3 -> boundary"
		Normal = 4, // "4 -> normal"
		NonCollidable = 5, // "5 -> non-collidable"
		ExclusiveOrShadow = 6, // "6 -> exclusive-or, shadow"
		Shadow = 7 // "7 -> shadow"
	}
}
