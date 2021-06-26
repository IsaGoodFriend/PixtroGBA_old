using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Pixtro.Emuware.DirectX;
using Pixtro.Emuware.OpenTK3;
using Pixtro.Client.Common;
using Pixtro.Client.Editor.CustomControls;
using Pixtro.Client.Editor.ToolExtensions;
using Pixtro.Common;
using Pixtro.Emulation.Common;
using Pixtro.WinForms.Controls;
using Pixtro.Client.Editor.Projects;

namespace Pixtro.Client.Editor
{
	public partial class MainForm
	{
		private void FileSubMenu_DropDownOpened(object sender, EventArgs e)
		{
			OpenProjectMenuItem.ShortcutKeyDisplayString = Config.HotkeyBindings["Open ROM"].Bindings;

			var hasSaveRam = Emulator.HasSaveRam();
			bool needBold = hasSaveRam && Emulator.AsSaveRam().SaveRamModified;

			SaveRAMSubMenu.Enabled = hasSaveRam;
			SaveRAMSubMenu.SetStyle(needBold ? FontStyle.Bold : FontStyle.Regular);

			CloseProjectMenuItem.Enabled = Project != null;

			NewProjectMenuItem.DropDownItems.Clear();
			foreach (var temp in ProjectTemplate.Templates)
			{
				ToolStripMenuItem child;
				NewProjectMenuItem.DropDownItems.Add(child = new ToolStripMenuItem { Enabled = true, Name = temp.Key, Text = temp.Value.meta.Name });

				child.Click += CreateNewTemplate;
			}
		}

		private void CreateNewTemplate(object sender, EventArgs e)
		{
			ToolStripMenuItem obj = sender as ToolStripMenuItem;

			CreateProjectPath(ProjectTemplate.Templates[obj.Name]);
		}

		private void RecentProjectMenuItem_DropDownOpened(object sender, EventArgs e)
		{
			RecentProjectSubMenu.DropDownItems.Clear();
			RecentProjectSubMenu.DropDownItems.AddRange(Config.RecentProjects.RecentMenu(this, LoadProjectFromRecent, "Projects", romLoading: true));
		}

		private bool HasSlot(int slot) => _stateSlots.HasSlot(Emulator, MovieSession.Movie, slot, SaveStatePrefix());

		private void ProjectSubMenu_DropDownOpened(object sender, EventArgs e)
		{

		}

		private void SaveRamSubMenu_DropDownOpened(object sender, EventArgs e)
		{
			FlushSaveRAMMenuItem.ShortcutKeyDisplayString = Config.HotkeyBindings["Flush SaveRAM"].Bindings;
		}

		private void OpenRomMenuItem_Click(object sender, EventArgs e)
		{
			OpenProject();
		}
		private void CloseProjectMenuItem_Click(object sender, EventArgs e)
		{
			CloseProject();
		}

		private void BuildProjectMenuItem_Click(object sender, EventArgs e)
		{
			BuildProject(false);
		}

		private void BuildReleaseMenuItem_Click(object sender, EventArgs e)
		{
			BuildProject(true);
		}

		private void RunProjectMenuItem_Click(object sender, EventArgs e)
		{
			RunProject();
		}

		private void BuildAndRunMenuItem_Click(object sender, EventArgs e)
		{
			BuildAndRun();
		}

		private void OpenAdvancedMenuItem_Click(object sender, EventArgs e)
		{
			using var oac = new OpenAdvancedChooser(this, Config, CreateCoreComm, Game, RunLibretroCoreChooser);
			if (this.ShowDialogWithTempMute(oac) == DialogResult.Cancel) return;

			if (oac.Result == AdvancedRomLoaderType.LibretroLaunchNoGame)
			{
				var argsNoGame = new LoadRomArgs
				{
					OpenAdvanced = new OpenAdvanced_LibretroNoGame(Config.LibretroCore)
				};
				LoadRom("", argsNoGame);
				return;
			}

			var args = new LoadRomArgs();

			var filter = RomLoader.RomFilter;

			if (oac.Result == AdvancedRomLoaderType.LibretroLaunchGame)
			{
				args.OpenAdvanced = new OpenAdvanced_Libretro();
				filter = oac.SuggestedExtensionFilter;
			}
			else if (oac.Result == AdvancedRomLoaderType.ClassicLaunchGame)
			{
				args.OpenAdvanced = new OpenAdvanced_OpenRom();
			}
			else if (oac.Result == AdvancedRomLoaderType.MameLaunchGame)
			{
				args.OpenAdvanced = new OpenAdvanced_MAME();
				filter = new FilesystemFilter("MAME Arcade ROMs", new[] { "zip" }).ToString();
			}
			else
			{
				throw new InvalidOperationException("Automatic Alpha Sanitizer");
			}

			/*************************/
			/* CLONE OF CODE FROM OpenRom (mostly) */
			using var ofd = new OpenFileDialog
			{
				InitialDirectory = Config.PathEntries.RomAbsolutePath(Emulator.SystemId),
				Filter = filter,
				RestoreDirectory = false,
				FilterIndex = _lastOpenRomFilter,
				Title = "Open Advanced"
			};

			if (!this.ShowDialogWithTempMute(ofd).IsOk()) return;

			var file = new FileInfo(ofd.FileName);
			Config.PathEntries.LastRomPath = file.DirectoryName;
			_lastOpenRomFilter = ofd.FilterIndex;
			/*************************/

			LoadRom(file.FullName, args);
		}

		private void CloseRomMenuItem_Click(object sender, EventArgs e)
		{
			Console.WriteLine($"Closing rom clicked Frame: {Emulator.Frame} Emulator: {Emulator.GetType().Name}");
			CloseRom();
			Console.WriteLine($"Closing rom clicked DONE Frame: {Emulator.Frame} Emulator: {Emulator.GetType().Name}");
		}

