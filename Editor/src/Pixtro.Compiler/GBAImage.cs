using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixtro.Compiler
{
	public class BackgroundTiles : Tileset
	{

		public BackgroundTiles(int width, int height) : base(width, height)
		{

		}

		public override IEnumerable<uint> Data(string _name)
		{
			if (tiles.Count > 192)
				throw new Exception();
			if (tiles.Count > 128)
				Compiler.WarningLog($"Background {_name} has a lot of tiles ({tiles.Count}).  It's recommended that you lower tile count");

			return base.Data(_name);
		}
	}

	public class Background
	{
		public BackgroundTiles tileset { get; private set; }

		private int width, height;
		private Tile[,] tiles;

		private ushort[,] rawData;

		private List<ushort[]> palettesBase;
		private int[,] paletteIdx;

		public Background(Bitmap _map, BackgroundTiles _tiles)
		{
			width = _map.Width >> 3;
			height = _map.Height >> 3;

			tiles = new Tile[width, height];

			rawData = new ushort[_map.Width, _map.Height];

			for (int y = 0; y < _map.Height; ++y)
				for (int x = 0; x < _map.Width; ++x)
					rawData[x, y] = _map.GetPixel(x, y).ToGBA();

			palettesBase = new List<ushort[]>();
			List<Color> palette = new List<Color>(_map.Palette.Entries);

			for (int i = 0; i < palette.Count; i += 16)
			{
				List<ushort> pal = new List<ushort>();

				bool hasColor = false;

				for (int j = 0; j < 16; ++j)
				{
					ushort color = palette[i + j].ToGBA();
					pal.Add(color);

					hasColor |= color != 0;
				}

				if (!hasColor)
					break;

				palettesBase.Add(pal.ToArray());
			}

			tileset = _tiles??new BackgroundTiles(width, height);

			SetTiles();
		}
		public Background(AsepriteReader _read, BackgroundTiles _tiles, string _layer)
		{
			width = _read.Width >> 3;
			height = _read.Height >> 3;

			tiles = new Tile[width, height];

			rawData = new ushort[_read.Width, _read.Height];

			for (int y = 0; y < _read.Height; ++y)
				for (int x = 0; x < _read.Width; ++x)
					rawData[x, y] = _read.ReadColor(x, y, _layer: _layer).ToGBA();

			palettesBase = new List<ushort[]>();
			List<FloatColor> palette = new List<FloatColor>(_read.Colors);

			for (int i = 0; i < palette.Count; i += 16)
			{
				List<ushort> pal = new List<ushort>();

				bool hasColor = false;

				for (int j = 0; j < 16; ++j)
				{
					ushort color = palette[i + j].ToGBA();
					pal.Add(color);

					hasColor |= color != 0;
				}

				if (!hasColor)
					break;

				palettesBase.Add(pal.ToArray());
			}

			tileset = _tiles ?? new BackgroundTiles(width, height);

			SetTiles();
		}


		private void SetTiles()
		{
			paletteIdx = new int[width, height];

			for (int yL = 0; yL < height; yL += 8)
			{
				for (int xL = 0; xL < width; xL += 8)
				{
					int p;

					for (p = 0; p < palettesBase.Count; ++p)
					{
						bool safe = true;
						for (int y = 0; y < 8; ++y)
						{
							for (int x = 0; x < 8; ++x)
							{
								if (rawData[xL + x, yL + y] != 0x8000 && !palettesBase[p].Contains(rawData[xL + x, yL + y]))
								{
									safe = false;
									break;
								}
							}
							if (!safe)
								break;
						}

						if (safe)
							break;
					}

					if (p == palettesBase.Count)
						throw new Exception();
				}
			}

			uint getOffset(int x, int y)
			{
				return (uint)Array.IndexOf(palettesBase[paletteIdx[x >> 3, y >> 3]], rawData[x, y]);
			};

			AddTiles(ArtCompiler.GetArrayFromSprite(width << 3, height << 3, getOffset, _background: true).GetEnumerator());
		}

		public void AddTiles(IEnumerator<uint> _tileData)
		{
			int x = 0, y = 0;
			do
			{
				List<uint> tileRaw = new List<uint>();

				for (int i = 0; i < 8 && _tileData.MoveNext(); ++i)
				{
					tileRaw.Add(_tileData.Current);
				}

				if (tileRaw.Count < 8)
					break;

				var tile = new Tile(8);
				tile.LoadInData(tileRaw.ToArray(), 0);

				tileset.SetTile(tile, x, y);

				tiles[x, y] = tile;

				x = (++x) % width;
				if (x == 0)
					++y;

			} while (true);
		}

		public IEnumerable<ushort> Data()
		{
			for (int y = 0; y < height; ++y)
			{
				for (int x = 0; x < width; ++x)
				{
					var tile = tiles[x, y];
					var ogTile = tileset.GetUniqueTile(tile);

					ushort value = (ushort)((paletteIdx[x, y] << 12) | (tile.GetFlipOffset(ogTile) << 10) | tileset.GetIndex(tile));

					yield return value;
				}
			}

			yield break;
		}
	}
}
