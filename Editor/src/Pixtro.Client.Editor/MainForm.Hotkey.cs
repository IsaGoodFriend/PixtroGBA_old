using System;
using System.Linq;

using Pixtro.Common;
using Pixtro.Emulation.Common;

namespace Pixtro.Client.Editor
{
	public partial class MainForm
	{
		private bool CheckHotkey(string trigger)
		{
			switch (trigger)
			{
				default:
					return false;

				// General
				case "Pause":
					ToggleGamePause();
					break;
				case "Pause Emulator":
					ToggleEmulatorPause();
					break;
				case "Frame Inch":
					//special! allow this key to get handled as Frame Advance, too
					FrameInch = true;
					return false;
				case "Toggle Throttle":
					_unthrottled ^= true;
					ThrottleMessage();
					break;
				case "Soft Reset":
					SoftReset();
					break;
				case "Hard Reset":
					HardReset();
					break;
				case "Clear Autohold":
					ClearAutohold();
					break;
				case "Screenshot":
					TakeScreenshot();
					break;
				case "Screen Raw to Clipboard":
					// Ctrl+C clash. any tool that has such acc must check this.
					// maybe check if mainform has focus instead?

					TakeScreenshotToClipboard();
					break;
				case "Screen Client to Clipboard":
					TakeScreenshotClientToClipboard();
					break;
				case "Full Screen":
					ToggleFullscreen();
					break;
				case "Flush SaveRAM":
					FlushSaveRAM();
					break;
				case "Display FPS":
					ToggleFps();
					break;
				case "Frame Counter":
					ToggleFrameCounter();
					break;
				case "Lag Counter":
					if (Emulator.CanPollInput())
					{
						ToggleLagCounter();
					}

					break;
				case "Toggle BG Input":
					ToggleBackgroundInput();
					break;
				case "Toggle Menu":
					MainMenuStrip.Visible ^= true;
					break;
				case "Volume Up":
					VolumeUp();
					break;
				case "Volume Down":
					VolumeDown();
					break;
				case "Toggle Sound":
					ToggleSound();
					break;
				case "Exit Program":
					_exitRequestPending = true;
					break;
				case "Larger Window":
					IncreaseWindowSize();
					break;
				case "Smaller Window":
					DecreaseWindowSize();
					break;
				case "Increase Speed":
					IncreaseSpeed();
					break;
				case "Reset Speed":
					ResetSpeed();
					break;
				case "Decrease Speed":
					DecreaseSpeed();
					break;
				case "Reboot Core":
					RebootCore();
					break;
				case "Toggle Skip Lag Frame":
					Config.SkipLagFrame ^= true;
					AddOnScreenMessage($"Skip Lag Frames toggled {(Config.SkipLagFrame ? "On" : "Off")}");
					break;
				case "Toggle Key Priority":
					ToggleKeyPriority();
					break;

				// Project
				case "Open Project":
					OpenProject();
					break;
				case "Close Project":
					CloseProject();
					break;
				// TODO: Implement hot keys
				case "Load Last Project":
					
					return false;
				case "Build Project":
					BuildProject(false);
					break;
				case "Run Project":
					RunProject();
					break;
				case "Build And Run":
					BuildAndRun();
					break;

				// Tools
				case "RAM Watch":
					Tools.LoadRamWatch(true);
					break;
				case "RAM Search":
					Tools.Load<RamSearch>();
					break;
				case "Hex Editor":
					Tools.Load<HexEditor>();
					break;

				// RAM Search
				case "Do Search":
					if (Tools.IsLoaded<RamSearch>())
					{
						Tools.RamSearch.DoSearch();
					}
					else
					{
						return false;
					}

					break;
				case "New Search":
					if (Tools.IsLoaded<RamSearch>())
					{
						Tools.RamSearch.NewSearch();
					}
					else
					{
						return false;
					}

					break;
				case "Previous Compare To":
					if (Tools.IsLoaded<RamSearch>())
					{
						Tools.RamSearch.NextCompareTo(reverse: true);
					}
					else
					{
						return false;
					}

					break;
				case "Next Compare To":
					if (Tools.IsLoaded<RamSearch>())
					{
						Tools.RamSearch.NextCompareTo();
					}
					else
					{
						return false;
					}

					break;
				case "Previous Operator":
					if (Tools.IsLoaded<RamSearch>())
					{
						Tools.RamSearch.NextOperator(reverse: true);
					}
					else
					{
						return false;
					}

					break;
				case "Next Operator":
					if (Tools.IsLoaded<RamSearch>())
					{
						Tools.RamSearch.NextOperator();
					}
					else
					{
						return false;
					}

					break;
			}

			return true;
		}

		// Determines if the value is a hotkey  that would be handled outside of the CheckHotkey method
		private bool IsInternalHotkey(string trigger)
		{
			switch (trigger)
			{
				default:
					return false;
				case "Autohold":
				case "Autofire":
				case "Frame Advance":
				case "Turbo":
				case "Rewind":
				case "Fast Forward":
					return true;
			}
		}
	}
}