		private void FlushSaveRAMMenuItem_Click(object sender, EventArgs e)
		{
			FlushSaveRAM();
		}

		private void RecordMovieMenuItem_Click(object sender, EventArgs e)
		{
			if (!Emulator.Attributes().Released)
			{
				var result = this.ModalMessageBox2(
					"Thanks for using BizHawk!  The emulation core you have selected "
						+ "is currently BETA-status.  We appreciate your help in testing BizHawk. "
						+ "You can record a movie on this core if you'd like to, but expect to "
						+ "encounter bugs and sync problems.  Continue?",
					"BizHawk");

				if (!result)
				{
					return;
				}
			}

			// Nag user to user a more accurate core, but let them continue anyway
			EnsureCoreIsAccurate();

		}

		private void PlayMovieMenuItem_Click(object sender, EventArgs e)
		{
		}

		private void StopMovieMenuItem_Click(object sender, EventArgs e)
		{
			StopMovie();
		}

		private void PlayFromBeginningMenuItem_Click(object sender, EventArgs e)
		{
			RestartMovie();
		}

		private void ImportMovieMenuItem_Click(object sender, EventArgs e)
		{
			using var ofd = new OpenFileDialog
			{
				InitialDirectory = Config.PathEntries.RomAbsolutePath(Emulator.SystemId),
				Multiselect = true,
				RestoreDirectory = false
			};

			if (this.ShowDialogWithTempMute(ofd).IsOk())
			{
				foreach (var fn in ofd.FileNames)
				{
					ProcessMovieImport(fn, false);
				}
			}
		}

		private void SaveMovieMenuItem_Click(object sender, EventArgs e)
		{
			SaveMovie();
		}

		private void SaveMovieAsMenuItem_Click(object sender, EventArgs e)
		{
			var filename = MovieSession.Movie.Filename;
			if (string.IsNullOrWhiteSpace(filename))
			{
				filename = Game.FilesystemSafeName();
			}

			var file = ToolFormBase.SaveFileDialog(
				filename,
				Config.PathEntries.MovieAbsolutePath(),
				"Movie Files",
				MovieSession.Movie.PreferredExtension,
				this);

			if (file != null)
			{
				MovieSession.Movie.Filename = file.FullName;
				Config.RecentMovies.Add(MovieSession.Movie.Filename);
				SaveMovie();
			}
		}

		private void StopMovieWithoutSavingMenuItem_Click(object sender, EventArgs e)
		{
			if (Config.Movies.EnableBackupMovies)
			{
				MovieSession.Movie.SaveBackup();
			}

			StopMovie(saveChanges: false);
		}

		private void AutomaticMovieBackupMenuItem_Click(object sender, EventArgs e)
		{
			Config.Movies.EnableBackupMovies ^= true;
		}

		private void FullMovieLoadstatesMenuItem_Click(object sender, EventArgs e)
		{
			Config.Movies.VBAStyleMovieLoadState ^= true;
		}

		private void MovieEndFinishMenuItem_Click(object sender, EventArgs e)
		{
			Config.Movies.MovieEndAction = MovieEndAction.Finish;
		}

		private void MovieEndRecordMenuItem_Click(object sender, EventArgs e)
		{
			Config.Movies.MovieEndAction = MovieEndAction.Record;
		}

		private void MovieEndStopMenuItem_Click(object sender, EventArgs e)
		{
			Config.Movies.MovieEndAction = MovieEndAction.Stop;
		}

		private void MovieEndPauseMenuItem_Click(object sender, EventArgs e)
		{
			Config.Movies.MovieEndAction = MovieEndAction.Pause;
		}

		private void ConfigAndRecordAVMenuItem_Click(object sender, EventArgs e)
		{
			if (OSTailoredCode.IsUnixHost) new MsgBox("Most of these options will cause crashes on Linux.", "A/V instability warning", MessageBoxIcon.Warning).ShowDialog();
			RecordAv();
		}

		private void RecordAVMenuItem_Click(object sender, EventArgs e)
		{
			RecordAv(null, null); // force unattended, but allow traditional setup
		}

		private void StopAVMenuItem_Click(object sender, EventArgs e)
		{
			StopAv();
		}

		private void ScreenshotMenuItem_Click(object sender, EventArgs e)
		{
			TakeScreenshot();
		}

		private void ScreenshotAsMenuItem_Click(object sender, EventArgs e)
		{
			var path = $"{ScreenshotPrefix()}.{DateTime.Now:yyyy-MM-dd HH.mm.ss}.png";

			using var sfd = new SaveFileDialog
			{
				InitialDirectory = Path.GetDirectoryName(path),
				FileName = Path.GetFileName(path),
				Filter = FilesystemFilter.PNGs.ToString()
			};

			if (this.ShowDialogWithTempMute(sfd).IsOk())
			{
				TakeScreenshot(sfd.FileName);
			}
		}

		private void ScreenshotClipboardMenuItem_Click(object sender, EventArgs e)
		{
			TakeScreenshotToClipboard();
		}

		private void ScreenshotClientClipboardMenuItem_Click(object sender, EventArgs e)
		{
			TakeScreenshotClientToClipboard();
		}

		private void ScreenshotCaptureOSDMenuItem_Click(object sender, EventArgs e)
		{
			Config.ScreenshotCaptureOsd ^= true;
		}

		private void ExitMenuItem_Click(object sender, EventArgs e)
		{
			if (Tools.AskSave())
			{
				Close();
			}
		}

		public void CloseEmulator(int? exitCode = null)
		{
			_exitRequestPending = true;
			if (exitCode != null) _exitCode = exitCode.Value;
		}

