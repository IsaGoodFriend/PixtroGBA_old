﻿namespace Pixtro.Client.Editor
{
	partial class GbaGpuView
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
			this.components = new System.ComponentModel.Container();
			this.listBoxWidgets = new System.Windows.Forms.ListBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label1 = new Pixtro.WinForms.Controls.LocLabelEx();
			this.buttonShowWidget = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.updownBGPal = new System.Windows.Forms.NumericUpDown();
			this.locLabelEx2 = new Pixtro.WinForms.Controls.LocLabelEx();
			this.updownSpritePal = new System.Windows.Forms.NumericUpDown();
			this.locLabelEx1 = new Pixtro.WinForms.Controls.LocLabelEx();
			this.buttonRefresh = new System.Windows.Forms.Button();
			this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
			this.radioButtonManual = new System.Windows.Forms.RadioButton();
			this.radioButtonScanline = new System.Windows.Forms.RadioButton();
			this.labelClipboard = new Pixtro.WinForms.Controls.LocLabelEx();
			this.timerMessage = new System.Windows.Forms.Timer(this.components);
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.updownBGPal)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.updownSpritePal)).BeginInit();
			this.SuspendLayout();
			// 
			// listBoxWidgets
			// 
			this.listBoxWidgets.Location = new System.Drawing.Point(12, 40);
			this.listBoxWidgets.Name = "listBoxWidgets";
			this.listBoxWidgets.Size = new System.Drawing.Size(137, 160);
			this.listBoxWidgets.TabIndex = 0;
			this.listBoxWidgets.DoubleClick += new System.EventHandler(this.listBoxWidgets_DoubleClick);
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.AutoScroll = true;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Location = new System.Drawing.Point(155, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(481, 474);
			this.panel1.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 24);
			this.label1.Name = "label1";
			this.label1.Text = "Available widgets:";
			// 
			// buttonShowWidget
			// 
			this.buttonShowWidget.Location = new System.Drawing.Point(29, 206);
			this.buttonShowWidget.Name = "buttonShowWidget";
			this.buttonShowWidget.Size = new System.Drawing.Size(75, 23);
			this.buttonShowWidget.TabIndex = 3;
			this.buttonShowWidget.Text = "Show >>";
			this.buttonShowWidget.UseVisualStyleBackColor = true;
			this.buttonShowWidget.Click += new System.EventHandler(this.buttonShowWidget_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.updownBGPal);
			this.groupBox1.Controls.Add(this.locLabelEx2);
			this.groupBox1.Controls.Add(this.updownSpritePal);
			this.groupBox1.Controls.Add(this.locLabelEx1);
			this.groupBox1.Controls.Add(this.buttonRefresh);
			this.groupBox1.Controls.Add(this.hScrollBar1);
			this.groupBox1.Controls.Add(this.radioButtonManual);
			this.groupBox1.Controls.Add(this.radioButtonScanline);
			this.groupBox1.Location = new System.Drawing.Point(15, 235);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(134, 192);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Refresh";
			// 
			// updownBGPal
			// 
			this.updownBGPal.Location = new System.Drawing.Point(6, 162);
			this.updownBGPal.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
			this.updownBGPal.Name = "updownBGPal";
			this.updownBGPal.Size = new System.Drawing.Size(122, 20);
			this.updownBGPal.TabIndex = 11;
			// 
			// locLabelEx2
			// 
			this.locLabelEx2.Location = new System.Drawing.Point(6, 146);
			this.locLabelEx2.MaximumSize = new System.Drawing.Size(145, 0);
			this.locLabelEx2.Name = "locLabelEx2";
			this.locLabelEx2.Text = "Background Palette";
			// 
			// updownSpritePal
			// 
			this.updownSpritePal.Location = new System.Drawing.Point(6, 123);
			this.updownSpritePal.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
			this.updownSpritePal.Name = "updownSpritePal";
			this.updownSpritePal.Size = new System.Drawing.Size(122, 20);
			this.updownSpritePal.TabIndex = 9;
			// 
			// locLabelEx1
			// 
			this.locLabelEx1.Location = new System.Drawing.Point(6, 107);
			this.locLabelEx1.MaximumSize = new System.Drawing.Size(145, 0);
			this.locLabelEx1.Name = "locLabelEx1";
			this.locLabelEx1.Text = "Sprite Palette";
			// 
			// buttonRefresh
			// 
			this.buttonRefresh.Location = new System.Drawing.Point(6, 81);
			this.buttonRefresh.Name = "buttonRefresh";
			this.buttonRefresh.Size = new System.Drawing.Size(75, 23);
			this.buttonRefresh.TabIndex = 4;
			this.buttonRefresh.Text = "Refresh";
			this.buttonRefresh.UseVisualStyleBackColor = true;
			this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
			// 
			// hScrollBar1
			// 
			this.hScrollBar1.LargeChange = 20;
			this.hScrollBar1.Location = new System.Drawing.Point(3, 39);
			this.hScrollBar1.Maximum = 246;
			this.hScrollBar1.Name = "hScrollBar1";
			this.hScrollBar1.Size = new System.Drawing.Size(128, 16);
			this.hScrollBar1.TabIndex = 3;
			this.hScrollBar1.ValueChanged += new System.EventHandler(this.hScrollBar1_ValueChanged);
			// 
			// radioButtonManual
			// 
			this.radioButtonManual.AutoSize = true;
			this.radioButtonManual.Location = new System.Drawing.Point(6, 58);
			this.radioButtonManual.Name = "radioButtonManual";
			this.radioButtonManual.Size = new System.Drawing.Size(60, 17);
			this.radioButtonManual.TabIndex = 2;
			this.radioButtonManual.TabStop = true;
			this.radioButtonManual.Text = "Manual";
			this.radioButtonManual.UseVisualStyleBackColor = true;
			this.radioButtonManual.CheckedChanged += new System.EventHandler(this.radioButtonManual_CheckedChanged);
			// 
			// radioButtonScanline
			// 
			this.radioButtonScanline.AutoSize = true;
			this.radioButtonScanline.Location = new System.Drawing.Point(6, 19);
			this.radioButtonScanline.Name = "radioButtonScanline";
			this.radioButtonScanline.Size = new System.Drawing.Size(66, 17);
			this.radioButtonScanline.TabIndex = 1;
			this.radioButtonScanline.Text = "Scanline";
			this.radioButtonScanline.UseVisualStyleBackColor = true;
			this.radioButtonScanline.CheckedChanged += new System.EventHandler(this.radioButtonScanline_CheckedChanged);
			// 
			// labelClipboard
			// 
			this.labelClipboard.Location = new System.Drawing.Point(12, 439);
			this.labelClipboard.MaximumSize = new System.Drawing.Size(145, 0);
			this.labelClipboard.Name = "labelClipboard";
			this.labelClipboard.Text = "CTRL + C: Copy under mouse to clipboard.";
			// 
			// timerMessage
			// 
			this.timerMessage.Interval = 5000;
			this.timerMessage.Tick += new System.EventHandler(this.timerMessage_Tick);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(636, 24);
			this.menuStrip1.TabIndex = 6;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// GbaGpuView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(636, 474);
			this.Controls.Add(this.labelClipboard);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.buttonShowWidget);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.listBoxWidgets);
			this.Controls.Add(this.menuStrip1);
			this.KeyPreview = true;
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "GbaGpuView";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GbaGpuView_FormClosed);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GbaGpuView_KeyDown);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.updownBGPal)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.updownSpritePal)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox listBoxWidgets;
		private System.Windows.Forms.Panel panel1;
		private Pixtro.WinForms.Controls.LocLabelEx label1;
		private System.Windows.Forms.Button buttonShowWidget;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button buttonRefresh;
		private System.Windows.Forms.HScrollBar hScrollBar1;
		private System.Windows.Forms.RadioButton radioButtonManual;
		private System.Windows.Forms.RadioButton radioButtonScanline;
		private Pixtro.WinForms.Controls.LocLabelEx labelClipboard;
		private System.Windows.Forms.Timer timerMessage;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.NumericUpDown updownBGPal;
		private WinForms.Controls.LocLabelEx locLabelEx2;
		private System.Windows.Forms.NumericUpDown updownSpritePal;
		private WinForms.Controls.LocLabelEx locLabelEx1;
	}
}