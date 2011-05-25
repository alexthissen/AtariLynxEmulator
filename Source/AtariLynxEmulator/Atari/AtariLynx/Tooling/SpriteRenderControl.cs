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

		private ushort VIDBAS = 0x0000;
		private ushort SCBNEXT;
		private byte backgroundColorIndex = 0x00;
		private SpriteEngine engine;

		public SpriteRenderControl()
		{
			InitializeComponent();
		}

		public SpriteEngine Engine
		{
			get { return engine; }
			set
			{
				engine = value;
				SCBNEXT = engine.SpriteControlBlock.SCBNEXT.Value;
				UpdateInformationPanel();
			}
		}

		private void UpdateInformationPanel()
		{
			scbNextLabel.Text = String.Format("SCBNEXT: {0:X4}", SCBNEXT);
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
			SetDefaultColorMap();
			Engine.InitializeFromSpriteDataStructure(SCBNEXT);
			Engine.OverrideVideo(videoMemory, VIDBAS);
		}

		private void ClearBackground()
		{
			for (int i = 0; i < videoMemory.Length; i++) videoMemory[i] = (byte)((backgroundColorIndex << 4) + backgroundColorIndex);
		}

		public void RenderSprite()
		{
			if (clearCheckBox.Checked) ClearBackground();
			
			Engine.RenderSingleSprite();

			int counter = 0;
			for (int index = 0; index < 160 * 102 / 2; index++)
			{
				byte source = videoMemory[VIDBAS + index];
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
			RefreshRendering();
		}

		private void RefreshRendering()
		{
			PrepareRenderEngine();

			//Engine.SpriteControlBlock.SPRHSIZ.Value = (ushort)(horizontalSizeTrackBar.Value * 256);
			//Engine.SpriteControlBlock.SPRVSIZ.Value = (ushort)(verticalSizeTrackBar.Value * 256);

			short currentValue = (short)Engine.SpriteControlBlock.VPOSSTRT.Value;
			currentValue += (short)verticalPositionScrollBar.Value;
			Engine.SpriteControlBlock.VPOSSTRT.Value = (ushort)currentValue;

			currentValue = (short)Engine.SpriteControlBlock.HPOSSTRT.Value;
			currentValue += (short)horizontalPositionScrollBar.Value;
			Engine.SpriteControlBlock.HPOSSTRT.Value = (ushort)currentValue;

			//Engine.SpriteControlBlock.SPRCTL0.VFlip = vFlipCheckBox.Checked;
			//Engine.SpriteControlBlock.SPRCTL0.HFlip = hFlipCheckBox.Checked;

			RenderSprite();
		}

		private void horizontalSizeTrackBar_ValueChanged(object sender, EventArgs e)
		{
			horizontalSizeLabel.Text = String.Format("Horizontal size: {0}", horizontalSizeTrackBar.Value);
			RefreshRendering();
		}

		private void verticalSizeTrackBar_ValueChanged(object sender, EventArgs e)
		{
			verticalSizeLabel.Text = String.Format("Vertical size: {0}", verticalSizeTrackBar.Value);
			RefreshRendering();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			SCBNEXT = Engine.SpriteControlBlock.SCBNEXT.Value;
			UpdateInformationPanel();
		}

		private void verticalPositionScrollBar_ValueChanged(object sender, EventArgs e)
		{
			RefreshRendering();
		}

		private void horizontalPositionScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			RefreshRendering();
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			backgroundColorIndex = (byte)comboBox1.SelectedIndex;
		}

		private void vFlipCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			RefreshRendering();
		}

		private void hFlipCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			RefreshRendering();
		}
	}
}