		private void EmulationMenuItem_DropDownOpened(object sender, EventArgs e)
		{
			PauseMenuItem.Checked = _didMenuPause ? _wasPaused : EmulatorPaused;

			SoftResetMenuItem.Enabled = Emulator.ControllerDefinition.BoolButtons.Contains("Reset")
				&& !MovieSession.Movie.IsPlaying();

			HardResetMenuItem.Enabled = Emulator.ControllerDefinition.BoolButtons.Contains("Power")
				&& !MovieSession.Movie.IsPlaying();

			PauseMenuItem.ShortcutKeyDisplayString = Config.HotkeyBindings["Pause"].Bindings;
			RebootCoreMenuItem.ShortcutKeyDisplayString = Config.HotkeyBindings["Reboot Core"].Bindings;
			SoftResetMenuItem.ShortcutKeyDisplayString = Config.HotkeyBindings["Soft Reset"].Bindings;
			HardResetMenuItem.ShortcutKeyDisplayString = Config.HotkeyBindings["Hard Reset"].Bindings;
		}

		private void PauseMenuItem_Click(object sender, EventArgs e)
		{
			if (IsTurboSeeking || IsSeeking)
			{
				PauseOnFrame = null;
			}
			else if (EmulatorPaused)
			{
				UnpauseEmulator();
			}
			else
			{
				PauseEmulator();
			}
		}

		private void PowerMenuItem_Click(object sender, EventArgs e)
		{
			RebootCore();
		}

		private void SoftResetMenuItem_Click(object sender, EventArgs e)
		{
			SoftReset();
		}

		private void HardResetMenuItem_Click(object sender, EventArgs e)
		{
			HardReset();
		}

		private void ViewSubMenu_DropDownOpened(object sender, EventArgs e)
		{
			DisplayFPSMenuItem.Checked = Config.DisplayFps;
			DisplayFrameCounterMenuItem.Checked = Config.DisplayFrameCounter;
			DisplayLagCounterMenuItem.Checked = Config.DisplayLagCounter;
			DisplayInputMenuItem.Checked = Config.DisplayInput;
			DisplayRerecordCountMenuItem.Checked = Config.DisplayRerecordCount;
			DisplaySubtitlesMenuItem.Checked = Config.DisplaySubtitles;

			DisplayFPSMenuItem.ShortcutKeyDisplayString = Config.HotkeyBindings["Display FPS"].Bindings;
			DisplayFrameCounterMenuItem.ShortcutKeyDisplayString = Config.HotkeyBindings["Frame Counter"].Bindings;
			DisplayLagCounterMenuItem.ShortcutKeyDisplayString = Config.HotkeyBindings["Lag Counter"].Bindings;
			DisplayInputMenuItem.ShortcutKeyDisplayString = Config.HotkeyBindings["Input Display"].Bindings;
			SwitchToFullscreenMenuItem.ShortcutKeyDisplayString = Config.HotkeyBindings["Full Screen"].Bindings;

			DisplayStatusBarMenuItem.Checked = Config.DispChromeStatusBarWindowed;
			DisplayLogWindowMenuItem.Checked = Tools.IsLoaded<LogWindow>();

			DisplayLagCounterMenuItem.Enabled = Emulator.CanPollInput();

			DisplayMessagesMenuItem.Checked = Config.DisplayMessages;
		}

		private void SwitchToFullscreenMenuItem_Click(object sender, EventArgs e)
		{
			ToggleFullscreen();
		}

		private void DisplayFpsMenuItem_Click(object sender, EventArgs e)
		{
			ToggleFps();
		}

		private void DisplayFrameCounterMenuItem_Click(object sender, EventArgs e)
		{
			ToggleFrameCounter();
		}

		private void DisplayLagCounterMenuItem_Click(object sender, EventArgs e)
		{
			ToggleLagCounter();
		}

		private void DisplayInputMenuItem_Click(object sender, EventArgs e)
		{
			ToggleInputDisplay();
		}

		private void DisplayRerecordsMenuItem_Click(object sender, EventArgs e)
		{
			Config.DisplayRerecordCount ^= true;
		}

		private void DisplaySubtitlesMenuItem_Click(object sender, EventArgs e)
		{
			Config.DisplaySubtitles ^= true;
		}

		private void DisplayStatusBarMenuItem_Click(object sender, EventArgs e)
		{
			Config.DispChromeStatusBarWindowed ^= true;
			SetStatusBar();
		}

		private void DisplayMessagesMenuItem_Click(object sender, EventArgs e)
		{
			Config.DisplayMessages ^= true;
		}

		private void DisplayLogWindowMenuItem_Click(object sender, EventArgs e)
		{
			Tools.Load<LogWindow>();
		}

		private void ConfigSubMenu_DropDownOpened(object sender, EventArgs e)
		{
			ControllersMenuItem.Enabled = Emulator.ControllerDefinition.Any();
			RewindOptionsMenuItem.Enabled = Emulator.HasSavestates();
		}

