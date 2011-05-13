namespace KillerApps.Emulation.Atari.Lynx.Tooling
{
	partial class SuzyVisualizerForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.spritesTabPage = new System.Windows.Forms.TabPage();
			this.registersTabPage = new System.Windows.Forms.TabPage();
			this.spriteRenderControl = new KillerApps.Emulation.Atari.Lynx.Tooling.SpriteRenderControl();
			this.tabControl1.SuspendLayout();
			this.spritesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.spritesTabPage);
			this.tabControl1.Controls.Add(this.registersTabPage);
			this.tabControl1.Location = new System.Drawing.Point(12, 12);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(571, 266);
			this.tabControl1.TabIndex = 0;
			// 
			// spritesTabPage
			// 
			this.spritesTabPage.Controls.Add(this.spriteRenderControl);
			this.spritesTabPage.Location = new System.Drawing.Point(4, 22);
			this.spritesTabPage.Name = "spritesTabPage";
			this.spritesTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.spritesTabPage.Size = new System.Drawing.Size(563, 240);
			this.spritesTabPage.TabIndex = 0;
			this.spritesTabPage.Text = "Sprites";
			this.spritesTabPage.UseVisualStyleBackColor = true;
			// 
			// registersTabPage
			// 
			this.registersTabPage.Location = new System.Drawing.Point(4, 22);
			this.registersTabPage.Name = "registersTabPage";
			this.registersTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.registersTabPage.Size = new System.Drawing.Size(563, 240);
			this.registersTabPage.TabIndex = 1;
			this.registersTabPage.Text = "Registers";
			this.registersTabPage.UseVisualStyleBackColor = true;
			// 
			// spriteRenderControl
			// 
			this.spriteRenderControl.Location = new System.Drawing.Point(6, 6);
			this.spriteRenderControl.LynxMemory = null;
			this.spriteRenderControl.Name = "spriteRenderControl";
			this.spriteRenderControl.Size = new System.Drawing.Size(543, 210);
			this.spriteRenderControl.SpriteControlBlock = null;
			this.spriteRenderControl.TabIndex = 0;
			// 
			// SuzyVisualizerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(595, 290);
			this.Controls.Add(this.tabControl1);
			this.Name = "SuzyVisualizerForm";
			this.Text = "Suzy Inspector";
			this.tabControl1.ResumeLayout(false);
			this.spritesTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage spritesTabPage;
		private System.Windows.Forms.TabPage registersTabPage;
		private SpriteRenderControl spriteRenderControl;

	}
}