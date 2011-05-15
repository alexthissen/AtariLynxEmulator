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
			this.scbNextLabel = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.verticalPositionScrollBar = new System.Windows.Forms.VScrollBar();
			this.horizontalPositionScrollBar = new System.Windows.Forms.HScrollBar();
			this.backgroundColorDialog = new System.Windows.Forms.ColorDialog();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.vFlipCheckBox = new System.Windows.Forms.CheckBox();
			this.hFlipCheckBox = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.horizontalSizeTrackBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.spritePictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.verticalSizeTrackBar)).BeginInit();
			this.SuspendLayout();
			// 
			// horizontalSizeLabel
			// 
			this.horizontalSizeLabel.AutoSize = true;
			this.horizontalSizeLabel.Location = new System.Drawing.Point(358, 3);
			this.horizontalSizeLabel.Name = "horizontalSizeLabel";
			this.horizontalSizeLabel.Size = new System.Drawing.Size(87, 13);
			this.horizontalSizeLabel.TabIndex = 5;
			this.horizontalSizeLabel.Text = "Horizontal size: 1";
			// 
			// horizontalSizeTrackBar
			// 
			this.horizontalSizeTrackBar.Location = new System.Drawing.Point(358, 21);
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
			this.renderButton.Location = new System.Drawing.Point(503, 239);
			this.renderButton.Name = "renderButton";
			this.renderButton.Size = new System.Drawing.Size(75, 23);
			this.renderButton.TabIndex = 6;
			this.renderButton.Text = "Render";
			this.renderButton.UseVisualStyleBackColor = true;
			this.renderButton.Click += new System.EventHandler(this.renderButton_Click);
			// 
			// verticalSizeTrackBar
			// 
			this.verticalSizeTrackBar.Location = new System.Drawing.Point(361, 85);
			this.verticalSizeTrackBar.Minimum = 1;
			this.verticalSizeTrackBar.Name = "verticalSizeTrackBar";
			this.verticalSizeTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.verticalSizeTrackBar.Size = new System.Drawing.Size(45, 134);
			this.verticalSizeTrackBar.TabIndex = 7;
			this.verticalSizeTrackBar.Value = 1;
			this.verticalSizeTrackBar.ValueChanged += new System.EventHandler(this.verticalSizeTrackBar_ValueChanged);
			// 
			// verticalSizeLabel
			// 
			this.verticalSizeLabel.AutoSize = true;
			this.verticalSizeLabel.Location = new System.Drawing.Point(358, 69);
			this.verticalSizeLabel.Name = "verticalSizeLabel";
			this.verticalSizeLabel.Size = new System.Drawing.Size(75, 13);
			this.verticalSizeLabel.TabIndex = 5;
			this.verticalSizeLabel.Text = "Vertical size: 1";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(486, 146);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 8;
			this.button1.Text = "button1";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// scbNextLabel
			// 
			this.scbNextLabel.AutoSize = true;
			this.scbNextLabel.Location = new System.Drawing.Point(4, 240);
			this.scbNextLabel.Name = "scbNextLabel";
			this.scbNextLabel.Size = new System.Drawing.Size(60, 13);
			this.scbNextLabel.TabIndex = 9;
			this.scbNextLabel.Text = "SCBNEXT:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(275, 276);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(259, 26);
			this.label1.TabIndex = 10;
			this.label1.Text = "WORK IN PROGRESS!!!";
			// 
			// verticalPositionScrollBar
			// 
			this.verticalPositionScrollBar.Location = new System.Drawing.Point(326, 3);
			this.verticalPositionScrollBar.Maximum = 102;
			this.verticalPositionScrollBar.Minimum = -102;
			this.verticalPositionScrollBar.Name = "verticalPositionScrollBar";
			this.verticalPositionScrollBar.Size = new System.Drawing.Size(17, 204);
			this.verticalPositionScrollBar.TabIndex = 11;
			this.verticalPositionScrollBar.ValueChanged += new System.EventHandler(this.verticalPositionScrollBar_ValueChanged);
			// 
			// horizontalPositionScrollBar
			// 
			this.horizontalPositionScrollBar.Location = new System.Drawing.Point(4, 210);
			this.horizontalPositionScrollBar.Maximum = 160;
			this.horizontalPositionScrollBar.Minimum = -160;
			this.horizontalPositionScrollBar.Name = "horizontalPositionScrollBar";
			this.horizontalPositionScrollBar.Size = new System.Drawing.Size(319, 17);
			this.horizontalPositionScrollBar.TabIndex = 12;
			this.horizontalPositionScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.horizontalPositionScrollBar_Scroll);
			// 
			// comboBox1
			// 
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15"});
			this.comboBox1.Location = new System.Drawing.Point(358, 242);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(56, 21);
			this.comboBox1.TabIndex = 13;
			this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// vFlipCheckBox
			// 
			this.vFlipCheckBox.AutoSize = true;
			this.vFlipCheckBox.Location = new System.Drawing.Point(132, 239);
			this.vFlipCheckBox.Name = "vFlipCheckBox";
			this.vFlipCheckBox.Size = new System.Drawing.Size(80, 17);
			this.vFlipCheckBox.TabIndex = 14;
			this.vFlipCheckBox.Text = "Vertical Flip";
			this.vFlipCheckBox.UseVisualStyleBackColor = true;
			this.vFlipCheckBox.CheckedChanged += new System.EventHandler(this.vFlipCheckBox_CheckedChanged);
			// 
			// hFlipCheckBox
			// 
			this.hFlipCheckBox.AutoSize = true;
			this.hFlipCheckBox.Location = new System.Drawing.Point(132, 263);
			this.hFlipCheckBox.Name = "hFlipCheckBox";
			this.hFlipCheckBox.Size = new System.Drawing.Size(92, 17);
			this.hFlipCheckBox.TabIndex = 15;
			this.hFlipCheckBox.Text = "Horizontal Flip";
			this.hFlipCheckBox.UseVisualStyleBackColor = true;
			this.hFlipCheckBox.CheckedChanged += new System.EventHandler(this.hFlipCheckBox_CheckedChanged);
			// 
			// SpriteRenderControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.hFlipCheckBox);
			this.Controls.Add(this.vFlipCheckBox);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.horizontalPositionScrollBar);
			this.Controls.Add(this.verticalPositionScrollBar);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.scbNextLabel);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.verticalSizeTrackBar);
			this.Controls.Add(this.renderButton);
			this.Controls.Add(this.verticalSizeLabel);
			this.Controls.Add(this.horizontalSizeLabel);
			this.Controls.Add(this.horizontalSizeTrackBar);
			this.Controls.Add(this.spritePictureBox);
			this.Name = "SpriteRenderControl";
			this.Size = new System.Drawing.Size(619, 306);
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
		private System.Windows.Forms.Label scbNextLabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.VScrollBar verticalPositionScrollBar;
		private System.Windows.Forms.HScrollBar horizontalPositionScrollBar;
		private System.Windows.Forms.ColorDialog backgroundColorDialog;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.CheckBox vFlipCheckBox;
		private System.Windows.Forms.CheckBox hFlipCheckBox;
	}
}