		private void FrameSkipMenuItem_DropDownOpened(object sender, EventArgs e)
		{
			MinimizeSkippingMenuItem.Checked = Config.AutoMinimizeSkipping;
			ClockThrottleMenuItem.Checked = Config.ClockThrottle;
			VsyncThrottleMenuItem.Checked = Config.VSyncThrottle;
			NeverSkipMenuItem.Checked = Config.FrameSkip == 0;
			Frameskip1MenuItem.Checked = Config.FrameSkip == 1;
			Frameskip2MenuItem.Checked = Config.FrameSkip == 2;
			Frameskip3MenuItem.Checked = Config.FrameSkip == 3;
			Frameskip4MenuItem.Checked = Config.FrameSkip == 4;
			Frameskip5MenuItem.Checked = Config.FrameSkip == 5;
			Frameskip6MenuItem.Checked = Config.FrameSkip == 6;
			Frameskip7MenuItem.Checked = Config.FrameSkip == 7;
			Frameskip8MenuItem.Checked = Config.FrameSkip == 8;
			Frameskip9MenuItem.Checked = Config.FrameSkip == 9;
			MinimizeSkippingMenuItem.Enabled = !NeverSkipMenuItem.Checked;
			if (!MinimizeSkippingMenuItem.Enabled)
			{
				MinimizeSkippingMenuItem.Checked = true;
			}

			AudioThrottleMenuItem.Enabled = Config.SoundEnabled;
			AudioThrottleMenuItem.Checked = Config.SoundThrottle;
			VsyncEnabledMenuItem.Checked = Config.VSync;

			Speed100MenuItem.Checked = Config.SpeedPercent == 100;
			Speed100MenuItem.Image = (Config.SpeedPercentAlternate == 100) ? Properties.Resources.FastForward : null;
			Speed150MenuItem.Checked = Config.SpeedPercent == 150;
			Speed150MenuItem.Image = (Config.SpeedPercentAlternate == 150) ? Properties.Resources.FastForward : null;
			Speed400MenuItem.Checked = Config.SpeedPercent == 400;
			Speed400MenuItem.Image = (Config.SpeedPercentAlternate == 400) ? Properties.Resources.FastForward : null;
			Speed200MenuItem.Checked = Config.SpeedPercent == 200;
			Speed200MenuItem.Image = (Config.SpeedPercentAlternate == 200) ? Properties.Resources.FastForward : null;
			Speed75MenuItem.Checked = Config.SpeedPercent == 75;
			Speed75MenuItem.Image = (Config.SpeedPercentAlternate == 75) ? Properties.Resources.FastForward : null;
			Speed50MenuItem.Checked = Config.SpeedPercent == 50;
			Speed50MenuItem.Image = (Config.SpeedPercentAlternate == 50) ? Properties.Resources.FastForward : null;

			Speed50MenuItem.Enabled =
				Speed75MenuItem.Enabled =
				Speed100MenuItem.Enabled =
				Speed150MenuItem.Enabled =
				Speed200MenuItem.Enabled =
				Speed400MenuItem.Enabled =
				Config.ClockThrottle;

			miUnthrottled.Checked = _unthrottled;
		}

		private void KeyPriorityMenuItem_DropDownOpened(object sender, EventArgs e)
		{
			BothHkAndControllerMenuItem.Checked = false;
			InputOverHkMenuItem.Checked = false;
			HkOverInputMenuItem.Checked = false;

			switch (Config.InputHotkeyOverrideOptions)
			{
				default:
				case 0:
					BothHkAndControllerMenuItem.Checked = true;
					break;
				case 1:
					InputOverHkMenuItem.Checked = true;
					break;
				case 2:
					HkOverInputMenuItem.Checked = true;
					break;
			}
		}

		private void ControllersMenuItem_Click(object sender, EventArgs e)
		{
			using var controller = new ControllerConfig(this, Emulator, Config);
			if (controller.ShowDialog().IsOk())
			{
				AddOnScreenMessage("Controller settings saved");
				InitControls();
				InputManager.SyncControls(Emulator, MovieSession, Config);
			}
			else
			{
				AddOnScreenMessage("Controller config aborted");
			}
		}

		private void HotkeysMenuItem_Click(object sender, EventArgs e)
		{
			using var hotkeyConfig = new HotkeyConfig(Config);
			if (hotkeyConfig.ShowDialog().IsOk())
			{
				AddOnScreenMessage("Hotkey settings saved");
				InitControls();
				InputManager.SyncControls(Emulator, MovieSession, Config);
			}
			else
			{
				AddOnScreenMessage("Hotkey config aborted");
			}
		}

		private void FirmwaresMenuItem_Click(object sender, EventArgs e)
		{
			if (e is RomLoader.RomErrorArgs args)
			{
				using var configForm = new FirmwaresConfig(FirmwareManager, Config.FirmwareUserSpecifications, this, Config.PathEntries, retryLoadRom: true, reloadRomPath: args.RomPath);
				var result = configForm.ShowDialog();
				args.Retry = result == DialogResult.Retry;
			}
			else
			{
				using var configForm = new FirmwaresConfig(FirmwareManager, Config.FirmwareUserSpecifications, this, Config.PathEntries);
				configForm.ShowDialog();
			}
		}

		private void MessagesMenuItem_Click(object sender, EventArgs e)
		{
			using var form = new MessageConfig(Config);
			var result = form.ShowDialog();
			AddOnScreenMessage(result.IsOk()
				? "Message settings saved"
				: "Message config aborted");
		}

		private void PathsMenuItem_Click(object sender, EventArgs e)
		{
			using var form = new PathConfig(this, Config.PathEntries, Game.System);
			form.ShowDialog();
		}

		private void SoundMenuItem_Click(object sender, EventArgs e)
		{
			static IEnumerable<string> GetDeviceNamesCallback(ESoundOutputMethod outputMethod) => outputMethod switch
			{
				ESoundOutputMethod.DirectSound => DirectSoundSoundOutput.GetDeviceNames(),
				ESoundOutputMethod.XAudio2 => XAudio2SoundOutput.GetDeviceNames(),
				ESoundOutputMethod.OpenAL => OpenALSoundOutput.GetDeviceNames(),
				_ => Enumerable.Empty<string>()
			};
			using var form = new SoundConfig(this, Config, GetDeviceNamesCallback);
			if (!form.ShowDialog().IsOk())
			{
				AddOnScreenMessage("Sound config aborted");
				return;
			}

			AddOnScreenMessage("Sound settings saved");
			if (form.ApplyNewSoundDevice)
			{
				Sound.Dispose();
				Sound = new Sound(Handle, Config, () => Emulator.VsyncRate());
				Sound.StartSound();
			}
			else
			{
				Sound.StopSound();
				Sound.StartSound();
			}
			RewireSound();
		}

		private void AutofireMenuItem_Click(object sender, EventArgs e)
		{
			using var form = new AutofireConfig(Config, InputManager.AutoFireController, InputManager.AutofireStickyXorAdapter);
			var result = form.ShowDialog();
			AddOnScreenMessage(result.IsOk()
				? "Autofire settings saved"
				: "Autofire config aborted");
		}

