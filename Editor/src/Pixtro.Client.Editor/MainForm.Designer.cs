using Pixtro.WinForms.Controls;

namespace Pixtro.Client.Editor
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.MainformMenu = new Pixtro.WinForms.Controls.MenuStripEx();
			this.FileSubMenu = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.NewProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.projectTemplatesWillGoHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.OpenProjectMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.CloseProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RecentProjectSubMenu = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripSeparator3 = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.toolStripMenuItem1 = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.SaveRAMSubMenu = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.FlushSaveRAMMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripMenuItem2 = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.ExitMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.EmulationSubMenu = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.PauseMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.RebootCoreMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripSeparator1 = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.SoftResetMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.HardResetMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.EmulatorMenuSeparator2 = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.LoadedCoreNameMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ProjectSubMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.BuildProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RunProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.BuildAndRunMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.BuildReleaseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ViewSubMenu = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.SwitchToFullscreenMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripSeparator2 = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.DisplayFPSMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.DisplayFrameCounterMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.DisplayLagCounterMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.DisplayInputMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.DisplayRerecordCountMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.DisplaySubtitlesMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripMenuItem4 = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.DisplayStatusBarMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.DisplayMessagesMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripSeparator8 = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.DisplayLogWindowMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ConfigSubMenu = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ControllersMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.HotkeysMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.DisplayConfigMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.SoundMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.PathsMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.FirmwaresMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.MessagesMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.RewindOptionsMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.extensionsToolStripMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ClientOptionsMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ProfilesMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripSeparator9 = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.SpeedSkipSubMenu = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ClockThrottleMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.AudioThrottleMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.VsyncThrottleMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripSeparator27 = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.VsyncEnabledMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripMenuItem3 = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.miUnthrottled = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.MinimizeSkippingMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.NeverSkipMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripMenuItem17 = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.Frameskip1MenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.Frameskip2MenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.Frameskip3MenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.Frameskip4MenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.Frameskip5MenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.Frameskip6MenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.Frameskip7MenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.Frameskip8MenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.Frameskip9MenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripMenuItem5 = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.Speed50MenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.Speed75MenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.Speed100MenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.Speed150MenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.Speed200MenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.Speed400MenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.KeyPrioritySubMenu = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.BothHkAndControllerMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.InputOverHkMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.HkOverInputMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.CoresSubMenu = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripSeparator10 = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.SaveConfigMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.SaveConfigAsMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.LoadConfigMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.LoadConfigFromMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ToolsSubMenu = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.RamWatchMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.RamSearchMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.HexEditorMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.CodeDataLoggerMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripSeparator29 = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.MultiDiskBundlerFileMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ExternalToolMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.dummyExternalTool = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.BatchRunnerMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.GenericCoreSubMenu = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.HelpSubMenu = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.OnlineHelpMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ForumsMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.FeaturesMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.AboutMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.A7800HawkCoreMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.MainStatusBar = new Pixtro.WinForms.Controls.StatusStripEx();
			this.AVIStatusLabel = new Pixtro.WinForms.Controls.StatusLabelEx();
			this.EmuStatus = new Pixtro.WinForms.Controls.StatusLabelEx();
			this.EditorLayoutSubmenu = new System.Windows.Forms.ToolStripDropDownButton();
			this.baseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.PlayRecordStatusButton = new System.Windows.Forms.ToolStripDropDownButton();
			this.PauseStatusButton = new System.Windows.Forms.ToolStripDropDownButton();
			this.RebootStatusBarIcon = new Pixtro.WinForms.Controls.StatusLabelEx();
			this.LedLightStatusLabel = new Pixtro.WinForms.Controls.StatusLabelEx();
			this.CheatStatusButton = new Pixtro.WinForms.Controls.StatusLabelEx();
			this.KeyPriorityStatusLabel = new Pixtro.WinForms.Controls.StatusLabelEx();
			this.ProfileFirstBootLabel = new Pixtro.WinForms.Controls.StatusLabelEx();
			this.LinkConnectStatusBarButton = new Pixtro.WinForms.Controls.StatusLabelEx();
			this.UpdateNotification = new Pixtro.WinForms.Controls.StatusLabelEx();
			this.MainFormContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.OpenRomContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.LoadLastRomContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.StopAVContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ContextSeparator_AfterROM = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.RecordMovieContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.PlayMovieContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.RestartMovieContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.StopMovieContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.LoadLastMovieContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.BackupMovieContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.StopNoSaveContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ViewSubtitlesContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.AddSubtitleContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ViewCommentsContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.SaveMovieContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.SaveMovieAsContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ContextSeparator_AfterMovie = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.UndoSavestateContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ContextSeparator_AfterUndo = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.ConfigContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripMenuItem6 = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripMenuItem7 = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripMenuItem8 = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripMenuItem9 = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripMenuItem10 = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripMenuItem11 = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripMenuItem12 = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripMenuItem13 = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripMenuItem14 = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripMenuItem15 = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.customizeToolStripMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripSeparator30 = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.toolStripMenuItem66 = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.toolStripMenuItem67 = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ScreenshotContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.CloseRomContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ClearSRAMContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.ShowMenuContextMenuSeparator = new Pixtro.WinForms.Controls.ToolStripSeparatorEx();
			this.ShowMenuContextMenuItem = new Pixtro.WinForms.Controls.ToolStripMenuItemEx();
			this.MainformMenu.SuspendLayout();
			this.MainStatusBar.SuspendLayout();
			this.MainFormContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainformMenu
			// 
			this.MainformMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileSubMenu,
            this.EmulationSubMenu,
            this.ProjectSubMenu,
            this.ViewSubMenu,
            this.ConfigSubMenu,
            this.ToolsSubMenu,
            this.GenericCoreSubMenu,
            this.HelpSubMenu});
			this.MainformMenu.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			this.MainformMenu.TabIndex = 0;
			this.MainformMenu.MenuActivate += new System.EventHandler(this.MainformMenu_MenuActivate);
			this.MainformMenu.MenuDeactivate += new System.EventHandler(this.MainformMenu_MenuDeactivate);
			// 
			// FileSubMenu
			// 
			this.FileSubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewProjectMenuItem,
            this.OpenProjectMenuItem,
            this.CloseProjectMenuItem,
            this.RecentProjectSubMenu,
            this.toolStripMenuItem1,
            this.SaveRAMSubMenu,
            this.toolStripMenuItem2,
            this.ExitMenuItem});
			this.FileSubMenu.Text = "&File";
			this.FileSubMenu.DropDownOpened += new System.EventHandler(this.FileSubMenu_DropDownOpened);
			// 
			// NewProjectMenuItem
			// 
			this.NewProjectMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectTemplatesWillGoHereToolStripMenuItem});
			this.NewProjectMenuItem.Name = "NewProjectMenuItem";
			this.NewProjectMenuItem.Size = new System.Drawing.Size(150, 22);
			this.NewProjectMenuItem.Text = "&New Project";
			// 
			// projectTemplatesWillGoHereToolStripMenuItem
			// 
			this.projectTemplatesWillGoHereToolStripMenuItem.Enabled = false;
			this.projectTemplatesWillGoHereToolStripMenuItem.Name = "projectTemplatesWillGoHereToolStripMenuItem";
			this.projectTemplatesWillGoHereToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
			this.projectTemplatesWillGoHereToolStripMenuItem.Text = "Project templates will go here";
			// 
			// OpenProjectMenuItem
			// 
			this.OpenProjectMenuItem.Text = "&Open Project";
			this.OpenProjectMenuItem.Click += new System.EventHandler(this.OpenRomMenuItem_Click);
			// 
			// CloseProjectMenuItem
			// 
			this.CloseProjectMenuItem.Name = "CloseProjectMenuItem";
			this.CloseProjectMenuItem.Size = new System.Drawing.Size(150, 22);
			this.CloseProjectMenuItem.Text = "&Close Project";
			this.CloseProjectMenuItem.Click += new System.EventHandler(this.CloseProjectMenuItem_Click);
			// 
			// RecentProjectSubMenu
			// 
			this.RecentProjectSubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator3});
			this.RecentProjectSubMenu.Text = "&Recent Project";
			this.RecentProjectSubMenu.DropDownOpened += new System.EventHandler(this.RecentProjectMenuItem_DropDownOpened);
			// 
			// SaveRAMSubMenu
			// 
			this.SaveRAMSubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FlushSaveRAMMenuItem});
			this.SaveRAMSubMenu.Text = "Save &RAM";
			this.SaveRAMSubMenu.DropDownOpened += new System.EventHandler(this.SaveRamSubMenu_DropDownOpened);
			// 
			// FlushSaveRAMMenuItem
			// 
			this.FlushSaveRAMMenuItem.Text = "&Flush Save Ram";
			this.FlushSaveRAMMenuItem.Click += new System.EventHandler(this.FlushSaveRAMMenuItem_Click);
			// 
			// ExitMenuItem
			// 
			this.ExitMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.ExitMenuItem.Text = "E&xit";
			this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
			// 
			// EmulationSubMenu
			// 
			this.EmulationSubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PauseMenuItem,
            this.RebootCoreMenuItem,
            this.toolStripSeparator1,
            this.SoftResetMenuItem,
            this.HardResetMenuItem,
            this.EmulatorMenuSeparator2,
            this.LoadedCoreNameMenuItem});
			this.EmulationSubMenu.Text = "&Emulation";
			this.EmulationSubMenu.DropDownOpened += new System.EventHandler(this.EmulationMenuItem_DropDownOpened);
			// 
			// PauseMenuItem
			// 
			this.PauseMenuItem.Text = "&Pause";
			this.PauseMenuItem.Click += new System.EventHandler(this.PauseMenuItem_Click);
			// 
			// RebootCoreMenuItem
			// 
			this.RebootCoreMenuItem.Text = "&Reboot Core";
			this.RebootCoreMenuItem.Click += new System.EventHandler(this.PowerMenuItem_Click);
			// 
			// SoftResetMenuItem
			// 
			this.SoftResetMenuItem.Text = "&Soft Reset";
			this.SoftResetMenuItem.Click += new System.EventHandler(this.SoftResetMenuItem_Click);
			// 
			// HardResetMenuItem
			// 
			this.HardResetMenuItem.Text = "&Hard Reset";
			this.HardResetMenuItem.Click += new System.EventHandler(this.HardResetMenuItem_Click);
			// 
			// LoadedCoreNameMenuItem
			// 
			this.LoadedCoreNameMenuItem.Enabled = false;
			this.LoadedCoreNameMenuItem.Text = "Loaded core: <core name> (sysID)";
			// 
			// ProjectSubMenu
			// 
			this.ProjectSubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BuildProjectMenuItem,
            this.RunProjectMenuItem,
            this.BuildAndRunMenuItem,
            this.BuildReleaseMenuItem});
			this.ProjectSubMenu.Enabled = false;
			this.ProjectSubMenu.Name = "ProjectSubMenu";
			this.ProjectSubMenu.Size = new System.Drawing.Size(56, 19);
			this.ProjectSubMenu.Text = "&Project";
			this.ProjectSubMenu.DropDownOpened += new System.EventHandler(this.ProjectSubMenu_DropDownOpened);
			// 
			// BuildProjectMenuItem
			// 
			this.BuildProjectMenuItem.Name = "BuildProjectMenuItem";
			this.BuildProjectMenuItem.Size = new System.Drawing.Size(150, 22);
			this.BuildProjectMenuItem.Text = "&Build Project";
			this.BuildProjectMenuItem.Click += new System.EventHandler(this.BuildProjectMenuItem_Click);
			// 
			// RunProjectMenuItem
			// 
			this.RunProjectMenuItem.Name = "RunProjectMenuItem";
			this.RunProjectMenuItem.Size = new System.Drawing.Size(150, 22);
			this.RunProjectMenuItem.Text = "&Run Project";
			this.RunProjectMenuItem.Click += new System.EventHandler(this.RunProjectMenuItem_Click);
			// 
			// BuildAndRunMenuItem
			// 
			this.BuildAndRunMenuItem.Name = "BuildAndRunMenuItem";
			this.BuildAndRunMenuItem.Size = new System.Drawing.Size(150, 22);
			this.BuildAndRunMenuItem.Text = "Build And Run";
			this.BuildAndRunMenuItem.Click += new System.EventHandler(this.BuildAndRunMenuItem_Click);
			// 
			// BuildReleaseMenuItem
			// 
			this.BuildReleaseMenuItem.Name = "BuildReleaseMenuItem";
			this.BuildReleaseMenuItem.Size = new System.Drawing.Size(150, 22);
			this.BuildReleaseMenuItem.Text = "Build Release";
			this.BuildReleaseMenuItem.Click += new System.EventHandler(this.BuildReleaseMenuItem_Click);
			// 
			// ViewSubMenu
			// 
			this.ViewSubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SwitchToFullscreenMenuItem,
            this.toolStripSeparator2,
            this.DisplayFPSMenuItem,
            this.DisplayFrameCounterMenuItem,
            this.DisplayLagCounterMenuItem,
            this.DisplayInputMenuItem,
            this.DisplayRerecordCountMenuItem,
            this.DisplaySubtitlesMenuItem,
            this.toolStripMenuItem4,
            this.DisplayStatusBarMenuItem,
            this.DisplayMessagesMenuItem,
            this.toolStripSeparator8,
            this.DisplayLogWindowMenuItem});
			this.ViewSubMenu.Text = "&View";
			this.ViewSubMenu.DropDownOpened += new System.EventHandler(this.ViewSubMenu_DropDownOpened);
			// 
			// SwitchToFullscreenMenuItem
			// 
			this.SwitchToFullscreenMenuItem.Text = "Switch to Fullscreen";
			this.SwitchToFullscreenMenuItem.Click += new System.EventHandler(this.SwitchToFullscreenMenuItem_Click);
			// 
			// DisplayFPSMenuItem
			// 
			this.DisplayFPSMenuItem.Text = "Display FPS";
			this.DisplayFPSMenuItem.Click += new System.EventHandler(this.DisplayFpsMenuItem_Click);
			// 
			// DisplayFrameCounterMenuItem
			// 
			this.DisplayFrameCounterMenuItem.Text = "Display FrameCounter";
			this.DisplayFrameCounterMenuItem.Click += new System.EventHandler(this.DisplayFrameCounterMenuItem_Click);
			// 
			// DisplayLagCounterMenuItem
			// 
			this.DisplayLagCounterMenuItem.Text = "Display Lag Counter";
			this.DisplayLagCounterMenuItem.Click += new System.EventHandler(this.DisplayLagCounterMenuItem_Click);
			// 
			// DisplayInputMenuItem
			// 
			this.DisplayInputMenuItem.Text = "Display Input";
			this.DisplayInputMenuItem.Click += new System.EventHandler(this.DisplayInputMenuItem_Click);
			// 
			// DisplayRerecordCountMenuItem
			// 
			this.DisplayRerecordCountMenuItem.Text = "Display Rerecord Count";
			this.DisplayRerecordCountMenuItem.Click += new System.EventHandler(this.DisplayRerecordsMenuItem_Click);
			// 
			// DisplaySubtitlesMenuItem
			// 
			this.DisplaySubtitlesMenuItem.Text = "Display Subtitles";
			this.DisplaySubtitlesMenuItem.Click += new System.EventHandler(this.DisplaySubtitlesMenuItem_Click);
			// 
			// DisplayStatusBarMenuItem
			// 
			this.DisplayStatusBarMenuItem.Text = "Display Status Bar";
			this.DisplayStatusBarMenuItem.Click += new System.EventHandler(this.DisplayStatusBarMenuItem_Click);
			// 
			// DisplayMessagesMenuItem
			// 
			this.DisplayMessagesMenuItem.Text = "Display Messages";
			this.DisplayMessagesMenuItem.Click += new System.EventHandler(this.DisplayMessagesMenuItem_Click);
			// 
			// DisplayLogWindowMenuItem
			// 
			this.DisplayLogWindowMenuItem.Text = "Open &Log Window...";
			this.DisplayLogWindowMenuItem.Click += new System.EventHandler(this.DisplayLogWindowMenuItem_Click);
			// 
			// ConfigSubMenu
			// 
			this.ConfigSubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ControllersMenuItem,
            this.HotkeysMenuItem,
            this.DisplayConfigMenuItem,
            this.SoundMenuItem,
            this.PathsMenuItem,
            this.FirmwaresMenuItem,
            this.MessagesMenuItem,
            this.RewindOptionsMenuItem,
            this.extensionsToolStripMenuItem,
            this.ClientOptionsMenuItem,
            this.ProfilesMenuItem,
            this.toolStripSeparator9,
            this.SpeedSkipSubMenu,
            this.KeyPrioritySubMenu,
            this.CoresSubMenu,
            this.toolStripSeparator10,
            this.SaveConfigMenuItem,
            this.SaveConfigAsMenuItem,
            this.LoadConfigMenuItem,
            this.LoadConfigFromMenuItem});
			this.ConfigSubMenu.Text = "&Config";
			this.ConfigSubMenu.DropDownOpened += new System.EventHandler(this.ConfigSubMenu_DropDownOpened);
			// 
			// ControllersMenuItem
			// 
			this.ControllersMenuItem.Text = "&Controllers...";
			this.ControllersMenuItem.Click += new System.EventHandler(this.ControllersMenuItem_Click);
			// 
			// HotkeysMenuItem
			// 
			this.HotkeysMenuItem.Text = "&Hotkeys...";
			this.HotkeysMenuItem.Click += new System.EventHandler(this.HotkeysMenuItem_Click);
			// 
			// DisplayConfigMenuItem
			// 
			this.DisplayConfigMenuItem.Text = "Display...";
			this.DisplayConfigMenuItem.Click += new System.EventHandler(this.DisplayConfigMenuItem_Click);
			// 
			// SoundMenuItem
			// 
			this.SoundMenuItem.Text = "&Sound...";
			this.SoundMenuItem.Click += new System.EventHandler(this.SoundMenuItem_Click);
			// 
			// PathsMenuItem
			// 
			this.PathsMenuItem.Text = "Paths...";
			this.PathsMenuItem.Click += new System.EventHandler(this.PathsMenuItem_Click);
			// 
			// FirmwaresMenuItem
			// 
			this.FirmwaresMenuItem.Text = "&Firmwares...";
			this.FirmwaresMenuItem.Click += new System.EventHandler(this.FirmwaresMenuItem_Click);
			// 
			// MessagesMenuItem
			// 
			this.MessagesMenuItem.Text = "&Messages...";
			this.MessagesMenuItem.Click += new System.EventHandler(this.MessagesMenuItem_Click);
			// 
			// RewindOptionsMenuItem
			// 
			this.RewindOptionsMenuItem.Text = "&Rewind && States...";
			this.RewindOptionsMenuItem.Click += new System.EventHandler(this.RewindOptionsMenuItem_Click);
			// 
			// extensionsToolStripMenuItem
			// 
			this.extensionsToolStripMenuItem.Text = "File Extensions...";
			this.extensionsToolStripMenuItem.Click += new System.EventHandler(this.FileExtensionsMenuItem_Click);
			// 
			// ClientOptionsMenuItem
			// 
			this.ClientOptionsMenuItem.Text = "&Customize...";
			this.ClientOptionsMenuItem.Click += new System.EventHandler(this.CustomizeMenuItem_Click);
			// 
			// ProfilesMenuItem
			// 
			this.ProfilesMenuItem.Text = "&Profiles...";
			this.ProfilesMenuItem.Click += new System.EventHandler(this.ProfilesMenuItem_Click);
			// 
			// SpeedSkipSubMenu
			// 
			this.SpeedSkipSubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ClockThrottleMenuItem,
            this.AudioThrottleMenuItem,
            this.VsyncThrottleMenuItem,
            this.toolStripSeparator27,
            this.VsyncEnabledMenuItem,
            this.toolStripMenuItem3,
            this.miUnthrottled,
            this.MinimizeSkippingMenuItem,
            this.NeverSkipMenuItem,
            this.toolStripMenuItem17,
            this.toolStripMenuItem5,
            this.Speed50MenuItem,
            this.Speed75MenuItem,
            this.Speed100MenuItem,
            this.Speed150MenuItem,
            this.Speed200MenuItem,
            this.Speed400MenuItem});
			this.SpeedSkipSubMenu.Text = "Speed/Skip";
			this.SpeedSkipSubMenu.DropDownOpened += new System.EventHandler(this.FrameSkipMenuItem_DropDownOpened);
			// 
			// ClockThrottleMenuItem
			// 
			this.ClockThrottleMenuItem.Text = "Clock Throttle";
			this.ClockThrottleMenuItem.Click += new System.EventHandler(this.ClockThrottleMenuItem_Click);
			// 
			// AudioThrottleMenuItem
			// 
			this.AudioThrottleMenuItem.Text = "Audio Throttle";
			this.AudioThrottleMenuItem.Click += new System.EventHandler(this.AudioThrottleMenuItem_Click);
			// 
			// VsyncThrottleMenuItem
			// 
			this.VsyncThrottleMenuItem.Text = "VSync Throttle";
			this.VsyncThrottleMenuItem.Click += new System.EventHandler(this.VsyncThrottleMenuItem_Click);
			// 
			// VsyncEnabledMenuItem
			// 
			this.VsyncEnabledMenuItem.Text = "VSync Enabled";
			this.VsyncEnabledMenuItem.Click += new System.EventHandler(this.VsyncEnabledMenuItem_Click);
			// 
			// miUnthrottled
			// 
			this.miUnthrottled.Text = "Unthrottled";
			this.miUnthrottled.Click += new System.EventHandler(this.UnthrottledMenuItem_Click);
			// 
			// MinimizeSkippingMenuItem
			// 
			this.MinimizeSkippingMenuItem.Text = "Auto-minimize skipping";
			this.MinimizeSkippingMenuItem.Click += new System.EventHandler(this.MinimizeSkippingMenuItem_Click);
			// 
			// NeverSkipMenuItem
			// 
			this.NeverSkipMenuItem.Text = "Skip 0 (never)";
			this.NeverSkipMenuItem.Click += new System.EventHandler(this.NeverSkipMenuItem_Click);
			// 
			// toolStripMenuItem17
			// 
			this.toolStripMenuItem17.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Frameskip1MenuItem,
            this.Frameskip2MenuItem,
            this.Frameskip3MenuItem,
            this.Frameskip4MenuItem,
            this.Frameskip5MenuItem,
            this.Frameskip6MenuItem,
            this.Frameskip7MenuItem,
            this.Frameskip8MenuItem,
            this.Frameskip9MenuItem});
			this.toolStripMenuItem17.Text = "Skip 1..9";
			// 
			// Frameskip1MenuItem
			// 
			this.Frameskip1MenuItem.Text = "1";
			this.Frameskip1MenuItem.Click += new System.EventHandler(this.Frameskip1MenuItem_Click);
			// 
			// Frameskip2MenuItem
			// 
			this.Frameskip2MenuItem.Text = "2";
			this.Frameskip2MenuItem.Click += new System.EventHandler(this.Frameskip2MenuItem_Click);
			// 
			// Frameskip3MenuItem
			// 
			this.Frameskip3MenuItem.Text = "3";
			this.Frameskip3MenuItem.Click += new System.EventHandler(this.Frameskip3MenuItem_Click);
			// 
			// Frameskip4MenuItem
			// 
			this.Frameskip4MenuItem.Text = "4";
			this.Frameskip4MenuItem.Click += new System.EventHandler(this.Frameskip4MenuItem_Click);
			// 
			// Frameskip5MenuItem
			// 
			this.Frameskip5MenuItem.Text = "5";
			this.Frameskip5MenuItem.Click += new System.EventHandler(this.Frameskip5MenuItem_Click);
			// 
			// Frameskip6MenuItem
			// 
			this.Frameskip6MenuItem.Text = "6";
			this.Frameskip6MenuItem.Click += new System.EventHandler(this.Frameskip6MenuItem_Click);
			// 
			// Frameskip7MenuItem
			// 
			this.Frameskip7MenuItem.Text = "7";
			this.Frameskip7MenuItem.Click += new System.EventHandler(this.Frameskip7MenuItem_Click);
			// 
			// Frameskip8MenuItem
			// 
			this.Frameskip8MenuItem.Text = "8";
			this.Frameskip8MenuItem.Click += new System.EventHandler(this.Frameskip8MenuItem_Click);
			// 
			// Frameskip9MenuItem
			// 
			this.Frameskip9MenuItem.Text = "9";
			this.Frameskip9MenuItem.Click += new System.EventHandler(this.Frameskip9MenuItem_Click);
			// 
			// Speed50MenuItem
			// 
			this.Speed50MenuItem.Text = "Speed 50%";
			this.Speed50MenuItem.Click += new System.EventHandler(this.Speed50MenuItem_Click);
			// 
			// Speed75MenuItem
			// 
			this.Speed75MenuItem.Text = "Speed 75%";
			this.Speed75MenuItem.Click += new System.EventHandler(this.Speed75MenuItem_Click);
			// 
			// Speed100MenuItem
			// 
			this.Speed100MenuItem.Text = "Speed 100%";
			this.Speed100MenuItem.Click += new System.EventHandler(this.Speed100MenuItem_Click);
			// 
			// Speed150MenuItem
			// 
			this.Speed150MenuItem.Text = "Speed 150%";
			this.Speed150MenuItem.Click += new System.EventHandler(this.Speed150MenuItem_Click);
			// 
			// Speed200MenuItem
			// 
			this.Speed200MenuItem.Text = "Speed 200%";
			this.Speed200MenuItem.Click += new System.EventHandler(this.Speed200MenuItem_Click);
			// 
			// Speed400MenuItem
			// 
			this.Speed400MenuItem.Text = "Speed 400%";
			this.Speed400MenuItem.Click += new System.EventHandler(this.Speed400MenuItem_Click);
			// 
			// KeyPrioritySubMenu
			// 
			this.KeyPrioritySubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BothHkAndControllerMenuItem,
            this.InputOverHkMenuItem,
            this.HkOverInputMenuItem});
			this.KeyPrioritySubMenu.Text = "Key Priority";
			this.KeyPrioritySubMenu.DropDownOpened += new System.EventHandler(this.KeyPriorityMenuItem_DropDownOpened);
			// 
			// BothHkAndControllerMenuItem
			// 
			this.BothHkAndControllerMenuItem.Text = "Both Hotkeys and Controllers";
			this.BothHkAndControllerMenuItem.Click += new System.EventHandler(this.BothHkAndControllerMenuItem_Click);
			// 
			// InputOverHkMenuItem
			// 
			this.InputOverHkMenuItem.Text = "Input overrides Hotkeys";
			this.InputOverHkMenuItem.Click += new System.EventHandler(this.InputOverHkMenuItem_Click);
			// 
			// HkOverInputMenuItem
			// 
			this.HkOverInputMenuItem.Text = "Hotkeys override Input";
			this.HkOverInputMenuItem.Click += new System.EventHandler(this.HkOverInputMenuItem_Click);
			// 
			// CoresSubMenu
			// 
			this.CoresSubMenu.Text = "Cores";
			// 
			// SaveConfigMenuItem
			// 
			this.SaveConfigMenuItem.Text = "Save Config";
			this.SaveConfigMenuItem.Click += new System.EventHandler(this.SaveConfigMenuItem_Click);
			// 
			// SaveConfigAsMenuItem
			// 
			this.SaveConfigAsMenuItem.Text = "Save Config As...";
			this.SaveConfigAsMenuItem.Click += new System.EventHandler(this.SaveConfigAsMenuItem_Click);
			// 
			// LoadConfigMenuItem
			// 
			this.LoadConfigMenuItem.Text = "Load Config";
			this.LoadConfigMenuItem.Click += new System.EventHandler(this.LoadConfigMenuItem_Click);
			// 
			// LoadConfigFromMenuItem
			// 
			this.LoadConfigFromMenuItem.Text = "Load Config From...";
			this.LoadConfigFromMenuItem.Click += new System.EventHandler(this.LoadConfigFromMenuItem_Click);
			// 
			// ToolsSubMenu
			// 
			this.ToolsSubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RamWatchMenuItem,
            this.RamSearchMenuItem,
            this.HexEditorMenuItem,
            this.CodeDataLoggerMenuItem,
            this.toolStripSeparator29,
            this.MultiDiskBundlerFileMenuItem,
            this.ExternalToolMenuItem,
            this.BatchRunnerMenuItem});
			this.ToolsSubMenu.Text = "&Tools";
			this.ToolsSubMenu.DropDownOpened += new System.EventHandler(this.ToolsSubMenu_DropDownOpened);
			// 
			// RamWatchMenuItem
			// 
			this.RamWatchMenuItem.Text = "RAM &Watch";
			this.RamWatchMenuItem.Click += new System.EventHandler(this.RamWatchMenuItem_Click);
			// 
			// RamSearchMenuItem
			// 
			this.RamSearchMenuItem.Text = "RAM &Search";
			this.RamSearchMenuItem.Click += new System.EventHandler(this.RamSearchMenuItem_Click);
			// 
			// HexEditorMenuItem
			// 
			this.HexEditorMenuItem.Text = "&Hex Editor";
			this.HexEditorMenuItem.Click += new System.EventHandler(this.HexEditorMenuItem_Click);
			// 
			// CodeDataLoggerMenuItem
			// 
			this.CodeDataLoggerMenuItem.Text = "Code-Data Logger";
			this.CodeDataLoggerMenuItem.Click += new System.EventHandler(this.CodeDataLoggerMenuItem_Click);
			// 
			// MultiDiskBundlerFileMenuItem
			// 
			this.MultiDiskBundlerFileMenuItem.Text = "Multi-disk Bundler";
			this.MultiDiskBundlerFileMenuItem.Click += new System.EventHandler(this.MultidiskBundlerMenuItem_Click);
			// 
			// ExternalToolMenuItem
			// 
			this.ExternalToolMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dummyExternalTool});
			this.ExternalToolMenuItem.Text = "External Tool";
			this.ExternalToolMenuItem.DropDownOpening += new System.EventHandler(this.ExternalToolMenuItem_DropDownOpening);
			// 
			// dummyExternalTool
			// 
			this.dummyExternalTool.Text = "None";
			// 
			// BatchRunnerMenuItem
			// 
			this.BatchRunnerMenuItem.Text = "Batch Runner";
			this.BatchRunnerMenuItem.Visible = false;
			this.BatchRunnerMenuItem.Click += new System.EventHandler(this.BatchRunnerMenuItem_Click);
			// 
			// GenericCoreSubMenu
			// 
			this.GenericCoreSubMenu.Text = "&Core";
			// 
			// HelpSubMenu
			// 
			this.HelpSubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OnlineHelpMenuItem,
            this.ForumsMenuItem,
            this.FeaturesMenuItem,
            this.AboutMenuItem});
			this.HelpSubMenu.Text = "&Help";
			this.HelpSubMenu.DropDownOpened += new System.EventHandler(this.HelpSubMenu_DropDownOpened);
			// 
			// OnlineHelpMenuItem
			// 
			this.OnlineHelpMenuItem.Text = "&Online Help...";
			this.OnlineHelpMenuItem.Click += new System.EventHandler(this.OnlineHelpMenuItem_Click);
			// 
			// ForumsMenuItem
			// 
			this.ForumsMenuItem.Text = "Forums...";
			this.ForumsMenuItem.Click += new System.EventHandler(this.ForumsMenuItem_Click);
			// 
			// FeaturesMenuItem
			// 
			this.FeaturesMenuItem.Text = "&Features";
			this.FeaturesMenuItem.Click += new System.EventHandler(this.FeaturesMenuItem_Click);
			// 
			// AboutMenuItem
			// 
			this.AboutMenuItem.Text = "&About";
			this.AboutMenuItem.Click += new System.EventHandler(this.AboutMenuItem_Click);
			// 
			// A7800HawkCoreMenuItem
			// 
			this.A7800HawkCoreMenuItem.Text = "A7800Hawk";
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AVIStatusLabel,
            this.EmuStatus,
            this.EditorLayoutSubmenu,
            this.PlayRecordStatusButton,
            this.PauseStatusButton,
            this.RebootStatusBarIcon,
            this.LedLightStatusLabel,
            this.CheatStatusButton,
            this.KeyPriorityStatusLabel,
            this.ProfileFirstBootLabel,
            this.LinkConnectStatusBarButton,
            this.UpdateNotification});
			this.MainStatusBar.Location = new System.Drawing.Point(0, 425);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.ShowItemToolTips = true;
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 1;
			// 
			// AVIStatusLabel
			// 
			this.AVIStatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.AVIStatusLabel.Text = "AVI Capture";
			// 
			// EditorLayoutSubmenu
			// 
			this.EditorLayoutSubmenu.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.EditorLayoutSubmenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.baseToolStripMenuItem});
			this.EditorLayoutSubmenu.Name = "EditorLayoutSubmenu";
			this.EditorLayoutSubmenu.Size = new System.Drawing.Size(44, 20);
			this.EditorLayoutSubmenu.Text = "Base";
			this.EditorLayoutSubmenu.ToolTipText = "Change the layout of the editor";
			// 
			// baseToolStripMenuItem
			// 
			this.baseToolStripMenuItem.Name = "baseToolStripMenuItem";
			this.baseToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
			this.baseToolStripMenuItem.Text = "Base";
			// 
			// PlayRecordStatusButton
			// 
			this.PlayRecordStatusButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.PlayRecordStatusButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.PlayRecordStatusButton.Name = "PlayRecordStatusButton";
			this.PlayRecordStatusButton.ShowDropDownArrow = false;
			this.PlayRecordStatusButton.Size = new System.Drawing.Size(4, 20);
			this.PlayRecordStatusButton.Text = "No movie is active";
			// 
			// PauseStatusButton
			// 
			this.PauseStatusButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.PauseStatusButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.PauseStatusButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.PauseStatusButton.Name = "PauseStatusButton";
			this.PauseStatusButton.ShowDropDownArrow = false;
			this.PauseStatusButton.Size = new System.Drawing.Size(4, 20);
			this.PauseStatusButton.Text = "toolStripDropDownButton1";
			this.PauseStatusButton.ToolTipText = "Emulator is paused";
			this.PauseStatusButton.Click += new System.EventHandler(this.PauseMenuItem_Click);
			// 
			// RebootStatusBarIcon
			// 
			this.RebootStatusBarIcon.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.RebootStatusBarIcon.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RebootStatusBarIcon.Text = "Reboot";
			this.RebootStatusBarIcon.ToolTipText = "A reboot of the core is needed for a setting change to take effect";
			this.RebootStatusBarIcon.Click += new System.EventHandler(this.PowerMenuItem_Click);
			// 
			// LedLightStatusLabel
			// 
			this.LedLightStatusLabel.ToolTipText = "Disk Drive LED Light";
			// 
			// CheatStatusButton
			// 
			this.CheatStatusButton.Click += new System.EventHandler(this.FreezeStatus_Click);
			// 
			// KeyPriorityStatusLabel
			// 
			this.KeyPriorityStatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.KeyPriorityStatusLabel.Margin = new System.Windows.Forms.Padding(5, 3, 5, 0);
			this.KeyPriorityStatusLabel.Text = "KeyPriority";
			this.KeyPriorityStatusLabel.Click += new System.EventHandler(this.KeyPriorityStatusLabel_Click);
			// 
			// ProfileFirstBootLabel
			// 
			this.ProfileFirstBootLabel.AutoToolTip = true;
			this.ProfileFirstBootLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.ProfileFirstBootLabel.Text = "ProfileFirstBootLabel";
			this.ProfileFirstBootLabel.ToolTipText = "Set up your profile before use";
			this.ProfileFirstBootLabel.Visible = false;
			this.ProfileFirstBootLabel.Click += new System.EventHandler(this.ProfileFirstBootLabel_Click);
			// 
			// LinkConnectStatusBarButton
			// 
			this.LinkConnectStatusBarButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.LinkConnectStatusBarButton.Text = "Link connection is currently enabled";
			this.LinkConnectStatusBarButton.ToolTipText = "Link connection is currently enabled";
			this.LinkConnectStatusBarButton.Click += new System.EventHandler(this.LinkConnectStatusBarButton_Click);
			// 
			// UpdateNotification
			// 
			this.UpdateNotification.IsLink = true;
			this.UpdateNotification.LinkColor = System.Drawing.Color.Red;
			this.UpdateNotification.Spring = true;
			this.UpdateNotification.Text = "New version available!";
			this.UpdateNotification.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.UpdateNotification.Click += new System.EventHandler(this.UpdateNotification_Click);
			// 
			// MainFormContextMenu
			// 
			this.MainFormContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenRomContextMenuItem,
            this.LoadLastRomContextMenuItem,
            this.StopAVContextMenuItem,
            this.ContextSeparator_AfterROM,
            this.RecordMovieContextMenuItem,
            this.PlayMovieContextMenuItem,
            this.RestartMovieContextMenuItem,
            this.StopMovieContextMenuItem,
            this.LoadLastMovieContextMenuItem,
            this.BackupMovieContextMenuItem,
            this.StopNoSaveContextMenuItem,
            this.ViewSubtitlesContextMenuItem,
            this.AddSubtitleContextMenuItem,
            this.ViewCommentsContextMenuItem,
            this.SaveMovieContextMenuItem,
            this.SaveMovieAsContextMenuItem,
            this.ContextSeparator_AfterMovie,
            this.UndoSavestateContextMenuItem,
            this.ContextSeparator_AfterUndo,
            this.ConfigContextMenuItem,
            this.ScreenshotContextMenuItem,
            this.CloseRomContextMenuItem,
            this.ClearSRAMContextMenuItem,
            this.ShowMenuContextMenuSeparator,
            this.ShowMenuContextMenuItem});
			this.MainFormContextMenu.Name = "contextMenuStrip1";
			this.MainFormContextMenu.Size = new System.Drawing.Size(217, 490);
			this.MainFormContextMenu.Closing += new System.Windows.Forms.ToolStripDropDownClosingEventHandler(this.MainFormContextMenu_Closing);
			this.MainFormContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.MainFormContextMenu_Opening);
			// 
			// OpenRomContextMenuItem
			// 
			this.OpenRomContextMenuItem.Text = "Open Rom";
			this.OpenRomContextMenuItem.Click += new System.EventHandler(this.OpenRomMenuItem_Click);
			// 
			// LoadLastRomContextMenuItem
			// 
			this.LoadLastRomContextMenuItem.Text = "Load Last ROM";
			this.LoadLastRomContextMenuItem.Click += new System.EventHandler(this.LoadLastRomContextMenuItem_Click);
			// 
			// StopAVContextMenuItem
			// 
			this.StopAVContextMenuItem.Text = "Stop AVI/WAV";
			this.StopAVContextMenuItem.Click += new System.EventHandler(this.StopAVMenuItem_Click);
			// 
			// RecordMovieContextMenuItem
			// 
			this.RecordMovieContextMenuItem.Text = "Record Movie";
			this.RecordMovieContextMenuItem.Click += new System.EventHandler(this.RecordMovieMenuItem_Click);
			// 
			// PlayMovieContextMenuItem
			// 
			this.PlayMovieContextMenuItem.Text = "Play Movie";
			this.PlayMovieContextMenuItem.Click += new System.EventHandler(this.PlayMovieMenuItem_Click);
			// 
			// RestartMovieContextMenuItem
			// 
			this.RestartMovieContextMenuItem.Text = "Restart Movie";
			this.RestartMovieContextMenuItem.Click += new System.EventHandler(this.PlayFromBeginningMenuItem_Click);
			// 
			// StopMovieContextMenuItem
			// 
			this.StopMovieContextMenuItem.Text = "Stop Movie";
			this.StopMovieContextMenuItem.Click += new System.EventHandler(this.StopMovieMenuItem_Click);
			// 
			// LoadLastMovieContextMenuItem
			// 
			this.LoadLastMovieContextMenuItem.Text = "Load Last Movie";
			this.LoadLastMovieContextMenuItem.Click += new System.EventHandler(this.LoadLastMovieContextMenuItem_Click);
			// 
			// BackupMovieContextMenuItem
			// 
			this.BackupMovieContextMenuItem.Text = "Backup Movie";
			this.BackupMovieContextMenuItem.Click += new System.EventHandler(this.BackupMovieContextMenuItem_Click);
			// 
			// StopNoSaveContextMenuItem
			// 
			this.StopNoSaveContextMenuItem.Text = "Stop Movie without Saving";
			this.StopNoSaveContextMenuItem.Click += new System.EventHandler(this.StopMovieWithoutSavingMenuItem_Click);
			// 
			// ViewSubtitlesContextMenuItem
			// 
			this.ViewSubtitlesContextMenuItem.Text = "View Subtitles";
			this.ViewSubtitlesContextMenuItem.Click += new System.EventHandler(this.ViewSubtitlesContextMenuItem_Click);
			// 
			// AddSubtitleContextMenuItem
			// 
			this.AddSubtitleContextMenuItem.Text = "Add Subtitle";
			this.AddSubtitleContextMenuItem.Click += new System.EventHandler(this.AddSubtitleContextMenuItem_Click);
			// 
			// ViewCommentsContextMenuItem
			// 
			this.ViewCommentsContextMenuItem.Text = "View Comments";
			this.ViewCommentsContextMenuItem.Click += new System.EventHandler(this.ViewCommentsContextMenuItem_Click);
			// 
			// SaveMovieContextMenuItem
			// 
			this.SaveMovieContextMenuItem.Text = "Save Movie";
			this.SaveMovieContextMenuItem.Click += new System.EventHandler(this.SaveMovieMenuItem_Click);
			// 
			// SaveMovieAsContextMenuItem
			// 
			this.SaveMovieAsContextMenuItem.Text = "Save Movie As...";
			this.SaveMovieAsContextMenuItem.Click += new System.EventHandler(this.SaveMovieAsMenuItem_Click);
			// 
			// UndoSavestateContextMenuItem
			// 
			this.UndoSavestateContextMenuItem.Text = "Undo Savestate";
			this.UndoSavestateContextMenuItem.Click += new System.EventHandler(this.UndoSavestateContextMenuItem_Click);
			// 
			// ConfigContextMenuItem
			// 
			this.ConfigContextMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem6,
            this.toolStripMenuItem7,
            this.toolStripMenuItem8,
            this.toolStripMenuItem9,
            this.toolStripMenuItem10,
            this.toolStripMenuItem11,
            this.toolStripMenuItem12,
            this.toolStripMenuItem13,
            this.toolStripMenuItem14,
            this.toolStripMenuItem15,
            this.customizeToolStripMenuItem,
            this.toolStripSeparator30,
            this.toolStripMenuItem66,
            this.toolStripMenuItem67});
			this.ConfigContextMenuItem.Text = "Config";
			// 
			// toolStripMenuItem6
			// 
			this.toolStripMenuItem6.Text = "&Controllers...";
			this.toolStripMenuItem6.Click += new System.EventHandler(this.ControllersMenuItem_Click);
			// 
			// toolStripMenuItem7
			// 
			this.toolStripMenuItem7.Text = "&Hotkeys...";
			this.toolStripMenuItem7.Click += new System.EventHandler(this.HotkeysMenuItem_Click);
			// 
			// toolStripMenuItem8
			// 
			this.toolStripMenuItem8.Text = "Display...";
			this.toolStripMenuItem8.Click += new System.EventHandler(this.DisplayConfigMenuItem_Click);
			// 
			// toolStripMenuItem9
			// 
			this.toolStripMenuItem9.Text = "&Sound...";
			this.toolStripMenuItem9.Click += new System.EventHandler(this.SoundMenuItem_Click);
			// 
			// toolStripMenuItem10
			// 
			this.toolStripMenuItem10.Text = "Paths...";
			this.toolStripMenuItem10.Click += new System.EventHandler(this.PathsMenuItem_Click);
			// 
			// toolStripMenuItem11
			// 
			this.toolStripMenuItem11.Text = "&Firmwares...";
			this.toolStripMenuItem11.Click += new System.EventHandler(this.FirmwaresMenuItem_Click);
			// 
			// toolStripMenuItem12
			// 
			this.toolStripMenuItem12.Text = "&Messages...";
			this.toolStripMenuItem12.Click += new System.EventHandler(this.MessagesMenuItem_Click);
			// 
			// toolStripMenuItem14
			// 
			this.toolStripMenuItem14.Text = "&Rewind...";
			this.toolStripMenuItem14.Click += new System.EventHandler(this.RewindOptionsMenuItem_Click);
			// 
			// toolStripMenuItem15
			// 
			this.toolStripMenuItem15.Text = "File Extensions...";
			this.toolStripMenuItem15.Click += new System.EventHandler(this.FileExtensionsMenuItem_Click);
			// 
			// customizeToolStripMenuItem
			// 
			this.customizeToolStripMenuItem.Text = "Customize...";
			this.customizeToolStripMenuItem.Click += new System.EventHandler(this.CustomizeMenuItem_Click);
			// 
			// toolStripMenuItem66
			// 
			this.toolStripMenuItem66.Text = "Save Config";
			this.toolStripMenuItem66.Click += new System.EventHandler(this.SaveConfigMenuItem_Click);
			// 
			// toolStripMenuItem67
			// 
			this.toolStripMenuItem67.Text = "Load Config";
			this.toolStripMenuItem67.Click += new System.EventHandler(this.LoadConfigMenuItem_Click);
			// 
			// ScreenshotContextMenuItem
			// 
			this.ScreenshotContextMenuItem.Text = "Screenshot";
			this.ScreenshotContextMenuItem.Click += new System.EventHandler(this.ScreenshotMenuItem_Click);
			// 
			// CloseRomContextMenuItem
			// 
			this.CloseRomContextMenuItem.Text = "Close ROM";
			this.CloseRomContextMenuItem.Click += new System.EventHandler(this.CloseRomMenuItem_Click);
			// 
			// ClearSRAMContextMenuItem
			// 
			this.ClearSRAMContextMenuItem.Text = "Close and Clear SRAM";
			this.ClearSRAMContextMenuItem.Click += new System.EventHandler(this.ClearSramContextMenuItem_Click);
			// 
			// ShowMenuContextMenuItem
			// 
			this.ShowMenuContextMenuItem.Text = "Show Menu";
			this.ShowMenuContextMenuItem.Click += new System.EventHandler(this.ShowMenuContextMenuItem_Click);
			// 
			// MainForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(585, 447);
			this.Controls.Add(this.MainStatusBar);
			this.Controls.Add(this.MainformMenu);
			this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.MainformMenu;
			this.MinimumSize = new System.Drawing.Size(475, 125);
			this.Name = "MainForm";
			this.Activated += new System.EventHandler(this.MainForm_Activated);
			this.Deactivate += new System.EventHandler(this.MainForm_Deactivate);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.Shown += new System.EventHandler(this.MainForm_Shown);
			this.Enter += new System.EventHandler(this.MainForm_Enter);
			this.Resize += new System.EventHandler(this.MainForm_Resize);
			this.MainformMenu.ResumeLayout(false);
			this.MainformMenu.PerformLayout();
			this.MainStatusBar.ResumeLayout(false);
			this.MainStatusBar.PerformLayout();
			this.MainFormContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Pixtro.WinForms.Controls.ToolStripMenuItemEx FileSubMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx OpenProjectMenuItem;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx toolStripMenuItem1;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ExitMenuItem;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx toolStripMenuItem2;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx EmulationSubMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ViewSubMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ConfigSubMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ToolsSubMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx HelpSubMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx PauseMenuItem;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx toolStripSeparator1;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx RebootCoreMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx SoftResetMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx OnlineHelpMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx AboutMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ControllersMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx HotkeysMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx RamWatchMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx RamSearchMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx HexEditorMenuItem;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx toolStripSeparator2;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx DisplayFPSMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx DisplayFrameCounterMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx DisplayInputMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx DisplayLagCounterMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx RecentProjectSubMenu;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx toolStripSeparator3;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx toolStripSeparator9;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx SoundMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx SpeedSkipSubMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx VsyncThrottleMenuItem;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx toolStripMenuItem3;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx MinimizeSkippingMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx NeverSkipMenuItem;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx toolStripMenuItem5;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx Speed50MenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx Speed75MenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx Speed100MenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx Speed150MenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx Speed200MenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ClockThrottleMenuItem;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx toolStripSeparator10;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx SaveConfigMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx LoadConfigMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx SwitchToFullscreenMenuItem;
		private StatusStripEx MainStatusBar;
		private Pixtro.WinForms.Controls.StatusLabelEx EmuStatus;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx MessagesMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx PathsMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx DisplayRerecordCountMenuItem;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx toolStripMenuItem4;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx DisplayStatusBarMenuItem;
		private System.Windows.Forms.ContextMenuStrip MainFormContextMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx OpenRomContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx LoadLastRomContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx ContextSeparator_AfterROM;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx RecordMovieContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx PlayMovieContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx LoadLastMovieContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx ContextSeparator_AfterMovie;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx AddSubtitleContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx UndoSavestateContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx ContextSeparator_AfterUndo;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx CloseRomContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx BackupMovieContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx StopMovieContextMenuItem;
		private System.Windows.Forms.ToolStripDropDownButton PauseStatusButton;
		private System.Windows.Forms.ToolStripDropDownButton PlayRecordStatusButton;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ViewSubtitlesContextMenuItem;
		private MenuStripEx MainformMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ViewCommentsContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx DisplayLogWindowMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx DisplaySubtitlesMenuItem;
		private Pixtro.WinForms.Controls.StatusLabelEx AVIStatusLabel;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx RestartMovieContextMenuItem;
		private Pixtro.WinForms.Controls.StatusLabelEx CheatStatusButton;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ShowMenuContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ForumsMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ScreenshotContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx HardResetMenuItem;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx EmulatorMenuSeparator2;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx LoadedCoreNameMenuItem;
		private Pixtro.WinForms.Controls.StatusLabelEx RebootStatusBarIcon;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx SaveMovieContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx AudioThrottleMenuItem;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx toolStripSeparator27;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx VsyncEnabledMenuItem;
		private Pixtro.WinForms.Controls.StatusLabelEx LedLightStatusLabel;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx KeyPrioritySubMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx BothHkAndControllerMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx InputOverHkMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx HkOverInputMenuItem;
		private Pixtro.WinForms.Controls.StatusLabelEx KeyPriorityStatusLabel;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx StopNoSaveContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx toolStripSeparator29;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ConfigContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx RewindOptionsMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx FirmwaresMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ClearSRAMContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx ShowMenuContextMenuSeparator;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx StopAVContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx GenericCoreSubMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx CoresSubMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx BatchRunnerMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx DisplayConfigMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx extensionsToolStripMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx toolStripMenuItem6;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx toolStripMenuItem7;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx toolStripMenuItem8;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx toolStripMenuItem9;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx toolStripMenuItem10;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx toolStripMenuItem11;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx toolStripMenuItem12;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx toolStripMenuItem13;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx toolStripMenuItem14;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx toolStripMenuItem15;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx toolStripSeparator30;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx toolStripMenuItem66;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx toolStripMenuItem67;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ClientOptionsMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx customizeToolStripMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ProfilesMenuItem;
		private Pixtro.WinForms.Controls.StatusLabelEx ProfileFirstBootLabel;
		private Pixtro.WinForms.Controls.StatusLabelEx LinkConnectStatusBarButton;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx FeaturesMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx SaveRAMSubMenu;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx FlushSaveRAMMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx MultiDiskBundlerFileMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx miUnthrottled;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx toolStripMenuItem17;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx Frameskip1MenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx Frameskip2MenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx Frameskip3MenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx Frameskip4MenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx Frameskip5MenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx Frameskip6MenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx Frameskip7MenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx Frameskip9MenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx Frameskip8MenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx Speed400MenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx DisplayMessagesMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx ExternalToolMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx CodeDataLoggerMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx dummyExternalTool;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx SaveConfigAsMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx LoadConfigFromMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx SaveMovieAsContextMenuItem;
		private Pixtro.WinForms.Controls.ToolStripMenuItemEx A7800HawkCoreMenuItem;
		private Pixtro.WinForms.Controls.ToolStripSeparatorEx toolStripSeparator8;
		private System.Windows.Forms.ToolStripMenuItem NewProjectMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ProjectSubMenu;
		private System.Windows.Forms.ToolStripMenuItem BuildProjectMenuItem;
		private System.Windows.Forms.ToolStripMenuItem CloseProjectMenuItem;
		private System.Windows.Forms.ToolStripMenuItem RunProjectMenuItem;
		private System.Windows.Forms.ToolStripMenuItem BuildAndRunMenuItem;
		private System.Windows.Forms.ToolStripMenuItem BuildReleaseMenuItem;
		private System.Windows.Forms.ToolStripMenuItem projectTemplatesWillGoHereToolStripMenuItem;
		private StatusLabelEx UpdateNotification;
		private System.Windows.Forms.ToolStripDropDownButton EditorLayoutSubmenu;
		private System.Windows.Forms.ToolStripMenuItem baseToolStripMenuItem;
	}
}
