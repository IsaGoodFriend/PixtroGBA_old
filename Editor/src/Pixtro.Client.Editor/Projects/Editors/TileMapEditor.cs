using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pixtro.Emulation.Common;
using System.Drawing;

namespace Pixtro.Client.Editor.Projects.Editors
{
	public class TileMapEditor : VisualEditorBase
	{
		public TileMapEditor()
		{
			Resize(128, 128);
		}
		public const string TILEMAP_DISPLAY = "tilemap";

		public static int TileSize = 16;

		public int X
		{
			get => x;
			set
			{
				if (x != value)
				{
					x = value;
					dirty = true;
				}
			}
		}
		public int Y
		{
			get => y;
			set
			{
				if (y != value)
				{
					y = value;
					dirty = true;
				}
			}
		}

		public void Resize(int width, int height)
		{
			Bitmap mapNew = new Bitmap(width * TileSize, height * TileSize);

			if (layerMap != null)
			{
				using (Graphics gfx = Graphics.FromImage(mapNew))
				{
					gfx.DrawImageUnscaledAndClipped(layerMap, new Rectangle(0, 0, layerMap.Width, layerMap.Height));
				}

				layerMap.Dispose();
			}

			layerMap = mapNew;
		}

		private void UpdateBlock(int x, int y)
		{
			RenderGraphics.FillRectangle(Brushes.Red, new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize));
		}

		internal override void Render(Graphics graphics)
		{
			for (int i = 0; i < 128; ++i)
				UpdateBlock(i, i);

		}
	}
}
