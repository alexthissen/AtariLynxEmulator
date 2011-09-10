using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using KillerApps.Emulation.Atari.Lynx;
using KillerApps.Emulation.Core;

namespace KillerApps.Emulation.Atari.Lynx.Tooling
{
	[Serializable]
	public class SpriteEngineDebugView
	{
		public SpriteEngineDebugView() { }

		public SpriteEngineDebugView(SpriteEngine engine)
		{
			this.SpriteContext = new SpriteContextDebugView(engine.context);
			this.SpriteControlBlock = new SpriteControlBlockDebugView(engine.SpriteControlBlock);
			this.RamMemory = engine.ramMemory;
		}

		public SpriteContextDebugView SpriteContext { get; set; }
		public byte[] RamMemory { get; set; }
		public SpriteControlBlockDebugView SpriteControlBlock { get; set; }
		public uint[] ColorMap;
	}
}
