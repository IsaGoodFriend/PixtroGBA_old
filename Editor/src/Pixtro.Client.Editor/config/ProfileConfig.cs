using System;
using System.Windows.Forms;

using Pixtro.Client.Common;
using Pixtro.Emulation.Common;
using Pixtro.Emulation.Cores;

namespace Pixtro.Client.Editor
{
	public partial class ProfileConfig : Form
	{
		private readonly IMainFormForConfig _mainForm;
		private readonly IEmulator _emulator;
		private readonly Config _config;

		public ProfileConfig(
			IMainFormForConfig mainForm,
			IEmulator emulator,
			Config config)
		{
			_mainForm = mainForm;
			_emulator = emulator;
			_config = config;
			InitializeComponent();
			Icon = Properties.Resources.ProfileIcon;
		}

		private void ProfileConfig_Load(object sender, EventArgs e)
		{
			ProfileSelectComboBox.SelectedItem = _config.SelectedProfile switch
			{
				ClientProfile.Casual => "Casual Gaming",
				ClientProfile.Longplay => "Longplays",
				ClientProfile.Tas => "Tool-assisted Speedruns",
				ClientProfile.N64Tas => "N64 Tool-assisted Speedruns",
				_ => "Casual Gaming"
			};

			AutoCheckForUpdates.Checked = _config.UpdateAutoCheckEnabled;
		}

		private void OkBtn_Click(object sender, EventArgs e)
		{
			_config.SelectedProfile = ProfileSelectComboBox.SelectedItem.ToString() switch
			{
				"Longplays" => ClientProfile.Longplay,
				"Tool-assisted Speedruns" => ClientProfile.Tas,
				"N64 Tool-assisted Speedruns" => ClientProfile.N64Tas,
				_ => ClientProfile.Casual
			};

			SetCasual();

			switch (_config.SelectedProfile)
			{
				case ClientProfile.Longplay:
					SetLongPlay();
					break;
				case ClientProfile.Tas:
					SetTas();
					break;
				case ClientProfile.N64Tas:
					SetTas();
					SetN64Tas();
					break;
			}

			bool oldUpdateAutoCheckEnabled = _config.UpdateAutoCheckEnabled;
			_config.UpdateAutoCheckEnabled = AutoCheckForUpdates.Checked;
			if (_config.UpdateAutoCheckEnabled != oldUpdateAutoCheckEnabled)
			{
				UpdateChecker.GlobalConfig = _config;
				if (!_config.UpdateAutoCheckEnabled)
				{
					UpdateChecker.ResetHistory();
				}

				UpdateChecker.BeginCheck(); // Call even if auto checking is disabled to trigger event (it won't actually check)
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		private void CancelBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void SetCasual()
		{
			_config.Savestates.NoLowResLargeScreenshots = false;
			_config.Savestates.SaveScreenshot = false;
			_config.AllowUdlr = false;
			_config.Savestates.MakeBackups = false;

			_config.Savestates.CompressionLevelNormal = 0;
			_config.Rewind.Enabled = true;
			_config.SkipLagFrame = false;

			// GB
			_config.PreferredCores["GB"] = CoreNames.Gambatte;
			_config.PreferredCores["GBC"] = CoreNames.Gambatte;
		}

		private void SetLongPlay()
		{
			_config.Savestates.CompressionLevelNormal = 5;

			// GB
			_config.PreferredCores["GB"] = CoreNames.Gambatte;
			_config.PreferredCores["GBC"] = CoreNames.Gambatte;
		}

		private void SetTas()
		{
			// General
			_config.Savestates.SaveScreenshot = true;
			_config.AllowUdlr = true;
			_config.Savestates.MakeBackups = true;
			_config.SkipLagFrame = false;
			_config.Savestates.CompressionLevelNormal = 5;

			// Rewind
			_config.Rewind.Enabled = false;

			
		}

		private void SetN64Tas()
		{
			// General
			_config.Savestates.MakeBackups = false;
			_config.SkipLagFrame = true;
			_config.Savestates.CompressionLevelNormal = 0;
		}

		private TSetting GetSyncSettings<TEmulator, TSetting>()
			where TSetting : class, new()
			where TEmulator : IEmulator
		{
			object fromCore = null;
			var settable = new SettingsAdapter(_emulator);
			if (settable.HasSyncSettings)
			{
				fromCore = settable.GetSyncSettings();
			}

			return fromCore as TSetting
				?? _config.GetCoreSyncSettings<TEmulator, TSetting>()
				?? new TSetting(); // guaranteed to give sensible defaults
		}

		private void PutSyncSettings<TEmulator>(object o)
			where TEmulator : IEmulator
		{
			if (_emulator is TEmulator)
			{
				_mainForm.PutCoreSyncSettings(o);
			}
			else
			{
				_config.PutCoreSyncSettings<TEmulator>(o);
			}
		}
	}
}
