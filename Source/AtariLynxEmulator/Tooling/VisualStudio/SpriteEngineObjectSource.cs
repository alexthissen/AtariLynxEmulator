using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.DebuggerVisualizers;
using System.IO;

namespace KillerApps.Emulation.Atari.Lynx.Tooling
{
	public class SpriteEngineObjectSource : VisualizerObjectSource
	{
		public override void GetData(object target, Stream outgoingData)
		{
			SpriteEngine engine = target as SpriteEngine;
			SpriteEngineDebugView view = new SpriteEngineDebugView(engine);
			base.GetData(view, outgoingData);
		}
	}
}
