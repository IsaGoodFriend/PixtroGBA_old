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
					TogglePause();
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
				case "Quick Load":
					LoadQuickSave($"QuickSave{Config.SaveSlot}");
					break;
				case "Quick Save":
					SaveQuickSave($"QuickSave{Config.SaveSlot}");
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
				case "Open ROM":
					OpenRom();
					break;
				case "Close ROM":
					CloseRom();
					break;
				case "Load Last ROM":
					LoadRomFromRecent(Config.RecentRoms.MostRecent);
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
				case "Input Display":
					ToggleInputDisplay();
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
				case "Record A/V":
					RecordAv();
					break;
				case "Stop A/V":
					StopAv();
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

				// Save States
				case "Save State 0":
					SaveQuickSave("QuickSave0");
					Config.SaveSlot = 0;
					UpdateStatusSlots();
					break;
				case "Save State 1":
					SaveQuickSave("QuickSave1");
					Config.SaveSlot = 1;
					UpdateStatusSlots();
					break;
				case "Save State 2":
					SaveQuickSave("QuickSave2");
					Config.SaveSlot = 2;
					UpdateStatusSlots();
					break;
				case "Save State 3":
					SaveQuickSave("QuickSave3");
					Config.SaveSlot = 3;
					UpdateStatusSlots();
					break;
				case "Save State 4":
					SaveQuickSave("QuickSave4");
					Config.SaveSlot = 4;
					UpdateStatusSlots();
					break;
				case "Save State 5":
					SaveQuickSave("QuickSave5");
					Config.SaveSlot = 5;
					UpdateStatusSlots();
					break;
				case "Save State 6":
					SaveQuickSave("QuickSave6");
					Config.SaveSlot = 6;
					UpdateStatusSlots();
					break;
				case "Save State 7":
					SaveQuickSave("QuickSave7");
					Config.SaveSlot = 7;
					UpdateStatusSlots();
					break;
				case "Save State 8":
					SaveQuickSave("QuickSave8");
					Config.SaveSlot = 8;
					UpdateStatusSlots();
					break;
				case "Save State 9":
					SaveQuickSave("QuickSave9");
					Config.SaveSlot = 9;
					UpdateStatusSlots();
					break;
				case "Load State 0":
					LoadQuickSave("QuickSave0");
					Config.SaveSlot = 0;
					UpdateStatusSlots();
					break;
				case "Load State 1":
					LoadQuickSave("QuickSave1");
					Config.SaveSlot = 1;
					UpdateStatusSlots();
					break;
				case "Load State 2":
					LoadQuickSave("QuickSave2");
					Config.SaveSlot = 2;
					UpdateStatusSlots();
					break;
				case "Load State 3":
					LoadQuickSave("QuickSave3");
					Config.SaveSlot = 3;
					UpdateStatusSlots();
					break;
				case "Load State 4":
					LoadQuickSave("QuickSave4");
					Config.SaveSlot = 4;
					UpdateStatusSlots();
					break;
				case "Load State 5":
					LoadQuickSave("QuickSave5");
					Config.SaveSlot = 5;
					UpdateStatusSlots();
					break;
				case "Load State 6":
					LoadQuickSave("QuickSave6");
					Config.SaveSlot = 6;
					UpdateStatusSlots();
					break;
				case "Load State 7":
					LoadQuickSave("QuickSave7");
					Config.SaveSlot = 7;
					UpdateStatusSlots();
					break;
				case "Load State 8":
					LoadQuickSave("QuickSave8");
					Config.SaveSlot = 8;
					UpdateStatusSlots();
					break;
				case "Load State 9":
					LoadQuickSave("QuickSave9");
					Config.SaveSlot = 9;
					UpdateStatusSlots();
					break;

				case "Select State 0":
					SelectSlot(0);
					break;
				case "Select State 1":
					SelectSlot(1);
					break;
				case "Select State 2":
					SelectSlot(2);
					break;
				case "Select State 3":
					SelectSlot(3);
					break;
				case "Select State 4":
					SelectSlot(4);
					break;
				case "Select State 5":
					SelectSlot(5);
					break;
				case "Select State 6":
					SelectSlot(6);
					break;
				case "Select State 7":
					SelectSlot(7);
					break;
				case "Select State 8":
					SelectSlot(8);
					break;
				case "Select State 9":
					SelectSlot(9);
					break;
				case "Save Named State":
					SaveStateAs();
					break;
				case "Load Named State":
					LoadStateAs();
					break;
				case "Previous Slot":
					PreviousSlot();
					break;
				case "Next Slot":
					NextSlot();
					break;

				// Movie
				case "Toggle read-only":
					ToggleReadOnly();
					break;
				case "Play Movie":
					PlayMovieMenuItem_Click(null, null);
					break;
				case "Record Movie":
					RecordMovieMenuItem_Click(null, null);
					break;
				case "Stop Movie":
					StopMovie();
					break;
				case "Play from beginning":
					RestartMovie();
					break;
				case "Save Movie":
					SaveMovie();
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
				case "Trace Logger":
					Tools.Load<TraceLogger>();
					break;
				case "Lua Console":
					OpenLuaConsole();
					break;
				case "Cheats":
					Tools.Load<Cheats>();
					break;
				case "Toggle All Cheats":
					if (CheatList.Any())
					{
						string type = " (mixed)";
						if (CheatList.All(c => c.Enabled))
						{
							type = " (off)";
						}
						else if (CheatList.All(c => !c.Enabled))
						{
							type = " (on)";
						}

						foreach (var x in CheatList)
						{
							x.Toggle();
						}

						AddOnScreenMessage($"Cheats toggled{type}");
					}

					break;
				case "ToolBox":
					if (!OSTailoredCode.IsUnixHost) Tools.Load<ToolBox>();
					break;
				case "Virtual Pad":
					Tools.Load<VirtualpadTool>();
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

				// Analog
				case "Y Up Small":
					Tools.VirtualPad.BumpAnalogValue(null, Config.AnalogSmallChange);
					break;
				case "Y Up Large":
					Tools.VirtualPad.BumpAnalogValue(null, Config.AnalogLargeChange);
					break;
				case "Y Down Small":
					Tools.VirtualPad.BumpAnalogValue(null, -Config.AnalogSmallChange);
					break;
				case "Y Down Large":
					Tools.VirtualPad.BumpAnalogValue(null, -Config.AnalogLargeChange);
					break;
				case "X Up Small":
					Tools.VirtualPad.BumpAnalogValue(Config.AnalogSmallChange, null);
					break;
				case "X Up Large":
					Tools.VirtualPad.BumpAnalogValue(Config.AnalogLargeChange, null);
					break;
				case "X Down Small":
					Tools.VirtualPad.BumpAnalogValue(-Config.AnalogSmallChange, null);
					break;
				case "X Down Large":
					Tools.VirtualPad.BumpAnalogValue(-Config.AnalogLargeChange, null);
					break;

				// DS
				case "Next Screen Layout":
					IncrementDSLayout(1);
					break;
				case "Previous Screen Layout":
					IncrementDSLayout(-1);
					break;
				case "Screen Rotate":
					IncrementDSScreenRotate();
					break;
			}

			return true;
		}

		private void IncrementDSScreenRotate()
		{
		}

		private void IncrementDSLayout(int delta)
		{
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
