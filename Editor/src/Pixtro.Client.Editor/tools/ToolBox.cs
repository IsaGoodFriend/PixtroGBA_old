using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Pixtro.Client.Common;
using Pixtro.Common;
using Pixtro.Emulation.Common;

namespace Pixtro.Client.Editor
{
	public partial class ToolBox : ToolFormBase
	{
		[RequiredService]
		private IEmulator Emulator { get; set; }

		protected override string WindowTitleStatic => string.Empty;

		public ToolBox()
		{
			InitializeComponent();
			Icon = Properties.Resources.ToolBoxIcon;
		}

		private void ToolBox_Load(object sender, EventArgs e)
		{
			if (OSTailoredCode.IsUnixHost)
			{
				Close();
				return;
			}
			Location = new Point(
				Owner.Location.X + Owner.Size.Width,
				Owner.Location.Y
			);
		}

		public override void Restart()
		{
			if (OSTailoredCode.IsUnixHost) return;
			SetTools();
			SetSize();

			ToolBoxStrip.Select();
			ToolBoxItems.First().Select();
		}

		private void SetTools()
		{
			ToolBoxStrip.Items.Clear();

			var tools = Pixtro.Common.ReflectionCache.Types
				.Where(t => typeof(IToolForm).IsAssignableFrom(t))
				.Where(t => typeof(Form).IsAssignableFrom(t))
				.Where(t => !typeof(ToolBox).IsAssignableFrom(t))
				.Where(t => ServiceInjector.IsAvailable(Emulator.ServiceProvider, t))
				.Where(t => VersionInfo.DeveloperBuild || !t.GetCustomAttributes(false).OfType<ToolAttribute>().Any(a => !a.Released));

			/*
			for (int i = 0; i < tools.Count(); i++)
			{
				Console.WriteLine(tools.ElementAt(i).FullName);
			}
			*/

			foreach (var t in tools)
			{
				if (t.FullName != "Pixtro.Client.Editor.ToolFormBase")
				{
					//var instance = Activator.CreateInstance(t);

					var image_t = Properties.Resources.Logo;
					var text_t = "";

					if (t.FullName == "Pixtro.Client.Editor.CoreFeatureAnalysis") { image_t = Properties.Resources.Logo; }
					if (t.FullName == "Pixtro.Client.Editor.LogWindow")			{ image_t = Properties.Resources.CommandWindow; }
					if (t.FullName == "Pixtro.Client.Editor.LuaConsole")			{ image_t = Properties.Resources.TextDocIcon; }
					if (t.FullName == "Pixtro.Client.Editor.MultiDiskBundler")	{ image_t = Properties.Resources.DualIcon; }
					if (t.FullName == "Pixtro.Client.Editor.VirtualpadTool")		{ image_t = Properties.Resources.GameControllerIcon; }
					if (t.FullName == "Pixtro.Client.Editor.BasicBot")			{ image_t = Properties.Resources.BasicBot; }
					if (t.FullName == "Pixtro.Client.Editor.CDL")					{ image_t = Properties.Resources.CdLoggerIcon; }
					if (t.FullName == "Pixtro.Client.Editor.Cheats")				{ image_t = Properties.Resources.BugIcon; }
					if (t.FullName == "Pixtro.Client.Editor.GenericDebugger")		{ image_t = Properties.Resources.BugIcon; }
					if (t.FullName == "Pixtro.Client.Editor.GameShark")			{ image_t = Properties.Resources.SharkIcon; }
					if (t.FullName == "Pixtro.Client.Editor.GBPrinterView")		{ image_t = Properties.Resources.GambatteIcon; }
					if (t.FullName == "Pixtro.Client.Editor.GbGpuView")			{ image_t = Properties.Resources.GambatteIcon; }
					if (t.FullName == "Pixtro.Client.Editor.HexEditor")			{ image_t = Properties.Resources.FreezeIcon; }
					if (t.FullName == "Pixtro.Client.Editor.TraceLogger")			{ image_t = Properties.Resources.PencilIcon; }
					if (t.FullName == "Pixtro.Client.Editor.RamSearch")			{ image_t = Properties.Resources.SearchIcon; }
					if (t.FullName == "Pixtro.Client.Editor.RamWatch")			{ image_t = Properties.Resources.WatchIcon; }
					if (t.FullName == "Pixtro.Client.Editor.NESSoundConfig")		{ image_t = Properties.Resources.NesControllerIcon; }
					if (t.FullName == "Pixtro.Client.Editor.NESMusicRipper")		{ image_t = Properties.Resources.NesControllerIcon; }
					if (t.FullName == "Pixtro.Client.Editor.NesPPU")				{ image_t = Properties.Resources.MonitorIcon; }
					if (t.FullName == "Pixtro.Client.Editor.NESNameTableViewer")	{ image_t = Properties.Resources.MonitorIcon; }
					if (t.FullName == "Pixtro.Client.Editor.SmsVdpViewer")		{ image_t = Properties.Resources.SmsIcon; }

					var tsb = new ToolStripButton
					{
						Image = image_t.ToBitmap(),
						Text = text_t,
						DisplayStyle = ToolStripItemDisplayStyle.Image
						//Image = ((Form)instance).Icon.ToBitmap(),
						//Text = ((Form)instance).Text,
						//DisplayStyle = ((Form)instance).ShowIcon ? ToolStripItemDisplayStyle.Image : ToolStripItemDisplayStyle.Text
					};

					tsb.Click += (o, e) =>
					{
						Tools.Load(t);
						//Close();
					};

					ToolBoxStrip.Items.Add(tsb);
				}
			}
		}

		private void SetSize()
		{
			var rows = (int)Math.Ceiling(ToolBoxItems.Count() / 4.0);
			Height = 30 + (rows * 30);
		}

		// Provide LINQ capabilities to an outdated form collection
		private IEnumerable<ToolStripItem> ToolBoxItems => ToolBoxStrip.Items.Cast<ToolStripItem>();

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				Close();
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}