		private void RewindOptionsMenuItem_Click(object sender, EventArgs e)
		{
			if (Emulator.HasSavestates())
			{
				using var form = new RewindConfig(Config, CreateRewinder, () => this.Rewinder, Emulator.AsStatable());
				AddOnScreenMessage(form.ShowDialog().IsOk()
					? "Rewind and State settings saved"
					: "Rewind config aborted");
			}
		}

		private void FileExtensionsMenuItem_Click(object sender, EventArgs e)
		{
			using var form = new FileExtensionPreferences(Config.PreferredPlatformsForExtensions);
			var result = form.ShowDialog();
			AddOnScreenMessage(result.IsOk()
				? "Rom Extension Preferences changed"
				: "Rom Extension Preferences cancelled");
		}

		private void BumpAutoFlushSaveRamTimer()
		{
			if (AutoFlushSaveRamIn > Config.FlushSaveRamFrames)
			{
				AutoFlushSaveRamIn = Config.FlushSaveRamFrames;
			}
		}

		private void CustomizeMenuItem_Click(object sender, EventArgs e)
		{
			using var form = new EmuHawkOptions(BumpAutoFlushSaveRamTimer, Config, this.AddOnScreenMessage);
			form.ShowDialog();
		}

		private void ProfilesMenuItem_Click(object sender, EventArgs e)
		{
			using var form = new ProfileConfig(this, Emulator, Config);
			if (form.ShowDialog().IsOk())
			{
				AddOnScreenMessage("Profile settings saved");

				// We hide the FirstBoot items since the user setup a Profile
				// Is it a bad thing to do this constantly?
				Config.FirstBoot = false;
				ProfileFirstBootLabel.Visible = false;
			}
			else
			{
				AddOnScreenMessage("Profile config aborted");
			}
		}

		private void ClockThrottleMenuItem_Click(object sender, EventArgs e)
		{
			Config.ClockThrottle ^= true;
			if (Config.ClockThrottle)
			{
				var old = Config.SoundThrottle;
				Config.SoundThrottle = false;
				if (old)
				{
					RewireSound();
				}

				old = Config.VSyncThrottle;
				Config.VSyncThrottle = false;
				if (old)
				{
					_presentationPanel.Resized = true;
				}
			}

			ThrottleMessage();
		}

		private void AudioThrottleMenuItem_Click(object sender, EventArgs e)
		{
			Config.SoundThrottle ^= true;
			RewireSound();
			if (Config.SoundThrottle)
			{
				Config.ClockThrottle = false;
				var old = Config.VSyncThrottle;
				Config.VSyncThrottle = false;
				if (old)
				{
					_presentationPanel.Resized = true;
				}
			}

			ThrottleMessage();
		}

		private void VsyncThrottleMenuItem_Click(object sender, EventArgs e)
		{
			Config.VSyncThrottle ^= true;
			_presentationPanel.Resized = true;
			if (Config.VSyncThrottle)
			{
				Config.ClockThrottle = false;
				var old = Config.SoundThrottle;
				Config.SoundThrottle = false;
				if (old)
				{
					RewireSound();
				}
			}

			if (!Config.VSync)
			{
				Config.VSync = true;
				VsyncMessage();
			}

			ThrottleMessage();
		}

		private void VsyncEnabledMenuItem_Click(object sender, EventArgs e)
		{
			Config.VSync ^= true;
			if (!Config.VSyncThrottle) // when vsync throttle is on, vsync is forced to on, so no change to make here
			{
				_presentationPanel.Resized = true;
			}

			VsyncMessage();
		}

		private void UnthrottledMenuItem_Click(object sender, EventArgs e)
		{
			_unthrottled ^= true;
			ThrottleMessage();
		}

		private void MinimizeSkippingMenuItem_Click(object sender, EventArgs e)
		{
			Config.AutoMinimizeSkipping ^= true;
		}

		private void NeverSkipMenuItem_Click(object sender, EventArgs e) { Config.FrameSkip = 0; FrameSkipMessage(); }
		private void Frameskip1MenuItem_Click(object sender, EventArgs e) { Config.FrameSkip = 1; FrameSkipMessage(); }
		private void Frameskip2MenuItem_Click(object sender, EventArgs e) { Config.FrameSkip = 2; FrameSkipMessage(); }
		private void Frameskip3MenuItem_Click(object sender, EventArgs e) { Config.FrameSkip = 3; FrameSkipMessage(); }
		private void Frameskip4MenuItem_Click(object sender, EventArgs e) { Config.FrameSkip = 4; FrameSkipMessage(); }
		private void Frameskip5MenuItem_Click(object sender, EventArgs e) { Config.FrameSkip = 5; FrameSkipMessage(); }
		private void Frameskip6MenuItem_Click(object sender, EventArgs e) { Config.FrameSkip = 6; FrameSkipMessage(); }
		private void Frameskip7MenuItem_Click(object sender, EventArgs e) { Config.FrameSkip = 7; FrameSkipMessage(); }
		private void Frameskip8MenuItem_Click(object sender, EventArgs e) { Config.FrameSkip = 8; FrameSkipMessage(); }
		private void Frameskip9MenuItem_Click(object sender, EventArgs e) { Config.FrameSkip = 9; FrameSkipMessage(); }

		private void Speed50MenuItem_Click(object sender, EventArgs e) => ClickSpeedItem(50);
		private void Speed75MenuItem_Click(object sender, EventArgs e) => ClickSpeedItem(75);
		private void Speed100MenuItem_Click(object sender, EventArgs e) => ClickSpeedItem(100);
		private void Speed150MenuItem_Click(object sender, EventArgs e) => ClickSpeedItem(150);
		private void Speed200MenuItem_Click(object sender, EventArgs e) => ClickSpeedItem(200);
		private void Speed400MenuItem_Click(object sender, EventArgs e) => ClickSpeedItem(400);

