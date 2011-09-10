using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx.Tooling
{
	[Serializable]
	public class SpriteContextDebugView
	{
		public ushort CollisionBase;
		public ushort CollisionOffset;
		public bool DontCollide;
		public bool VerticalStretch;
		public ushort HorizontalOffset;
		public ushort HorizontalSizeOffset;
		public ushort VideoBase;
		public ushort VerticalOffset;
		public ushort VerticalSizeOffset;
		
		public SpriteContextDebugView(SpriteContext context)
		{
			CollisionBase = context.COLLBAS.Value;
			CollisionOffset = context.COLLOFF.Value;
			DontCollide = context.DontCollide;
			VerticalStretch = context.VStretch;
			HorizontalOffset = context.HOFF.Value;
			HorizontalSizeOffset = context.HSIZOFF.Value;
			VideoBase = context.VIDBAS.Value;
			VerticalOffset = context.VOFF.Value;
			VerticalSizeOffset = context.VSIZOFF.Value;
		}
	}
}
