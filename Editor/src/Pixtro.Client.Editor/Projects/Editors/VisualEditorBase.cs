using Pixtro.Emulation.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace Pixtro.Client.Editor.Projects
{
	public abstract class VisualEditorBase : IVideoProvider
	{
		internal int x, y;
		internal float scale;
		private int width, height;

		public int VirtualWidth
		{
			get => width;
			set
			{
				if (width != value)
				{
					sizeDirty = true;
					dirty = true;
					width = value;
				}
			}
		}

		public int VirtualHeight
		{
			get => height;
			set
			{
				if (height != value)
				{
					sizeDirty = true;
					dirty = true;
					height = value;
				}
			}
		}

		public int BufferWidth => VirtualWidth;

		public int BufferHeight => VirtualHeight;

		public int VsyncNumerator => 60;

		public int VsyncDenominator => 1;

		public int BackgroundColor => unchecked((int)0xff000000);

		internal virtual bool UseMovableLayer => false;

		private bool sizeDirty;
		internal bool dirty;

		private int[] buffer;
		private Bitmap finalMap;
		internal Bitmap layerMap;
		internal Graphics RenderGraphics { get; private set; }

		public VisualEditorBase()
		{
			finalMap = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
		}

		public int[] GetVideoBuffer()
		{
			if (dirty)
			{
				if (sizeDirty)
				{
					finalMap.Dispose();
					finalMap = new Bitmap(VirtualWidth, VirtualHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
				}
				StartRender();
				buffer = GetBuffer();
			}

			return buffer;
		}

		private unsafe int[] GetBuffer()
		{
			int[] values = new int[width * height];

			var bits = finalMap.LockBits(new Rectangle(0, 0, finalMap.Width, finalMap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			int* ptr = (int*)bits.Scan0;

			for (int y = 0; y < height; ++y)
			{
				for (int x = 0; x < width; ++x)
				{
					values[x + (y * width)] = ptr[x + (y * bits.Stride / 4)];
				}
			}

			finalMap.UnlockBits(bits);

			return values;
		}

		private void StartRender()
		{
			Brush bgBrush = new SolidBrush(Color.FromArgb(BackgroundColor));

			if (UseMovableLayer)
			{
				using (Graphics gfx = Graphics.FromImage(layerMap))
				{
					RenderGraphics = gfx;

					Render(gfx);

					RenderGraphics = null;
				}
				using (Graphics gfx = Graphics.FromImage(finalMap))
				{
					gfx.FillRectangle(bgBrush, new Rectangle(Point.Empty, finalMap.Size));

					gfx.DrawImage(layerMap, x, y, (int)(width * scale), (int)(height * scale));
				}
			}
			else
			{
				using (Graphics gfx = Graphics.FromImage(finalMap))
				{
					RenderGraphics = gfx;

					gfx.FillRectangle(bgBrush, new Rectangle(Point.Empty, finalMap.Size));

					Render(gfx);

					RenderGraphics = null;
				}
			}

		}
		internal abstract void Render(Graphics graphics);
	}
}
