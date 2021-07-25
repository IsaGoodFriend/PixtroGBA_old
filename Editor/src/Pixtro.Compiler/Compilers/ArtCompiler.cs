using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Pixtro.Compiler {
	public delegate uint IndexOnSprite(int x, int y);
	public static class ArtCompiler {

		public static void StartCompiling()
		{
			palettesFromSprites.Clear();
			tilesets.Clear();
			backgroundsCompiled.Clear();
		}

		private static Dictionary<string, Color[]> palettesFromSprites = new Dictionary<string, Color[]>();

		public static Dictionary<string, Tileset> ArtTilesets;

		private static Dictionary<string, BackgroundTiles> tilesets = new Dictionary<string, BackgroundTiles>();
		private static List<string> backgroundsCompiled = new List<string>();

		public static void Compile(string _path)
		{
			StartCompiling();
			string toSavePath = Path.Combine(Compiler.Settings.ProjectPath, "build\\source");

			ArtTilesets = new Dictionary<string, Tileset>();

			CompileTilesets(Path.Combine(_path, "tilesets"), new CompileToC());

#if !DEBUG
			bool needsRecompile = false;

			long editTime = File.GetLastWriteTime(toSavePath + "\\sprites.c").Ticks;
			
			string[] folders = new string[]{ "backgrounds", "palettes", "particles", "sprites", "titlecards" };

			foreach (var folder in folders) {
				if (!Directory.Exists(Path.Combine(_path, folder)))
					continue;
				foreach (var file in Directory.GetFiles(Path.Combine(_path, folder), "*", SearchOption.AllDirectories))
				{
					if (File.GetLastWriteTime(file).Ticks > editTime)
					{
						needsRecompile = true;
						break;
					}
				}
				if (needsRecompile)
					break;
			}
			if (!needsRecompile)
			{
				Compiler.Log("Skipping art compiling");
				return;
			}
#endif
			Compiler.Log("Compiling art assets");

			tilesets = new Dictionary<string, BackgroundTiles>();
			backgroundsCompiled = new List<string>();

			var compiler = new CompileToC();

			Compiler.DebugLog("Compiling sprites");
			CompileSprites(Path.Combine(_path, "sprites"), compiler);

			Compiler.DebugLog("Compiling palettes");
			CompilePalettes(Path.Combine(_path, "palettes"), compiler);

			Compiler.DebugLog("Compiling particles");
			compiler.options |= CompileToC.CompileOptions.CompileEmptyArrays;
			CompileParticles(Path.Combine(_path, "particles"), compiler);
			compiler.options &= ~CompileToC.CompileOptions.CompileEmptyArrays;

			Compiler.DebugLog("Compiling backgrounds");
			CompileBackgrounds(Path.Combine(_path, "backgrounds"), compiler);

			Compiler.DebugLog("Compiling title cards");
			CompileTitleCards(Path.Combine(_path, "titlecards"), compiler);

			Compiler.DebugLog("Saving art to file");
			compiler.SaveTo(toSavePath, "sprites");
		}

		private static void CompileTilesets(string _path, CompileToC _compiler) {
			string[] getFiles = Directory.GetFiles(_path);

			foreach (string s in getFiles) {
				string ext = Path.GetExtension(s);

				string name = "TILE_" + Path.GetFileNameWithoutExtension(s);

				switch (ext) {
					case ".bmp": {
						Bitmap map = new Bitmap(s);

						List<Color> palette = new List<Color>(map.Palette.Entries);

						palettesFromSprites.Add(name, palette.ToArray());

						Tileset tileset = new Tileset(map.Width >> (Compiler.Settings.LargeTiles ? 4 : 3), map.Height >> (Compiler.Settings.LargeTiles ? 4 : 3));


						uint getValueSmall (int x, int y) {
							return (uint)palette.IndexOf(map.GetPixel(x, y));
						};

						tileset.AddTiles(
							GetArrayFromSprite(map.Width, map.Height, getValueSmall, Compiler.Settings.LargeTiles).GetEnumerator(),
							Compiler.Settings.LargeTiles);

						ArtTilesets.Add(name, tileset);

						_compiler.AddValueDefine(name + "_len", tileset.tiles.Count << (Compiler.Settings.LargeTiles ? 2 : 0));

						_compiler.BeginArray(CompileToC.ArrayType.UInt, name);
						_compiler.AddRange(Enumerable.ToArray(tileset.Data("asdf")));
						_compiler.EndArray();

						map.Dispose();

						break;
					}
					case ".ase":

						using (AsepriteReader read = new AsepriteReader(s)) {

							int index = 0;

							foreach (var array in read.GetSprites(_largeTiles: Compiler.Settings.LargeTiles)) {
								string tName =  $"{name}_{index++}";

								var tileset = new Tileset(read.Width >> 3, read.Height >> 3);
								tileset.AddTiles(array.Cast<uint>().GetEnumerator(), Compiler.Settings.LargeTiles);

								_compiler.BeginArray(CompileToC.ArrayType.UInt, tName);
								_compiler.AddRange(array);
								_compiler.EndArray();
							}
						}
						break;
				}


			}

		}
		private static void CompileSprites(string _path, CompileToC _compiler) {
			string[] getFiles = Directory.GetFiles(_path);

			foreach (string s in getFiles) {
				string ext = Path.GetExtension(s);

				string name = "SPR_" + Path.GetFileNameWithoutExtension(s);

				switch (ext) {
					case ".bmp": {
						Bitmap map = new Bitmap(s);

						List<Color> palette = new List<Color>(map.Palette.Entries);

						palettesFromSprites.Add(name, palette.ToArray());

						_compiler.BeginArray(CompileToC.ArrayType.UInt, name);


						_compiler.AddRange(Enumerable.ToArray(GetArrayFromSprite(map.Width, map.Height,
							(x, y) => { return (uint)palette.IndexOf(map.GetPixel(x, y)); })));

						_compiler.EndArray();

						map.Dispose();

						break;
					}
					case ".ase":
						bool separateTags = true;
						using (AsepriteReader read = new AsepriteReader(s)) {

							if (separateTags && read.TagNames.Length > 0) {
								foreach (var tag in read.Tags) {
									_compiler.BeginArray(CompileToC.ArrayType.UInt, $"{name}_{tag.name}");

									foreach (var array in read.GetSprites(tag.name)) {
										_compiler.AddRange(array);
									}

									_compiler.AddValueDefine($"{name}_{tag.name}_len", tag.end - tag.start + 1);
									_compiler.EndArray();
								}
							}
							else {
								_compiler.BeginArray(CompileToC.ArrayType.UInt, $"{name}");

								foreach (var array in read.GetSprites()) {
									_compiler.AddRange(array);
								}

								foreach (var tag in read.Tags) {
									_compiler.AddValueDefine($"{name}_{tag.name}_size", tag.start * ((read.Width * read.Height) >> 2));
									_compiler.AddValueDefine($"{name}_{tag.name}_len", tag.end - tag.start + 1);
								}

								_compiler.EndArray();

							}
						}
						break;
				}


			}

		}
		private static void CompilePalettes(string _path, CompileToC _compiler) {
			string[] getFiles = Directory.GetFiles(_path);

			List<string> addedIn = new List<string>();

			foreach (string s in getFiles) {
				string ext = Path.GetExtension(s);

				if (ext != ".bmp" && ext != ".pal") {
					continue;
				}

				string name = "PAL_" + Path.GetFileNameWithoutExtension(s);

				if (addedIn.Contains(name))
					continue;

				addedIn.Add(name);

				switch (ext) {
					case ".bmp": {
						Bitmap map = new Bitmap(s);

						List<Color> palette = new List<Color>(map.Palette.Entries);

						for (int i = 0; i < map.Height; ++i) {
							_compiler.BeginArray(CompileToC.ArrayType.UShort, name + (map.Height == 1 ? "" : "_" + i.ToString()));

							for (int j = 0; j < 16; ++j)
								_compiler.AddValue(map.GetPixel(j, i).ToGBA(0));


							_compiler.EndArray();
						}

						map.Dispose();

						break;
					}
					case ".pal":
						using (var sr = new StreamReader(File.Open(s, FileMode.Open))) {
							sr.ReadLine();
							sr.ReadLine();

							int count = int.Parse(sr.ReadLine());

							for (int i = 0; i < count / 16; ++i) {
								_compiler.BeginArray(CompileToC.ArrayType.UShort, name + (count == 16 ? "" : "_" + i.ToString()));

								for (int j = 0; j < 16; ++j) {
									string[] read = sr.ReadLine().Split(' ');

									byte r = (byte)((int.Parse(read[0]) & 0xF8) >> 3);
									byte g = (byte)((int.Parse(read[1]) & 0xF8) >> 3);
									byte b = (byte)((int.Parse(read[2]) & 0xF8) >> 3);

									_compiler.AddValue((ushort)(r | (g << 5) | (b << 10)));
								}

								_compiler.EndArray();
							}
						}
						break;
				}
			}
		}
		private static void CompileParticles(string _path, CompileToC _compiler) {
			string[] getFiles = Directory.GetFiles(_path);

			int index = 0;

			void add_particle(string name, int length) {
				length = Math.Min(length, 16) - 1;

				_compiler.AddValueDefine($"PART_{name}", index | (length << 12));

				index += length;
			}

			_compiler.BeginArray(CompileToC.ArrayType.UInt, "particles");

			foreach (string s in getFiles) {
				string ext = Path.GetExtension(s);

				string name = Path.GetFileNameWithoutExtension(s);


				switch (ext) {
					case ".bmp": {
						Bitmap map = new Bitmap(s);

						List<Color> palette = new List<Color>(map.Palette.Entries);

						uint getIdx(int x, int y) {
							x = (x & 7) | (map.Width  - (x & ~0x7) - 8);
							y = (y & 7) | (map.Height - (y & ~0x7) - 8);

							return (uint)palette.IndexOf(map.GetPixel(x, y));
						}

						_compiler.AddRange(Enumerable.ToArray(GetArrayFromSprite(map.Width, map.Height, getIdx)));


						add_particle(name, (map.Width * map.Height) >> 6);
							

						map.Dispose();

						break;
					}
					case ".ase":
						using (AsepriteReader read = new AsepriteReader(s)) {
							foreach (var tag in read.Tags) {
								foreach (var array in read.GetSprites(tag.name, _readFramesBackwards: true)) {
									_compiler.AddRange(array);
								}

								add_particle(tag.name, tag.end - tag.start + 1);
							}

							break;
						}
				}

			}

			_compiler.EndArray();

		}
		private static void CompileBackgrounds(string _path, CompileToC _compiler) {
			string[] getFiles = Directory.GetDirectories(_path);

			foreach (string s in getFiles) {
				CompileBackground(s, _compiler, true);
			}

			List<BackgroundTiles> unique = new List<BackgroundTiles>();

			foreach (var str in tilesets.Keys) {
				if (unique.Contains(tilesets[str]))
					continue;

				unique.Add(tilesets[str]);

				var array = tilesets[str].Data(str).ToArray();

				_compiler.AddValueDefine(str + "_len", array.Length >> 3);

				_compiler.BeginArray(CompileToC.ArrayType.UInt, str);
				_compiler.AddRange(array);
				_compiler.EndArray();
			}
		}
		private static void CompileBackground(string _path, CompileToC _compiler, bool _saveTiles) {
			string name = Path.GetFileName(_path);

			if (backgroundsCompiled.Contains(name)) // prevent backgrounds from being recompiled
				return;

			string dataPath = null;
			string ext = null;

			foreach (var s in Directory.GetFiles(_path)) {
				ext = Path.GetExtension(s);
				if (ext == ".bmp" || ext == ".ase") {
					dataPath = s;
					break;
				}
			}

			if (dataPath == null)
				return;

			string otherTileset = null;

			BackgroundTiles tiles = null;
			if (otherTileset == null && tilesets.ContainsKey(name))
				otherTileset = name;

			if (otherTileset != null) {
				if (otherTileset != name)
					CompileBackground(Path.Combine(_path, otherTileset), _compiler, false);

				tiles = tilesets[otherTileset];
			}

			switch (ext) {
				case ".bmp": {
					// Only mark a background 
					if (_saveTiles)
						backgroundsCompiled.Add(name);

					Bitmap map = new Bitmap(dataPath);
					if (map.Width % 256 != 0 || map.Height % 256 != 0){
						map.Dispose();
						break;
					}

					Background bg = new Background(map, tiles);

					string tileName = $"BGT_{name}";

					if (otherTileset != null && otherTileset != name) {
						_compiler.AddValueDefine(tileName, otherTileset);
					}

					tilesets.Add(tileName, bg.tileset);

					if (_saveTiles) {
						_compiler.BeginArray(CompileToC.ArrayType.UShort, "BG_" + name);

						_compiler.AddRange(Enumerable.ToArray(bg.Data()));

						_compiler.EndArray();

						_compiler.AddValueDefine($"BG_{name}_size", ((map.Width * map.Height) >> 16) - 1);
					}

					map.Dispose();

					break;
				}
				case ".ase":
					using (AsepriteReader read = new AsepriteReader(dataPath)) {
						if (read.Width % 256 != 0 || read.Height % 256 != 0)
							break;

						// Only mark a background 
						if (_saveTiles)
							backgroundsCompiled.Add(name);

						List<FloatColor> palette = new List<FloatColor>(read.Colors);

						if (otherTileset == null) {
							otherTileset = $"BGT_{name}";

							if (read.LayerNames.Length > 1)
								otherTileset += "_" + read.LayerNames[0];
						}

						BackgroundTiles tileset = null;

						foreach (var layer in read.LayerNames) {
							string tileName = $"BGT_{name}";

							if (read.LayerNames.Length > 1)
								tileName += "_" + layer.Replace(" ", "");

							Background bg = new Background(read, tiles, layer);
							tileset = bg.tileset;

							if (otherTileset != name) {
								_compiler.AddValueDefine(tileName, otherTileset);
							}

							if (_saveTiles) {
								string exName = $"BG_{name}";
								if (read.LayerNames.Length > 1)
									exName += "_" + layer.Replace(" ", "");

								_compiler.BeginArray(CompileToC.ArrayType.UShort, exName);

								_compiler.AddRange(Enumerable.ToArray(bg.Data()));

								_compiler.AddValueDefine($"BG_{name}_size", ((read.Width * read.Height) >> 16) - 1);

								_compiler.EndArray();
							}
						}

						tilesets.Add(otherTileset, tileset);

						break;

					}
			}
		}

		private static void CompileTitleCards(string _path, CompileToC _compiler) {
			if (!Directory.Exists(_path))
				return;
			
			// Get all the cards that the user wants to use, and compile only those.
			string[] allCards = File.ReadAllLines(Path.Combine(_path, "order.txt"));

			List<string> addedCards = new List<string>();

			// foreach card, search for it and compile it if it exists.
			foreach (var cardName in allCards){
				if (string.IsNullOrWhiteSpace(cardName))
					continue;
				
				string name, tag = null;
				if (cardName.Contains(":")) {
					tag = cardName.Split(':')[1];
					name = cardName.Split(':')[0];
				}
				else
					name = cardName;
				
				foreach (string s in Directory.GetFiles(_path)) {
					// Found card, now compile it and stop searching for others of the same name
					if (Path.GetFileNameWithoutExtension(s) == name){
						addedCards.Add(name);
						CompileTitleCard(s, _compiler, tag);
						break;
					}
				}
			}

			_compiler.BeginArray(CompileToC.ArrayType.UShortPtr, "INTRO_CARDS");

			foreach (var str in addedCards){
				//_compiler.AddValue($"(unsigned short*)CARD_{str}");
				//_compiler.AddValue($"(unsigned short*)CARDTILE_{str}");
			}
			//_compiler.AddValue(0);

			_compiler.EndArray();
		}
		private static void CompileTitleCard(string _path, CompileToC _compiler, string _aseLayer = null) {
			string name = Path.GetFileNameWithoutExtension(_path);

			BackgroundTiles tiles = null;
			Background bg = null;

			switch (Path.GetExtension(_path)) {
				case ".bmp": {
					if (_aseLayer != null) break;

					Bitmap map = new Bitmap(_path);
					if (map.Width != 240 || map.Height != 160){
						map.Dispose();
						break;
					}

					bg = new Background(map, tiles);

					map.Dispose();

					break;
				}
				case ".ase":
					using (AsepriteReader read = new AsepriteReader(_path)) {
						if (read.Width != 240 || read.Height != 160)
							break;

						List<FloatColor> palette = new List<FloatColor>(read.Colors);
						
						if (_aseLayer != null && read.LayerNames.Contains(_aseLayer)) {
							bg = new Background(read, tiles, _aseLayer);
						}
						else{
							bg = new Background(read, tiles, read.LayerNames[0]);
						}

						break;

					}
			}
			if (bg == null)
				return;
			
			_compiler.BeginArray(CompileToC.ArrayType.UShort, "CARD_" + name);

			_compiler.AddRange(Enumerable.ToArray(bg.Data()));

			_compiler.EndArray();

			_compiler.BeginArray(CompileToC.ArrayType.UShort, "CARDTILE_" + name);

			_compiler.AddRange(Enumerable.ToArray(bg.tileset.Data(name)));

			_compiler.EndArray();
		}


		public static IEnumerable<uint> GetArrayFromSprite(int _width, int _height, IndexOnSprite _values, bool _largeTiles = false, bool _background = false) {

			if (_largeTiles) {
				for (int yL = 0; yL < _height >> 3; yL += 2) {
					for (int xL = 0; xL < _width >> 3; ++xL) {
						for (int y = 0; y < 16; ++y) {
							uint tempValue = 0;

							for (int i = 7; i >= 0; --i)
								tempValue = (tempValue << 4) | _values((xL << 3) + i, (yL << 3) + y);

							yield return tempValue;
						}
					}
				}
			}
			else if (_background) {
				for (int yW = 0; yW < _height >> 3; yW += 32) {
					for (int xW = 0; xW < _width >> 3; xW += 32) {

						for (int yL = 0; yL < 32; ++yL) {
							for (int xL = 0; xL < 32; ++xL) {
								for (int y = 0; y < 8; ++y) {
									uint tempValue = 0;

									for (int i = 7; i >= 0; --i)
										tempValue = (tempValue << 4) | _values(((xL + xW) << 3) + i, ((yL + yW) << 3) + y);

									yield return tempValue;
								}
							}
						}

					}
				}
			}
			else {
				for (int yL = 0; yL < _height >> 3; ++yL) {
					for (int xL = 0; xL < _width >> 3; ++xL) {
						for (int y = 0; y < 8; ++y) {
							uint tempValue = 0;

							for (int i = 7; i >= 0; --i)
								tempValue = (tempValue << 4) | _values((xL << 3) + i, (yL << 3) + y);

							yield return tempValue;
						}
					}
				}
			}

		}
	}
}