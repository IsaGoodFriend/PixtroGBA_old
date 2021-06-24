using Pixtro.WinForms.Controls;

namespace Pixtro.Client.Editor
{
	partial class VirtualpadTool
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
			this.ControllerBox = new System.Windows.Forms.GroupBox();
			this.PadBoxContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.clearAllToolStripMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.StickyContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ControllerPanel = new System.Windows.Forms.Panel();
			this.PadMenu = new MenuStripEx();
			this.PadsSubMenu = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ClearAllMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.StickyMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripSeparator4 = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.ExitMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.SettingsSubMenu = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ClearClearsAnalogInputMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ControllerBox.SuspendLayout();
			this.PadBoxContextMenu.SuspendLayout();
			this.PadMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// ControllerBox
			// 
			this.ControllerBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ControllerBox.ContextMenuStrip = this.PadBoxContextMenu;
			this.ControllerBox.Controls.Add(this.ControllerPanel);
			this.ControllerBox.Location = new System.Drawing.Point(12, 27);
			this.ControllerBox.Name = "ControllerBox";
			this.ControllerBox.Size = new System.Drawing.Size(431, 277);
			this.ControllerBox.TabIndex = 11;
			this.ControllerBox.TabStop = false;
			this.ControllerBox.Text = "Controllers";
			// 
			// PadBoxContextMenu
			// 
			this.PadBoxContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearAllToolStripMenuItem,
            this.StickyContextMenuItem});
			this.PadBoxContextMenu.Name = "PadBoxContextMenu";
			this.PadBoxContextMenu.Size = new System.Drawing.Size(143, 48);
			this.PadBoxContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.PadBoxContextMenu_Opening);
			// 
			// clearAllToolStripMenuItem
			// 
			this.clearAllToolStripMenuItem.ShortcutKeyDisplayString = "Del";
			this.clearAllToolStripMenuItem.Text = "Clear All";
			this.clearAllToolStripMenuItem.Click += new System.EventHandler(this.ClearAllMenuItem_Click);
			// 
			// StickyContextMenuItem
			// 
			this.StickyContextMenuItem.Text = "Sticky";
			this.StickyContextMenuItem.Click += new System.EventHandler(this.StickyMenuItem_Click);
			// 
			// ControllerPanel
			// 
			this.ControllerPanel.AutoScroll = true;
			this.ControllerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ControllerPanel.Location = new System.Drawing.Point(3, 16);
			this.ControllerPanel.Name = "ControllerPanel";
			this.ControllerPanel.Size = new System.Drawing.Size(425, 258);
			this.ControllerPanel.TabIndex = 0;
			// 
			// PadMenu
			// 
			this.PadMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PadsSubMenu,
            this.SettingsSubMenu});
			this.PadMenu.TabIndex = 7;
			// 
			// PadsSubMenu
			// 
			this.PadsSubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ClearAllMenuItem,
            this.StickyMenuItem,
            this.toolStripSeparator4,
            this.ExitMenuItem});
			this.PadsSubMenu.Text = "&Pads";
			this.PadsSubMenu.DropDownOpened += new System.EventHandler(this.PadsSubMenu_DropDownOpened);
			// 
			// ClearAllMenuItem
			// 
			this.ClearAllMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.ClearAllMenuItem.Text = "&Clear All";
			this.ClearAllMenuItem.Click += new System.EventHandler(this.ClearAllMenuItem_Click);
			// 
			// StickyMenuItem
			// 
			this.StickyMenuItem.Text = "Sticky";
			this.StickyMenuItem.Click += new System.EventHandler(this.StickyMenuItem_Click);
			// 
			// ExitMenuItem
			// 
			this.ExitMenuItem.ShortcutKeyDisplayString = "Alt+F4";
			this.ExitMenuItem.Text = "E&xit";
			this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
			// 
			// SettingsSubMenu
			// 
			this.SettingsSubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ClearClearsAnalogInputMenuItem});
			this.SettingsSubMenu.Text = "&Settings";
			this.SettingsSubMenu.DropDownOpened += new System.EventHandler(this.OptionsSubMenu_DropDownOpened);
			// 
			// ClearClearsAnalogInputMenuItem
			// 
			this.ClearClearsAnalogInputMenuItem.Text = "&Clear also clears Analog Input";
			this.ClearClearsAnalogInputMenuItem.Click += new System.EventHandler(this.ClearClearsAnalogInputMenuItem_Click);
			// 
			// VirtualpadTool
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(452, 312);
			this.Controls.Add(this.ControllerBox);
			this.Controls.Add(this.PadMenu);
			this.Name = "VirtualpadTool";
			this.Load += new System.EventHandler(this.VirtualpadTool_Load);
			this.ControllerBox.ResumeLayout(false);
			this.PadBoxContextMenu.ResumeLayout(false);
			this.PadMenu.ResumeLayout(false);
			this.PadMenu.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MenuStripEx PadMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx SettingsSubMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx PadsSubMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ClearAllMenuItem;
		private System.Windows.Forms.GroupBox ControllerBox;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx StickyMenuItem;
		private System.Windows.Forms.ContextMenuStrip PadBoxContextMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx clearAllToolStripMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx StickyContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ClearClearsAnalogInputMenuItem;
		private System.Windows.Forms.Panel ControllerPanel;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx toolStripSeparator4;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ExitMenuItem;
	}
}