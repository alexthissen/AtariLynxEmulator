using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace KillerApps.Emulation.Atari.Lynx.Tooling
{
	public partial class SpriteRenderControl : UserControl
	{
		byte[] RedColorMap = new byte[0x10];
		byte[] GreenColorMap = new byte[0x10];
		byte[] BlueColorMap = new byte[0x10];
		byte[] screenPixels = new byte[160 * 102 * 3];
		byte[] videoMemory = new byte[0x10000];

		public SpriteControlBlock SpriteControlBlock { get; set; }
		public byte[] LynxMemory { get; set; }
		
		SpriteContext context = new SpriteContext();
		SpriteControlBlock scb = new SpriteControlBlock();
		SpriteEngine engine;
		private ushort currentScb = 0x169e;
	
		public SpriteRenderControl()
		{
			InitializeComponent();
		}

		public void SetDefaultColorMap()
		{
			for (int i = 0; i < 16; i++)
			{
				RedColorMap[i] = (byte)((i+1) * 15);
				BlueColorMap[i] = (byte)((i + 1) * 15);
				GreenColorMap[i] = (byte)((i + 1) * 15);
			}
		}

		public void CopyColorMap()
		{
			for (int index = 0; index < 0x0F; index++)
			{
				byte value;
				value = LynxMemory[Mikey.Addresses.GREEN0 + index];
				GreenColorMap[index] = (byte)((value & 0x0F) << 4);

				value = LynxMemory[Mikey.Addresses.BLUERED0 + index];
				BlueColorMap[index] = (byte)(value & 0xF0);
				RedColorMap[index] = (byte)((value & 0x0F) << 4);
			}
		}

		protected void PrepareRenderEngine()
		{
			engine = new SpriteEngine(context, LynxMemory, scb, videoMemory);
			SetDefaultColorMap();
			engine.InitializeFromSpriteDataStructure(LynxMemory, currentScb);

			context.HOFF.Value = 0;
			context.VOFF.Value = 0;
			context.VIDBAS.Value = 0x0000;
		}

		public void RenderSprite()
		{
			for (int i = 0; i < videoMemory.Length; i++) videoMemory[i] = 0x00;
			engine.RenderSingleSprite();

			int counter = 0;
			for (int index = 0; index < 160 * 102 / 2; index++)
			{
				byte source = videoMemory[context.VIDBAS.Value + index];
				SetPixel(counter, (byte)(source >> 4)); counter += 3;
				SetPixel(counter, (byte)(source & 0x0F)); counter += 3;
			}

			unsafe
			{
				Bitmap bmp = new Bitmap(160, 102, PixelFormat.Format24bppRgb);
				BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, 160, 102), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
				IntPtr ptr = bmpdata.Scan0;
				Marshal.Copy(screenPixels, 0, ptr, screenPixels.Length);
				bmp.UnlockBits(bmpdata);
				spritePictureBox.Image = bmp;
			}
		}

		private void SetPixel(int counter, byte source)
		{
			screenPixels[counter++] = RedColorMap[source];
			screenPixels[counter++] = GreenColorMap[source];
			screenPixels[counter++] = BlueColorMap[source];
		}

		private void renderButton_Click(object sender, EventArgs e)
		{
			PrepareRenderEngine();

			scb.SPRHSIZ.Value = (ushort)(horizontalSizeTrackBar.Value * 256);
			scb.SPRVSIZ.Value = (ushort)(verticalSizeTrackBar.Value * 256);

			RenderSprite();
		}

		private void horizontalSizeTrackBar_ValueChanged(object sender, EventArgs e)
		{
			horizontalSizeLabel.Text = String.Format("Horizontal size: {0}", horizontalSizeTrackBar.Value);
		}

		private void verticalSizeTrackBar_ValueChanged(object sender, EventArgs e)
		{
			verticalSizeLabel.Text = String.Format("Vertical size: {0}", verticalSizeTrackBar.Value);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			currentScb = scb.SCBNEXT.Value;
		}
	}
}
