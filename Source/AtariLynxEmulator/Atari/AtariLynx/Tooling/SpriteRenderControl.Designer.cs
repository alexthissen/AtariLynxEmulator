namespace KillerApps.Emulation.Atari.Lynx.Tooling
{
	partial class SpriteRenderControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.horizontalSizeLabel = new System.Windows.Forms.Label();
			this.horizontalSizeTrackBar = new System.Windows.Forms.TrackBar();
			this.spritePictureBox = new System.Windows.Forms.PictureBox();
			this.renderButton = new System.Windows.Forms.Button();
			this.verticalSizeTrackBar = new System.Windows.Forms.TrackBar();
			this.verticalSizeLabel = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.horizontalSizeTrackBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.spritePictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.verticalSizeTrackBar)).BeginInit();
			this.SuspendLayout();
			// 
			// horizontalSizeLabel
			// 
			this.horizontalSizeLabel.AutoSize = true;
			this.horizontalSizeLabel.Location = new System.Drawing.Point(329, 3);
			this.horizontalSizeLabel.Name = "horizontalSizeLabel";
			this.horizontalSizeLabel.Size = new System.Drawing.Size(87, 13);
			this.horizontalSizeLabel.TabIndex = 5;
			this.horizontalSizeLabel.Text = "Horizontal size: 1";
			// 
			// horizontalSizeTrackBar
			// 
			this.horizontalSizeTrackBar.Location = new System.Drawing.Point(329, 21);
			this.horizontalSizeTrackBar.Minimum = 1;
			this.horizontalSizeTrackBar.Name = "horizontalSizeTrackBar";
			this.horizontalSizeTrackBar.Size = new System.Drawing.Size(137, 45);
			this.horizontalSizeTrackBar.TabIndex = 4;
			this.horizontalSizeTrackBar.Value = 1;
			this.horizontalSizeTrackBar.ValueChanged += new System.EventHandler(this.horizontalSizeTrackBar_ValueChanged);
			// 
			// spritePictureBox
			// 
			this.spritePictureBox.Location = new System.Drawing.Point(3, 3);
			this.spritePictureBox.Name = "spritePictureBox";
			this.spritePictureBox.Size = new System.Drawing.Size(320, 204);
			this.spritePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.spritePictureBox.TabIndex = 3;
			this.spritePictureBox.TabStop = false;
			// 
			// renderButton
			// 
			this.renderButton.Location = new System.Drawing.Point(412, 137);
			this.renderButton.Name = "renderButton";
			this.renderButton.Size = new System.Drawing.Size(75, 23);
			this.renderButton.TabIndex = 6;
			this.renderButton.Text = "Render";
			this.renderButton.UseVisualStyleBackColor = true;
			this.renderButton.Click += new System.EventHandler(this.renderButton_Click);
			// 
			// verticalSizeTrackBar
			// 
			this.verticalSizeTrackBar.Location = new System.Drawing.Point(332, 86);
			this.verticalSizeTrackBar.Minimum = 1;
			this.verticalSizeTrackBar.Name = "verticalSizeTrackBar";
			this.verticalSizeTrackBar.Size = new System.Drawing.Size(134, 45);
			this.verticalSizeTrackBar.TabIndex = 7;
			this.verticalSizeTrackBar.Value = 1;
			this.verticalSizeTrackBar.ValueChanged += new System.EventHandler(this.verticalSizeTrackBar_ValueChanged);
			// 
			// verticalSizeLabel
			// 
			this.verticalSizeLabel.AutoSize = true;
			this.verticalSizeLabel.Location = new System.Drawing.Point(329, 69);
			this.verticalSizeLabel.Name = "verticalSizeLabel";
			this.verticalSizeLabel.Size = new System.Drawing.Size(75, 13);
			this.verticalSizeLabel.TabIndex = 5;
			this.verticalSizeLabel.Text = "Vertical size: 1";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(365, 174);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 8;
			this.button1.Text = "button1";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// SpriteRenderControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.button1);
			this.Controls.Add(this.verticalSizeTrackBar);
			this.Controls.Add(this.renderButton);
			this.Controls.Add(this.verticalSizeLabel);
			this.Controls.Add(this.horizontalSizeLabel);
			this.Controls.Add(this.horizontalSizeTrackBar);
			this.Controls.Add(this.spritePictureBox);
			this.Name = "SpriteRenderControl";
			this.Size = new System.Drawing.Size(543, 210);
			((System.ComponentModel.ISupportInitialize)(this.horizontalSizeTrackBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.spritePictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.verticalSizeTrackBar)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label horizontalSizeLabel;
		private System.Windows.Forms.TrackBar horizontalSizeTrackBar;
		private System.Windows.Forms.PictureBox spritePictureBox;
		private System.Windows.Forms.Button renderButton;
		private System.Windows.Forms.TrackBar verticalSizeTrackBar;
		private System.Windows.Forms.Label verticalSizeLabel;
		private System.Windows.Forms.Button button1;
	}
}