		private void BothHkAndControllerMenuItem_Click(object sender, EventArgs e)
		{
			Config.InputHotkeyOverrideOptions = 0;
			UpdateKeyPriorityIcon();
		}

		private void InputOverHkMenuItem_Click(object sender, EventArgs e)
		{
			Config.InputHotkeyOverrideOptions = 1;
			UpdateKeyPriorityIcon();
		}

		private void HkOverInputMenuItem_Click(object sender, EventArgs e)
		{
			Config.InputHotkeyOverrideOptions = 2;
			UpdateKeyPriorityIcon();
		}

		private void SaveConfigMenuItem_Click(object sender, EventArgs e)
		{
			SaveConfig();
			AddOnScreenMessage("Saved settings");
		}

		private void SaveConfigAsMenuItem_Click(object sender, EventArgs e)
		{
			var path = Config.DefaultIniPath;
			using var sfd = new SaveFileDialog
			{
				InitialDirectory = Path.GetDirectoryName(path),
				FileName = Path.GetFileName(path),
				Filter = ConfigFileFSFilterString
			};

			if (this.ShowDialogWithTempMute(sfd).IsOk())
			{
				SaveConfig(sfd.FileName);
				AddOnScreenMessage("Copied settings");
			}
		}

		private void LoadConfigMenuItem_Click(object sender, EventArgs e)
		{
			LoadConfigFile(Config.DefaultIniPath);
		}

		private void LoadConfigFromMenuItem_Click(object sender, EventArgs e)
		{
			var path = Config.DefaultIniPath;
			using var ofd = new OpenFileDialog
			{
				InitialDirectory = Path.GetDirectoryName(path),
				FileName = Path.GetFileName(path),
				Filter = ConfigFileFSFilterString
			};

			if (this.ShowDialogWithTempMute(ofd).IsOk())
			{
				LoadConfigFile(ofd.FileName);
			}
		}

		private void ToolsSubMenu_DropDownOpened(object sender, EventArgs e)
		{
			RamWatchMenuItem.ShortcutKeyDisplayString = Config.HotkeyBindings["RAM Watch"].Bindings;
			RamSearchMenuItem.ShortcutKeyDisplayString = Config.HotkeyBindings["RAM Search"].Bindings;
			HexEditorMenuItem.ShortcutKeyDisplayString = Config.HotkeyBindings["Hex Editor"].Bindings;
			CodeDataLoggerMenuItem.Enabled = Tools.IsAvailable<CDL>();

			HexEditorMenuItem.Enabled = Tools.IsAvailable<HexEditor>();
			RamSearchMenuItem.Enabled = Tools.IsAvailable<RamSearch>();
			RamWatchMenuItem.Enabled = Tools.IsAvailable<RamWatch>();

			BatchRunnerMenuItem.Visible = VersionInfo.DeveloperBuild;
		}

		private void ExternalToolMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			ExternalToolMenuItem.DropDownItems.Clear();

			foreach (var item in ExtToolManager.ToolStripMenu)
			{
				if (item.Tag is ValueTuple<string, string> tuple)
				{
					if (item.Enabled)
					{
						item.Click += (clickEventSender, clickEventArgs) => Tools.LoadExternalToolForm(tuple.Item1, tuple.Item2);
					}
				}
				else
				{
					item.Image = Properties.Resources.ExclamationRed;
				}

				ExternalToolMenuItem.DropDownItems.Add(item);
			}

			if (ExternalToolMenuItem.DropDownItems.Count == 0)
			{
				ExternalToolMenuItem.DropDownItems.Add("None");
			}
		}

		private void ToolBoxMenuItem_Click(object sender, EventArgs e)
		{
			Tools.Load<ToolBox>();
		}

		private void RamWatchMenuItem_Click(object sender, EventArgs e)
		{
			Tools.LoadRamWatch(true);
		}

		private void RamSearchMenuItem_Click(object sender, EventArgs e) => Tools.Load<RamSearch>();

		private void LuaConsoleMenuItem_Click(object sender, EventArgs e)
		{
			OpenLuaConsole();
		}

		private void TAStudioMenuItem_Click(object sender, EventArgs e)
		{
			if (!Emulator.CanPollInput())
			{
				ShowMessageBox(owner: null, "Current core does not support input polling. TAStudio can't be used.");
				return;
			}

		}

		private void HexEditorMenuItem_Click(object sender, EventArgs e)
		{
			var editor = Tools.Load<HexEditor>();
		}

		private void TraceLoggerMenuItem_Click(object sender, EventArgs e)
		{
			Tools.Load<TraceLogger>();
		}

		private void DebuggerMenuItem_Click(object sender, EventArgs e)
		{
			Tools.Load<GenericDebugger>();
		}

		private void CodeDataLoggerMenuItem_Click(object sender, EventArgs e)
		{
			Tools.Load<CDL>();
		}

		private void MacroToolMenuItem_Click(object sender, EventArgs e)
		{
		}

		private void VirtualPadMenuItem_Click(object sender, EventArgs e)
		{
			Tools.Load<VirtualpadTool>();
		}

		private void BasicBotMenuItem_Click(object sender, EventArgs e)
		{
		}

		private void CheatsMenuItem_Click(object sender, EventArgs e)
		{
			Tools.Load<Cheats>();
		}

		private void CheatCodeConverterMenuItem_Click(object sender, EventArgs e)
		{
			Tools.Load<GameShark>();
		}

		private void MultidiskBundlerMenuItem_Click(object sender, EventArgs e)
		{
			Tools.Load<MultiDiskBundler>();
		}

		private void BatchRunnerMenuItem_Click(object sender, EventArgs e)
		{
			using var form = new BatchRun(this, Config, CreateCoreComm);
			form.ShowDialog();
		}

