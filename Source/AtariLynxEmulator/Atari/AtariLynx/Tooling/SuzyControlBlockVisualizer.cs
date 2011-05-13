using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace KillerApps.Emulation.Atari.Lynx.Tooling
{
	/// <summary>
	/// A Visualizer for SuzyVisualizer.  
	/// </summary>
	public class SuzyVisualizer : DialogDebuggerVisualizer
	{
		protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
		{
			if (windowService == null)
				throw new ArgumentNullException("windowService");
			if (objectProvider == null)
				throw new ArgumentNullException("objectProvider");
			 
			Suzy suzy = (Suzy)objectProvider.GetObject();

			using (SuzyVisualizerForm displayForm = new SuzyVisualizerForm(suzy))
			{
				windowService.ShowDialog(displayForm);
			}
		}

		/// <summary>
		/// Tests the visualizer by hosting it outside of the debugger.
		/// </summary>
		/// <param name="objectToVisualize">The object to display in the visualizer.</param>
		public static void TestShowVisualizer(object objectToVisualize)
		{
			VisualizerDevelopmentHost visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(SuzyVisualizer));
			visualizerHost.ShowVisualizer();
		}
	}
}
