using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Linq;

using Pixtro.Emulation.Common;
using Pixtro.Client.Common;
using Pixtro.Common;

namespace Pixtro.Client.Editor
{
	// TODO - Allow relative paths in record TextBox
	public partial class RecordMovie : Form, IDialogParent
	{
		private readonly IMainFormForTools _mainForm;
		private readonly Config _config;
		private readonly GameInfo _game;
		private readonly IEmulator _emulator;
		private readonly FirmwareManager _firmwareManager;

		public IDialogController DialogController => _mainForm;

		public RecordMovie(
			IMainFormForTools mainForm,
			Config config,
			GameInfo game,
			IEmulator core,
			FirmwareManager firmwareManager)
		{
			_mainForm = mainForm;
			_config = config;
			_game = game;
			_emulator = core;
			_firmwareManager = firmwareManager;
			InitializeComponent();
			BrowseBtn.Image = Properties.Resources.OpenFile;
			if (OSTailoredCode.IsUnixHost) Load += (_, _) =>
			{
				//HACK to make this usable on Linux. No clue why this Form in particular is so much worse, maybe the GroupBox? --yoshi
				DefaultAuthorCheckBox.Location += new Size(0, 20);
				var s = new Size(0, 36);
				OK.Location += s;
				Cancel.Location += s;
			};

			if (!_emulator.HasSavestates())
			{
				StartFromCombo.Items.Remove(
					StartFromCombo.Items
						.OfType<object>()
						.First(i => i.ToString()
							.ToLower() == "now"));
			}

			if (!_emulator.HasSaveRam())
			{
				StartFromCombo.Items.Remove(
					StartFromCombo.Items
						.OfType<object>()
						.First(i => i.ToString()
							.ToLower() == "saveram"));
			}
		}

		private string MakePath()
		{
			var path = RecordBox.Text;

			if (!string.IsNullOrWhiteSpace(path))
			{
				if (path.LastIndexOf(Path.DirectorySeparatorChar) == -1)
				{
					if (path[0] != Path.DirectorySeparatorChar)
					{
						path = path.Insert(0, Path.DirectorySeparatorChar.ToString());
					}

					path = _config.PathEntries.MovieAbsolutePath() + path;

				}
			}

			return path;
		}

		private void Ok_Click(object sender, EventArgs e)
		{
			var path = MakePath();
			if (!string.IsNullOrWhiteSpace(path))
			{
				var test = new FileInfo(path);
				if (test.Exists)
				{
					var result = DialogController.ShowMessageBox2($"{path} already exists, overwrite?", "Confirm overwrite", EMsgBoxIcon.Warning, useOKCancel: true);
					if (!result)
					{
						return;
					}
				}

				var fileInfo = new FileInfo(path);
				if (!fileInfo.Exists)
				{
					Directory.CreateDirectory(fileInfo.DirectoryName);
				}

				_config.UseDefaultAuthor = DefaultAuthorCheckBox.Checked;
				if (DefaultAuthorCheckBox.Checked)
				{
					_config.DefaultAuthor = AuthorBox.Text;
				}

				Close();
			}
			else
			{
				DialogController.ShowMessageBox("Please select a movie to record", "File selection error", EMsgBoxIcon.Error);
			}
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void BrowseBtn_Click(object sender, EventArgs e)
		{
			string movieFolderPath = _config.PathEntries.MovieAbsolutePath();
			
			// Create movie folder if it doesn't already exist
			try
			{
				if (!Directory.Exists(movieFolderPath))
				{
					Directory.CreateDirectory(movieFolderPath);
				}
			}
			catch (Exception movieDirException)
			{
				if (movieDirException is IOException
					|| movieDirException is UnauthorizedAccessException)
				{
					//TO DO : Pass error to user?
				}
				else throw;
			}
			
			var preferredExt = "bk2";
			using var sfd = new SaveFileDialog
			{
				InitialDirectory = movieFolderPath,
				DefaultExt = $".{preferredExt}",
				FileName = RecordBox.Text,
				OverwritePrompt = false,
				Filter = new FilesystemFilterSet(new FilesystemFilter("Movie Files", new[] { preferredExt })).ToString()
			};

			var result = this.ShowDialogWithTempMute(sfd);
			if (result == DialogResult.OK
				&& !string.IsNullOrWhiteSpace(sfd.FileName))
			{
				RecordBox.Text = sfd.FileName;
			}
		}

		private void RecordMovie_Load(object sender, EventArgs e)
		{
			RecordBox.Text = _game.FilesystemSafeName();
			StartFromCombo.SelectedIndex = 0;
			DefaultAuthorCheckBox.Checked = _config.UseDefaultAuthor;
			if (_config.UseDefaultAuthor)
			{
				AuthorBox.Text = _config.DefaultAuthor;
			}
		}

		private void RecordBox_DragEnter(object sender, DragEventArgs e)
		{
			e.Set(DragDropEffects.Copy);
		}

		private void RecordBox_DragDrop(object sender, DragEventArgs e)
		{
			var filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
			RecordBox.Text = filePaths[0];
		}
	}
}