		private void GenericCoreSettingsMenuItem_Click(object sender, EventArgs e)
		{
			var coreName = ((CoreAttribute) Attribute.GetCustomAttribute(Emulator.GetType(), typeof(CoreAttribute))).CoreName;
			GenericCoreConfig.DoDialog(this, $"{coreName} Settings");
		}

		private void HelpSubMenu_DropDownOpened(object sender, EventArgs e)
		{
			FeaturesMenuItem.Visible = VersionInfo.DeveloperBuild;
		}

		private void OnlineHelpMenuItem_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("http://tasvideos.org/BizHawk.html");
		}

		private void ForumsMenuItem_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("http://tasvideos.org/forum/viewforum.php?f=64");
		}

		private void FeaturesMenuItem_Click(object sender, EventArgs e)
		{
			Tools.Load<CoreFeatureAnalysis>();
		}

		private void AboutMenuItem_Click(object sender, EventArgs e)
		{
			using var form = new BizBox();
			form.ShowDialog();
		}

		private void MainFormContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_wasPaused = EmulatorPaused;
			_didMenuPause = true;
			PauseEmulator();

			OpenRomContextMenuItem.Visible = Emulator.IsNull() || _inFullscreen;

			bool showMenuVisible = _inFullscreen || !MainMenuStrip.Visible; // need to always be able to restore this as an emergency measure

			if (_argParser._chromeless)
			{
				showMenuVisible = true; // I decided this was always possible in chrome-less mode, we'll see what they think
			}

			var movieIsActive = MovieSession.Movie.IsActive();

			ShowMenuContextMenuItem.Visible =
				ShowMenuContextMenuSeparator.Visible =
				showMenuVisible;

			LoadLastRomContextMenuItem.Visible = Emulator.IsNull();

			StopAVContextMenuItem.Visible = _currAviWriter != null;

			ContextSeparator_AfterMovie.Visible =
				ContextSeparator_AfterUndo.Visible =
				ScreenshotContextMenuItem.Visible =
				CloseRomContextMenuItem.Visible =
				UndoSavestateContextMenuItem.Visible =
				!Emulator.IsNull();

			RecordMovieContextMenuItem.Visible =
				PlayMovieContextMenuItem.Visible =
				LoadLastMovieContextMenuItem.Visible =
				!Emulator.IsNull() && !movieIsActive;

			RestartMovieContextMenuItem.Visible =
				StopMovieContextMenuItem.Visible =
				ViewSubtitlesContextMenuItem.Visible =
				ViewCommentsContextMenuItem.Visible =
				SaveMovieContextMenuItem.Visible =
				SaveMovieAsContextMenuItem.Visible =
					movieIsActive;

			BackupMovieContextMenuItem.Visible = movieIsActive;

			StopNoSaveContextMenuItem.Visible = movieIsActive && MovieSession.Movie.Changes;

			AddSubtitleContextMenuItem.Visible = !Emulator.IsNull() && movieIsActive && !MovieSession.ReadOnly;

			ConfigContextMenuItem.Visible = _inFullscreen;

			ClearSRAMContextMenuItem.Visible = File.Exists(Config.PathEntries.SaveRamAbsolutePath(Game, MovieSession.Movie));

			ContextSeparator_AfterROM.Visible = OpenRomContextMenuItem.Visible || LoadLastRomContextMenuItem.Visible;

			LoadLastRomContextMenuItem.Enabled = !Config.RecentProjects.Empty;
			LoadLastMovieContextMenuItem.Enabled = !Config.RecentMovies.Empty;

			if (movieIsActive)
			{
				if (MovieSession.ReadOnly)
				{
					ViewSubtitlesContextMenuItem.Text = "View Subtitles";
					ViewCommentsContextMenuItem.Text = "View Comments";
				}
				else
				{
					ViewSubtitlesContextMenuItem.Text = "Edit Subtitles";
					ViewCommentsContextMenuItem.Text = "Edit Comments";
				}
			}

			var file = new FileInfo($"{SaveStatePrefix()}.QuickSave{Config.SaveSlot}.State.bak");

			if (file.Exists)
			{
				//UndoSavestateContextMenuItem.Enabled = true;
				//if (_stateSlots.IsRedo(MovieSession.Movie, Config.SaveSlot))
				//{
				//	UndoSavestateContextMenuItem.Text = $"Redo Save to slot {Config.SaveSlot}";
				//	UndoSavestateContextMenuItem.Image = Properties.Resources.Redo;
				//}
				//else
				//{
				//	UndoSavestateContextMenuItem.Text = $"Undo Save to slot {Config.SaveSlot}";
				//	UndoSavestateContextMenuItem.Image = Properties.Resources.Undo;
				//}
			}
			else
			{
				UndoSavestateContextMenuItem.Enabled = false;
				UndoSavestateContextMenuItem.Text = "Undo Savestate";
				UndoSavestateContextMenuItem.Image = Properties.Resources.Undo;
			}

			ShowMenuContextMenuItem.Text = MainMenuStrip.Visible ? "Hide Menu" : "Show Menu";
		}

		private void MainFormContextMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
		{
			if (!_wasPaused)
			{
				UnpauseEmulator();
			}
		}

		private void DisplayConfigMenuItem_Click(object sender, EventArgs e)
		{
			using var window = new DisplayConfig(Config, GL);
			if (window.ShowDialog().IsOk())
			{
				DisplayManager.RefreshUserShader();
				FrameBufferResized();
				SynchChrome();
				if (window.NeedReset)
				{
					AddOnScreenMessage("Restart program for changed settings");
				}
			}
		}

		private void LoadLastRomContextMenuItem_Click(object sender, EventArgs e)
		{
			//LoadRomFromRecent(Config.RecentRoms.MostRecent);
		}

		private void LoadLastMovieContextMenuItem_Click(object sender, EventArgs e)
		{
			LoadMoviesFromRecent(Config.RecentMovies.MostRecent);
		}

		private void BackupMovieContextMenuItem_Click(object sender, EventArgs e)
		{
			MovieSession.Movie.SaveBackup();
			AddOnScreenMessage("Backup movie saved.");
		}

		private void ViewSubtitlesContextMenuItem_Click(object sender, EventArgs e)
		{
		}

		private void AddSubtitleContextMenuItem_Click(object sender, EventArgs e)
		{
		}

		private void ViewCommentsContextMenuItem_Click(object sender, EventArgs e)
		{
		}

		private void UndoSavestateContextMenuItem_Click(object sender, EventArgs e)
		{
		}

		private void ClearSramContextMenuItem_Click(object sender, EventArgs e)
		{
			CloseRom(clearSram: true);
		}

		private void ShowMenuContextMenuItem_Click(object sender, EventArgs e)
		{
			MainMenuStrip.Visible ^= true;
			FrameBufferResized();
		}

		private void DumpStatusButton_Click(object sender, EventArgs e)
		{
			string details = Emulator.RomDetails();
			if (string.IsNullOrWhiteSpace(details))
			{
				details = _defaultRomDetails;
			}

			if (!string.IsNullOrEmpty(details))
			{
				Tools.Load<LogWindow>();
				((LogWindow) Tools.Get<LogWindow>()).ShowReport("Dump Status Report", details);
			}
		}

		private void KeyPriorityStatusLabel_Click(object sender, EventArgs e)
		{
			Config.InputHotkeyOverrideOptions = Config.InputHotkeyOverrideOptions switch
			{
				1 => 2,
				2 => Config.NoMixedInputHokeyOverride ? 1 : 0,
				_ => 1,
			};
			UpdateKeyPriorityIcon();
		}

		private void FreezeStatus_Click(object sender, EventArgs e)
		{
			if (CheatStatusButton.Visible)
			{
				Tools.Load<Cheats>();
			}
		}

		private void ProfileFirstBootLabel_Click(object sender, EventArgs e)
		{
			// We do not check if the user is actually setting a profile here.
			// This is intentional.
			using var profileForm = new ProfileConfig(this, Emulator, Config);
			profileForm.ShowDialog();
			Config.FirstBoot = false;
			ProfileFirstBootLabel.Visible = false;
		}

		private void LinkConnectStatusBarButton_Click(object sender, EventArgs e)
		{
			// toggle Link status (only outside of a movie session)
			if (!MovieSession.Movie.IsPlaying())
			{
				Emulator.AsLinkable().LinkConnected ^= true;
				Console.WriteLine("Cable connect status to {0}", Emulator.AsLinkable().LinkConnected);
			}
		}

		private void UpdateNotification_Click(object sender, EventArgs e)
		{
			Sound.StopSound();
			var result = this.ModalMessageBox3(
				$"Version {Config.UpdateLatestVersion} is now available. Would you like to open the BizHawk homepage?\r\n\r\nClick \"No\" to hide the update notification for this version.",
				"New Version Available",
				EMsgBoxIcon.Question);
			Sound.StartSound();

			if (result == true)
			{
				System.Threading.ThreadPool.QueueUserWorkItem(s =>
				{
					using (System.Diagnostics.Process.Start(VersionInfo.HomePage))
					{
					}
				});
			}
			else if (result == false)
			{
				UpdateChecker.GlobalConfig = Config;
				UpdateChecker.IgnoreNewVersion();
				UpdateChecker.BeginCheck(skipCheck: true); // Trigger event to hide new version notification
			}
		}

		private void MainForm_Activated(object sender, EventArgs e)
		{
			if (!Config.RunInBackground)
			{
				if (!_wasPaused)
				{
					UnpauseEmulator();
				}

				_wasPaused = false;
			}
		}

		private void MainForm_Deactivate(object sender, EventArgs e)
		{
			if (!Config.RunInBackground)
			{
				if (EmulatorPaused)
				{
					_wasPaused = true;
				}

				PauseEmulator();
			}
		}

		private void TimerMouseIdle_Tick(object sender, EventArgs e)
		{
			if (_inFullscreen && Config.DispChromeFullscreenAutohideMouse)
			{
				AutohideCursor(true);
			}
		}

		private void MainForm_Enter(object sender, EventArgs e)
		{
			AutohideCursor(false);
		}

		private void MainForm_Resize(object sender, EventArgs e)
		{
			_presentationPanel.Resized = true;
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{

			if (Config.RecentWatches.AutoLoad)
			{
				Tools.LoadRamWatch(!Config.DisplayRamWatch);
			}

			if (Config.Cheats.Recent.AutoLoad)
			{
				Tools.Load<Cheats>();
			}

			Tools.AutoLoad();
			HandlePlatformMenus();
		}

		protected override void OnClosed(EventArgs e)
		{
			_windowClosedAndSafeToExitProcess = true;
			base.OnClosed(e);
		}

		private void MainformMenu_MenuActivate(object sender, EventArgs e)
		{
			HandlePlatformMenus();
			MaybePauseFromMenuOpened();
		}

		public void MaybePauseFromMenuOpened()
		{
			if (Config.PauseWhenMenuActivated)
			{
				_wasPaused = EmulatorPaused;
				_didMenuPause = true;
				PauseEmulator();
			}
		}

		private void MainformMenu_MenuDeactivate(object sender, EventArgs e) => MaybeUnpauseFromMenuClosed();

		public void MaybeUnpauseFromMenuClosed()
		{
			if (!_wasPaused)
			{
				UnpauseEmulator();
			}
		}

		private static void FormDragEnter(object sender, DragEventArgs e)
		{
			e.Set(DragDropEffects.Copy);
		}

		private void FormDragDrop(object sender, DragEventArgs e)
		{
			Sound.StopSound();
			try
			{
				FormDragDrop_internal(e);
			}
			catch (Exception ex)
			{
				ShowMessageBox(owner: null, $"Exception on drag and drop:\n{ex}");
			}
			finally
			{
				Sound.StartSound();
			}
		}
	}
}
