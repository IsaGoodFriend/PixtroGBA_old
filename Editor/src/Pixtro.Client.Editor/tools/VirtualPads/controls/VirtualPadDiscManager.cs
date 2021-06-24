﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Pixtro.Client.Common;
using Pixtro.Emulation.Common;

namespace Pixtro.Client.Editor
{
	public partial class VirtualPadDiscManager : UserControl, IVirtualPadControl
	{
		private readonly InputManager _inputManager;

		public VirtualPadDiscManager(
			InputManager inputManager,
			IEmulator ownerEmulator,
			string name,
			IReadOnlyList<string> buttonNames)
		{
			_inputManager = inputManager;
			_ownerEmulator = ownerEmulator;
			Name = name;
			InitializeComponent();
			btnOpen.InputManager = _inputManager;
			btnClose.InputManager = _inputManager;
			btnOpen.Name = buttonNames[0];
			btnClose.Name = buttonNames[1];
			_discSelectName = buttonNames[2];

			// these need to follow InitializeComponent call
			UpdateCoreAssociation();
			UpdateValues();
		}

		private readonly string _discSelectName;
		private readonly object _ownerEmulator;

		private void UpdateCoreAssociation()
		{

			var buttons = new List<string> { "- NONE -" };

			lvDiscs.Items.Clear();

			int idx = 0;
			foreach (var button in buttons)
			{
				var lvi = new ListViewItem { Text = idx.ToString() };
				lvi.SubItems.Add(button);
				lvDiscs.Items.Add(lvi);
				idx++;
			}
		}


		public void Clear()
		{
		}

		public void UpdateValues()
		{
			// make sure we try to keep something selected here, for clarity.
			// but maybe later we'll just make it so that unselecting means no disc and don't display the disc 0
			if (lvDiscs.SelectedIndices.Count == 0)
				lvDiscs.SelectedIndices.Add(0);
		}

		public void Set(IController controller)
		{
			//controller.AxisValue("Disc Select")
		}

		public bool ReadOnly { get; set; }

		private void lvDiscs_SelectedIndexChanged(object sender, EventArgs e)
		{
			// emergency measure: if no selection, set no disc
			_inputManager.StickyXorAdapter.SetAxis(_discSelectName, lvDiscs.SelectedIndices.Count == 0 ? 0 : lvDiscs.SelectedIndices[0]);
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			if (lblTimeZero.Visible)
			{
				btnOpen.Checked = !btnClose.Checked;
				UpdateValues();
			}
		}

		private void btnOpen_Click(object sender, EventArgs e)
		{
			if (lblTimeZero.Visible)
			{
				btnClose.Checked = !btnOpen.Checked;
				UpdateValues();
			}
		}
	}
}