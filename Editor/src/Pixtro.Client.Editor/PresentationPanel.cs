﻿using System;
using System.Drawing;
using System.Windows.Forms;

using Pixtro.Client.Common;
using Pixtro.Emuware.BizwareGL;

namespace Pixtro.Client.Editor
{
	/// <summary>
	/// Thinly wraps a BizwareGL.GraphicsControl for EmuHawk's needs
	/// </summary>
	public class PresentationPanel
	{
		private readonly Config _config;


		public PresentationPanel(
			Config config,
			IGL gl,
			MainForm form)
		{
			_config = config;


			GraphicsControl = new GraphicsControl(gl)
			{
				Dock = DockStyle.None,
				BackColor = Color.Black
			};

			// pass through these events to the form. we might need a more scalable solution for mousedown etc. for zapper and whatnot.
			// http://stackoverflow.com/questions/547172/pass-through-mouse-events-to-parent-control (HTTRANSPARENT)
			GraphicsControl.MouseDown += form.MainForm_MouseDown;
			GraphicsControl.MouseUp += form.MainForm_MouseUp;
			GraphicsControl.MouseMove += form.MainForm_MouseMove;
			GraphicsControl.MouseWheel += form.MainForm_MouseWheel;
		}

		private bool _isDisposed;
		public void Dispose()
		{
			if (_isDisposed) return;
			_isDisposed = true;
			GraphicsControl.Dispose();
		}

		//graphics resources
		public GraphicsControl GraphicsControl;

		public Control Control => GraphicsControl;
		public static implicit operator Control(PresentationPanel self) { return self.GraphicsControl; }

		public bool Resized { get; set; }

		public Size NativeSize => GraphicsControl.ClientSize;
	}
}
