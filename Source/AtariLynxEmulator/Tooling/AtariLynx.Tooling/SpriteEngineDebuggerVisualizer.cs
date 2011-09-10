using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace KillerApps.Emulation.Atari.Lynx.Tooling
{
	public class SpriteEngineDebuggerVisualizer : DialogDebuggerVisualizer
	{
		protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
		{
			if (windowService == null)
				throw new ArgumentNullException("windowService");
			if (objectProvider == null)
				throw new ArgumentNullException("objectProvider");
			 
			SpriteEngineDebugView view = (SpriteEngineDebugView)objectProvider.GetObject();
			
			SpriteControlBlock scb = new SpriteControlBlock();
			scb.SPRCOLL.ByteData = view.SpriteControlBlock.SpriteCollision;
			scb.SPRCTL0.ByteData = view.SpriteControlBlock.SpriteControlBlock0;
			scb.SPRCTL1.ByteData = view.SpriteControlBlock.SpriteControlBlock1;
			scb.SCBNEXT.Value = view.SpriteControlBlock.ScbNext;

			SpriteContext context = new SpriteContext();
			context.COLLBAS.Value = view.SpriteContext.CollisionBase;
			context.COLLOFF.Value = view.SpriteContext.CollisionOffset;
			context.DontCollide = view.SpriteContext.DontCollide;
			context.HOFF.Value = view.SpriteContext.HorizontalOffset;
			context.HSIZOFF.Value = view.SpriteContext.HorizontalSizeOffset;
			context.VIDBAS.Value = view.SpriteContext.VideoBase;
			context.VOFF.Value = view.SpriteContext.VerticalOffset;
			context.VSIZOFF.Value = view.SpriteContext.VerticalSizeOffset;
			context.VStretch = view.SpriteContext.VerticalStretch;

			SpriteEngine engine = new SpriteEngine(context, view.RamMemory, scb);

			using (SpriteEngineVisualizerForm displayForm = new SpriteEngineVisualizerForm(engine))
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
			VisualizerDevelopmentHost visualizerHost = 
				new VisualizerDevelopmentHost(objectToVisualize, typeof(SpriteEngineDebuggerVisualizer), typeof(SpriteEngineObjectSource));
			visualizerHost.ShowVisualizer();
		}
	}
}
