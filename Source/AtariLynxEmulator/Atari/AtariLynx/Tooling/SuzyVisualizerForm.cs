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
	public partial class SuzyVisualizerForm : Form
	{
		private Suzy suzy = null;

		public SuzyVisualizerForm(Suzy suzy)
		{
			InitializeComponent();
			this.suzy = suzy;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			spriteRenderControl.LynxMemory = suzy.Ram.GetDirectAccess();
		}
	}
}
