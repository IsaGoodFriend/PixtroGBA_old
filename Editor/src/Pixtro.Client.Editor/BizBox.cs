using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Pixtro.Client.Editor.Properties;
using Pixtro.Common;
using Pixtro.Emulation.Cores;

namespace Pixtro.Client.Editor
{
	public partial class BizBox : Form
	{
		public BizBox()
		{
			InitializeComponent();
			Icon = Resources.Logo;
			pictureBox1.Image = Resources.CorpHawk;
			btnCopyHash.Image = Resources.Duplicate;
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			linkLabel1.LinkVisited = true;
			Process.Start(VersionInfo.HomePage);
		}

		private void OK_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void BizBox_Load(object sender, EventArgs e)
		{
			string mainVersion = VersionInfo.MainVersion;
			if (IntPtr.Size == 8)
			{
				mainVersion += " (x64)";
			}

			DeveloperBuildLabel.Visible = VersionInfo.DeveloperBuild;

			Text = "Hi";

			VersionLabel.Text = $"Version {mainVersion}";
			DateLabel.Text = VersionInfo.ReleaseDate;

			foreach (var core in CoreInventory.Instance.SystemsFlat.Where(core => core.CoreAttr.Released)
				.OrderByDescending(core => core.Name.ToLowerInvariant()))
			{
				CoreInfoPanel.Controls.Add(new BizBoxInfoControl(core.CoreAttr)
				{
					Dock = DockStyle.Top
				});
			}

			linkLabel2.Text = $"Commit asdf";
		}

		private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
		}

		private void btnCopyHash_Click(object sender, EventArgs e)
		{
		}

		private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("https://github.com/TASVideos/BizHawk/graphs/contributors");
		}
	}
}
