using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KillerApps.Emulation.Processors;

namespace KillerApps.Emulation.Atari.Lynx.Tooling
{
	public partial class SpriteEngineVisualizerForm : Form
	{
		private SpriteEngine engine = null;

		public SpriteEngineVisualizerForm(SpriteEngine engine)
		{
			InitializeComponent();
			this.engine = engine;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			spriteRenderControl.Engine = engine;
		}
	}
}
