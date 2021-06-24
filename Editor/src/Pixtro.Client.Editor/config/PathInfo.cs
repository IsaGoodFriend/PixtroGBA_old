using System;
using System.Windows.Forms;

namespace Pixtro.Client.Editor
{
	public partial class PathInfo : Form
	{
		public PathInfo()
		{
			InitializeComponent();
		}

		private void Ok_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
