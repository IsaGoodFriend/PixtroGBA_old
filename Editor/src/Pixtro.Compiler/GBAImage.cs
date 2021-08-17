using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
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
			List<FloatColor> palette = new List<FloatColor>(_read.ColorPalette);

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

			AddTiles(FullCompiler.GetArrayFromSprite(width << 3, height << 3, _background: true).GetEnumerator());
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

	public class GBAImage
	{

		private unsafe static GBAImage FromBitmap(Bitmap map, Rectangle section)
		{
			FloatColor[,] values = new FloatColor[section.Width, section.Height];

			var data = map.LockBits(section, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			byte* ptr = (byte*)data.Scan0.ToPointer();

			for (int y = section.Left; y < section.Right; ++y)
			{
				for (int x = section.Top; x < section.Bottom; ++x)
				{
					int i = (x * 4) + (y * data.Stride);

					values[x - section.X, y - section.Y] = new FloatColor(ptr[i + 2], ptr[i + 1], ptr[i], ptr[i + 3]);
				}
			}

			map.UnlockBits(data);

			return new GBAImage(values);
		}

		public static GBAImage FromFile(string path)
		{
			Bitmap map = new Bitmap(path);

			if (map.Width % 8 != 0 || map.Height % 8 != 0)
				throw new Exception();

			return FromBitmap(map, new Rectangle(0, 0, map.Width, map.Height));
		}
		public static GBAImage[] AnimateFromFile(string path, int width, int height)
		{
			if (width % 8 != 0 || height % 8 != 0)
				throw new Exception();

			Bitmap map = new Bitmap(path);

			// Throw error if file can't be divided evenly
			if (map.Width % width != 0 || map.Width % height != 0)
				throw new Exception();

			int frameX = map.Width / width,
				frameY = map.Height / height;

			List<GBAImage> images = new List<GBAImage>();

			for (int y = 0; y < frameY; ++y)
			{
				for (int x = 0; x < frameX; ++x)
				{
					images.Add(FromBitmap(map, new Rectangle(x * width, y * height, width, height)));
				}
			}

			return images.ToArray();
		}
		public static GBAImage[] FromAsepriteProject(string path)
		{
			using (AsepriteReader reader = new AsepriteReader(path))
			{
				List<Color[]> palettes = null;
				if (reader.IndexedColors)
				{
					var colors = reader.ColorPalette;

					palettes = new List<Color[]>();

					string test = colors[0].ToString();

					int paletteCount = (colors.Length + 14) >> 4;
					
					for (int i = 0; i < paletteCount << 4; i += 16)
					{
						Color[] pal = new Color[16];

						pal[0] = Color.FromArgb(0, 0, 0, 0);
						int index;
						for (index = 1; index < 16 && (index + i) < colors.Length; ++index)
						{
							pal[index] = colors[index + i].ToGBAColor();
						}
						for (; index < 16; ++index)
						{
							pal[index] = Color.FromArgb(0, 0, 0, 0);
						}

						palettes.Add(pal);
					}
				}

				// Angry.  You didn't feed me a properly formatted image
				if (reader.Width % 8 != 0 || reader.Height % 8 != 0)
					throw new Exception();

				GBAImage[] retval = new GBAImage[reader.FrameCount];

				for (int i = 0; i < reader.FrameCount; ++i)
				{
					retval[i] = new GBAImage(reader.GetFrameValue(i, true), palettes);
				}

				return retval;
			}
		}

		public int Width { get; private set; }
		public int Height { get; private set; }

		private int[,] baseValues;
		private List<Color[]> finalPalettes;

		private bool palettesLocked;

		private GBAImage(FloatColor[,] colors, List<Color[]> exportPalettes = null)
		{
			Width = colors.GetLength(0);
			Height = colors.GetLength(1);
			baseValues = new int[Width, Height];

			List<Color?[]> palettes = new List<Color?[]>();

			if (exportPalettes != null)
			{
				foreach (var pal in exportPalettes)
				{
					palettes.Add(pal.Select(val => (Color?)val).ToArray());
				}
				palettesLocked = true;
			}

			for (int ty = 0; ty < Height; ty += 8)
			{
				for (int tx = 0; tx < Width; tx += 8)
				{
					List<Color> palette = new List<Color>();

					Color[,] rawData = new Color[8, 8];

					for (int y = 0; y < 8; ++y)
					{
						for (int x = 0; x < 8; ++x)
						{
							rawData[x, y] = colors[x + tx, y + ty].ToGBAColor();
							if (!palette.Contains(rawData[x, y]))
								palette.Add(rawData[x, y]);
						}
					}

					int paletteIndex = 0;

					foreach (var pal in palettes)
					{
						var foundPalette = pal;

						foreach (var col in palette)
						{
							if (!pal.ContainsValue(col))
							{
								foundPalette = null;
								break;
							}
						}

						if (foundPalette != null) {
							palette = new List<Color>(foundPalette.Where(value => value != null).Select(value => (Color)value));
							break;
						}
						paletteIndex += 16;
					}
					if (paletteIndex >> 4 == palettes.Count)
					{
						if (palettesLocked)
							throw new Exception();

						paletteIndex = 0;

						bool selectedPalette = false;
						foreach (var pal in palettes)
						{
							int nullCount = 0;
							// Count how many null slots are in current palette
							for (int i = 0; i < 16; ++i)
							{
								if (pal[i] == null)
									nullCount++;
							}
							// Find and count every color current palette doesn't have
							List<Color> toAdd = new List<Color>();
							foreach (var color in palette)
							{
								if (!pal.Contains(color))
								{
									nullCount--;
									toAdd.Add(color);
								}
							}

							// If there's enough null slots to add, then add them and stop checking palettes
							if (nullCount >= 0)
							{
								for (int i = 0; i < 16 && toAdd.Count > 0; ++i)
								{
									if (pal[i] == null)
									{
										pal[i] = toAdd[0];
										toAdd.RemoveAt(0);
									}
								}
								selectedPalette = true;

								palette = new List<Color>(pal.Where(value => value != null).Select(value => (Color)value));
								break;
							}

							paletteIndex += 16;
						}

						if (!selectedPalette)
						{
							List<Color?> addPal = new List<Color?>(palette.Select(value => (Color?)value));

							while (addPal.Count < 16)
								addPal.Add(null);
							palettes.Add(addPal.ToArray());
						}

					}

					for (int y = 0; y < 8; ++y)
					{
						for (int x = 0; x < 8; ++x)
						{
							baseValues[x + tx, y + ty] = palette.IndexOf(rawData[x, y]) | paletteIndex;
						}
					}
				}
			}

			finalPalettes = new List<Color[]>();
			foreach (var pal in palettes)
			{
				finalPalettes.Add(pal.Where(value => value != null).Select(val => (Color)val).ToArray());
			}

		}
	}
}
