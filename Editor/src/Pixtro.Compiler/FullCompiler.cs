using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Pixtro.Compiler
{
	internal static class FullCompiler
	{
		static void ClearDictionaries()
		{
			ArtTilesets.Clear();
			palettesFromSprites.Clear();
			tilesets.Clear();
			backgroundsCompiled.Clear();
			compiledLevels.Clear();
			levelPacks.Clear();
			usedLevels.Clear();

			entLocalCount = 0;
			entGlobalCount = 0;
			entSectionCount = 0;
			typeLocalCount.Clear();
			typeGlobalCount.Clear();
			typeSectionCount.Clear();
		}

		private const string
							 ArtPath = "art",
							 LevelPath = "levels",
							 BackgroundPath = ArtPath + "/backgrounds",
							 ParticlePath = ArtPath + "/particles",
							 SpritePath = ArtPath + "/sprites",
							 TilesetPath = ArtPath + "/tilesets",
							 TitleCardPath = ArtPath + "/titlecards",
							 LevelPackPath = LevelPath + "/_packs",
							 BuildToPath = "build/source";

		private static Dictionary<string, Color[]> palettesFromSprites = new Dictionary<string, Color[]>();

		public static Dictionary<string, Tileset> ArtTilesets = new Dictionary<string, Tileset>();

		private static Dictionary<string, GBAImage[]> CompiledImages = new Dictionary<string, GBAImage[]>();

		private static Dictionary<string, BackgroundTiles> tilesets = new Dictionary<string, BackgroundTiles>();
		private static List<string> backgroundsCompiled = new List<string>();

		static Dictionary<string, CompressedLevel> compiledLevels = new Dictionary<string, CompressedLevel>();
		static Dictionary<string, List<string>> levelPacks = new Dictionary<string, List<string>>();
		static List<string> usedLevels = new List<string>();

		private static string currentType;

		private static int entGlobalCount, entSectionCount, entLocalCount;

		private static Dictionary<string, int> 
			typeGlobalCount = new Dictionary<string, int>(),
			typeSectionCount = new Dictionary<string, int>(),
			typeLocalCount = new Dictionary<string, int>();


		public static void Compile()
		{
			ClearDictionaries();

			void AddFile(string file)
			{
				string ext = Path.GetExtension(file);

				string localPath = file.Replace(Path.Combine(Settings.ProjectPath, ArtPath) + "/", "").Replace('\\', '/');
				string name = Path.ChangeExtension(localPath, "").Replace('/', '_').Replace(".", "");

				try
				{
					switch (ext)
					{
						case ".bmp":
							throw new Exception(); // BMP no longer supported
						case ".ase":
							CompiledImages.Add(name, GBAImage.FromAsepriteProject(file));
							break;
						case ".png":
							CompiledImages.Add(name, new GBAImage[] { GBAImage.FromFile(file) });
							break;
					}
				}
				catch (Exception e)
				{
					Compiler.ErrorLog(e);
				}
			}

			foreach (var file in Directory.GetFiles(Path.Combine(Settings.ProjectPath, BackgroundPath), "*", SearchOption.AllDirectories))
				AddFile(file);
			foreach (var file in Directory.GetFiles(Path.Combine(Settings.ProjectPath, ParticlePath), "*", SearchOption.AllDirectories))
				AddFile(file);
			foreach (var file in Directory.GetFiles(Path.Combine(Settings.ProjectPath, SpritePath), "*", SearchOption.AllDirectories))
				AddFile(file);
			foreach (var file in Directory.GetFiles(Path.Combine(Settings.ProjectPath, TilesetPath), "*", SearchOption.AllDirectories))
				AddFile(file);
			foreach (var file in Directory.GetFiles(Path.Combine(Settings.ProjectPath, TitleCardPath), "*", SearchOption.AllDirectories))
				AddFile(file);


			string levelPath = Path.Combine(Settings.ProjectPath, LevelPath),
			tilesetPath =  Path.Combine(Settings.ProjectPath, TilesetPath);
			
			string toSavePath = Path.Combine(Settings.ProjectPath, BuildToPath);

			Compiler.Log("Compiling levels");
			var levelCompiler = new CompileToC();
			var artCompiler = new CompileToC();

			Dictionary<string, VisualPackMetadata> parseData = JsonConvert.DeserializeObject<Dictionary<string, VisualPackMetadata>>(File.ReadAllText(levelPath + "\\meta_level.json"));

			// Get all the art from the tilesets and compile them into C# code for ease of access
			CompileTilesets(tilesetPath);

			// Finalize tile mapping
			foreach (var p in parseData)
			{
				p.Value.Name = p.Key;

				foreach (char c in p.Value.Wrapping.Keys)
				{
					var wrap = p.Value.Wrapping[c];

					if (wrap.MappingCopy != null)
					{
						string[] split = wrap.MappingCopy.Split('/', '\\');

						var otherWrap = parseData[split[0]].Wrapping[split[1][0]];

						wrap.Mapping = otherWrap.Mapping;
						wrap.TileMapping = otherWrap.TileMapping;
					}

					wrap.FinalizeMasks();
				}
			}

			// Todo: allow custom seeds to get consistent result
			CompressedLevel.Randomizer = new Random();

			// Go through each level pack and figure out which levels are used and where
			foreach (var pack in Directory.GetFiles(Path.Combine(Settings.ProjectPath, LevelPackPath)))
			{
				string name = Path.GetFileNameWithoutExtension(pack);

				List<string> levelList = new List<string>();

				foreach (var level in File.ReadAllLines(pack))
				{
					if (string.IsNullOrWhiteSpace(level))
						continue;

					levelList.Add(level);
				}

				for (int i = levelList.Count - 1; i >= 0; --i)
				{
					string lName = levelList[i];
					if (usedLevels.Contains(lName))
						levelList.RemoveAt(i);
					else
						usedLevels.Add(lName);
				}
				levelPacks.Add(name, levelList);

				foreach (var p in parseData.Values)
				{
					if (p.LevelPacks.Contains(name))
					{
						p.levelsIncluded.AddRange(levelList);
					}
				}
			}


			// Compile levels
			// Foreach Visual Pack
			foreach (var parse in parseData.Values)
			{
				typeSectionCount.Clear();
				entSectionCount = 0;

				CompressedLevel.DataParse = parse;

				foreach (var level in parse.levelsIncluded)
				{
					var localPath = Path.Combine(Settings.ProjectPath, LevelPath, level);
					var ext = "";

					if (File.Exists(localPath + ".bin"))
						ext = ".bin";
					else if (File.Exists(localPath + ".json"))
						ext = ".json";
					else if (File.Exists(localPath + ".txt"))
						ext = ".txt";

					// Troubled.  Level doesn't exist with the accepted extensions
					if (ext == "")
						throw new Exception();

					localPath = localPath.Replace('\\', '/') + ext;

					CompressedLevel compressed = null;

					entLocalCount = 0;
					typeLocalCount.Clear();

					CompressedLevel.RNGSeed = (uint)new Random(localPath.GetHashCode()).Next(0x800, 0xFFFFFF);


					switch (ext)
					{
						case ".txt":
							compressed = CompileLevelTxt(level + ".txt");
							break;
						case ".json":
							throw new NotImplementedException();
						default: // Compressed Binary File
							CompileLevelBin(level, levelCompiler);
							break;
					}

					// Troubled.  Unable to compile level
					if (compressed == null)
						throw new Exception();

					localPath = $"LVL_{Path.GetFileNameWithoutExtension(level.Replace('/', '_').Replace('\\', '_'))}";

					compiledLevels.Add(localPath, compressed);

					levelCompiler.BeginArray(CompileToC.ArrayType.Char, localPath);
					levelCompiler.AddRange(compressed.BinaryData());
					levelCompiler.EndArray();
				}

				if (parse.fullTileset == null)
					continue;

				int length = parse.fullTileset.RawTiles.Count;

				levelCompiler.BeginArray(CompileToC.ArrayType.UInt, "TILESET_" + parse.Name);

				List<Tile> rawTiles = new List<Tile>(parse.fullTileset.RawTiles);

				foreach (var tile in rawTiles)
				{
					levelCompiler.AddRange(tile.RawData);
				}

				levelCompiler.EndArray();

				levelCompiler.BeginArray(CompileToC.ArrayType.UInt, "TILECOLL_" + parse.Name);

				foreach (var tile in parse.fullTileset)
				{
					levelCompiler.AddValue((tile.collisionType << 8) | tile.collisionShape);
				}
				levelCompiler.AddValue(0xFFFF);

				levelCompiler.EndArray();

				List<Tile> tileset = new List<Tile>(parse.fullTileset.RawTiles);

				levelCompiler.BeginArray(CompileToC.ArrayType.UShort, "TILE_MAPPING_" + parse.Name);

				int size = parse.fullTileset.First().sizeOfTile / 8;

				foreach (var tile in parse.fullTileset)
				{
					for (int i = 0; i < size * size; ++i)
					{
						var mappedTile = new Tile(8);
						mappedTile.LoadInData(tile.RawData, i * 8);

						Tile rawTile = null;

						foreach (var rt in rawTiles)
						{
							if (mappedTile.EqualTo(rt, Tile.FlipStyle.Both))
							{
								rawTile = rt;
								break;
							}
						}

						ushort value = (ushort)(rawTiles.IndexOf(rawTile, new Tileset.CompareTiles()) + 1);
						if (rawTile != null)
							value |= (ushort)(mappedTile.GetFlipOffset(rawTile) << 10);
						levelCompiler.AddValue(value);
					}
				}
				levelCompiler.EndArray();

				levelCompiler.AddValueDefine($"TILESET_{parse.Name}_len", length);
			}

			// Compile Level Packs
			foreach (var pack in levelPacks)
			{
				string name = pack.Key;

				levelCompiler.BeginArray(CompileToC.ArrayType.UInt, "PACK_" + name);

				List<string> levelList = new List<string>();

				foreach (var level in pack.Value)
				{
					levelList.Add("LVL_" + level);
				}

				for (int i = 0; i < levelList.Count; ++i)
				{
					if (i != 0)
					{
						levelCompiler.AddValue(1);
					}
					levelCompiler.AddValue("&" + levelList[i]);

					CompressedLevel level = compiledLevels[levelList[i]];

					for (int j = 0; j < level.Layers; ++j)
					{
						levelCompiler.AddValue((2) | (j << 4));
					}
					levelCompiler.AddValue(3);
				}

				levelCompiler.AddValue(0);

				levelCompiler.EndArray();
			}

			levelCompiler.SaveTo(toSavePath, "levels");

			string artPath = Path.Combine(Settings.ProjectPath, ArtPath);

#if !DEBUG
			bool needsRecompile = false;

			long editTime = File.GetLastWriteTime(toSavePath + "\\sprites.c").Ticks;

			string[] folders = new string[]{ "backgrounds", "palettes", "particles", "sprites", "titlecards" };

			foreach (var folder in folders)
			{
				if (!Directory.Exists(Path.Combine(artPath, folder)))
					continue;
				foreach (var file in Directory.GetFiles(Path.Combine(artPath, folder), "*", SearchOption.AllDirectories))
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

			Compiler.DebugLog("Compiling sprites");
			CompileSprites(Path.Combine(artPath, "sprites"), artCompiler);

			Compiler.DebugLog("Compiling palettes");
			CompilePalettes(Path.Combine(artPath, "palettes"), artCompiler);

			Compiler.DebugLog("Compiling particles");
			artCompiler.options |= CompileToC.CompileOptions.CompileEmptyArrays;
			CompileParticles(Path.Combine(artPath, "particles"), artCompiler);
			artCompiler.options &= ~CompileToC.CompileOptions.CompileEmptyArrays;

			Compiler.DebugLog("Compiling backgrounds");
			CompileBackgrounds(Path.Combine(artPath, "backgrounds"), artCompiler);

			Compiler.DebugLog("Compiling title cards");
			CompileTitleCards(Path.Combine(artPath, "titlecards"), artCompiler);

			Compiler.DebugLog("Saving art to file");
			artCompiler.SaveTo(toSavePath, "sprites");
		}

		private static string NextLine(StreamReader _reader, bool ignoreWhitespace = true)
		{

			string retval;

			if (ignoreWhitespace)
			{
				do
					retval = _reader.ReadLine();
				while (string.IsNullOrWhiteSpace(retval));
			}
			else
			{
				do
					retval = _reader.ReadLine();
				while (string.IsNullOrEmpty(retval));
			}

			return retval;
		}
		private static string[] SplitWithTrim(string str, char splitChar)
		{
			string[] split = str.Split(new char[]{ splitChar }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < split.Length; ++i)
			{
				split[i] = split[i].Trim();
			}

			return split;
		}

		private static byte ParseMetadata(string algorithm)
		{
			byte retval;

			if (byte.TryParse(algorithm, out retval))
				return retval;

			double getvals(string[] args)
			{
				switch (args[0].ToLower())
				{
					case "entglobalcount":
						return entGlobalCount;
					case "entlocalcount":
						return entLocalCount;
					case "entsectioncount":
						return entSectionCount;

					case "typeglobalcount":
						return typeGlobalCount[currentType];
					case "typelocalcount":
						return typeLocalCount[currentType];
					case "typesectioncount":
						return typeSectionCount[currentType];

					case "packsize":
						return levelPacks[args[1]].Count;

					case "levelindex":
					{
						string pack;

						if (args.Length < 3)
						{
							pack = "NULL";
						}
						else
						{
							pack = args[2];
						}

						// TODO: auto detect level pack if not given a third argument
						return levelPacks[pack].IndexOf(args[1]);
					}
				}

				return 0;
			}

			return DataParser.EvaluateByte(algorithm, getvals);
		}
		private static CompressedLevel CompileLevelBin(string _path, CompileToC _compiler)
		{

			// TODO: Add support for this at some point.

			var reader = new BinaryFileParser(_path, "PIXTRO_LVL");

			string baseName = Path.GetFileNameWithoutExtension(_path);

			foreach (var node in reader.Nodes)
			{
				switch (node.Name)
				{
					case "level":
						if (node.Children[0].Name != "meta")
							continue;

						CompressedLevel level = new CompressedLevel();
						string levelName = null;

						foreach (var child in node.Children)
						{
							switch (child.Name)
							{
								case "meta":
									level.Width = child.GetInteger("width");
									level.Height = child.GetInteger("height");
									level.Layers = child.GetInteger("layers");
									levelName = child.GetString("name");
									break;
								case "layer":
									int layerIndex = child.GetInteger("index");
									string[] values = (child.GetString("data")).Split('\n');

									for (int i = 0; i < values.Length; ++i)
									{
										level.AddLine(layerIndex, i, values[i]);
									}
									break;
								case "entity":

									var ent = new CompressedLevel.Entity();

									ent.x = child.GetInteger("x");
									ent.y = child.GetInteger("y");
									if (child.Attributes["type"] is string)
									{
										ent.type = CompressedLevel.DataParse.EntityIndex[child.Attributes["type"] as string];
									}
									else
									{
										ent.type = (byte)child.GetInteger("type");
									}

									level.entities.Add(ent);

									foreach (var attr in child.Attributes.Keys)
									{
										switch (attr)
										{
											case "x":
											case "y":
											case "name":
											case "type":
												break;
											default:
												if (child.Attributes[attr] is string)
												{
													ent.data.Add(ParseMetadata(child.Attributes[attr] as string));
												}
												else
												{
													ent.data.Add((byte)child.GetInteger(attr));
												}

												break;
										}

									}

									entLocalCount++;
									entGlobalCount++;
									entSectionCount++;

									currentType = child.GetString("name");

									if (!typeLocalCount.ContainsKey(currentType))
									{
										typeLocalCount.Add(currentType, 0);
										typeGlobalCount.Add(currentType, 0);
										typeSectionCount.Add(currentType, 0);
									}
									typeLocalCount[currentType]++;
									typeGlobalCount[currentType]++;
									typeSectionCount[currentType]++;


									break;
							}
						}

						_compiler.BeginArray(CompileToC.ArrayType.Char, $"LVL_{baseName}_{levelName}");

						_compiler.AddRange(level.BinaryData());

						_compiler.EndArray();

						break;

					case "meta":
						break;
				}
			}

			return null;
		}
		private static CompressedLevel CompileLevelTxt(string levelLocalPath)
		{
			levelLocalPath = Path.Combine(Settings.ProjectPath, LevelPath, levelLocalPath);

			CompressedLevel retval = new CompressedLevel();

			using (StreamReader reader = new StreamReader(File.Open(levelLocalPath, FileMode.Open)))
			{
				string[] split = SplitWithTrim(NextLine(reader), '-');

				retval.Width = int.Parse(split[0]);
				retval.Height = int.Parse(split[1]);
				retval.Layers = int.Parse(split[2]);

				while (!reader.EndOfStream)
				{
					string dataType = NextLine(reader);
					split = SplitWithTrim(dataType, '-');

					switch (split[0])
					{
						case "layer":
						{
							int layer = dataType.Contains('-') ? int.Parse(split[1]) : 0;

							for (int i = 0; i < retval.Height; ++i)
							{
								retval.AddLine(layer, i, NextLine(reader, false));
							}
							break;
						}
						case "entities":
							string ent = "";

							while (ent != "end")
							{
								ent = NextLine(reader);

								if (ent == "end")
									break;

								split = SplitWithTrim(ent, ';');

								var entity = new CompressedLevel.Entity();

								entity.x = int.Parse(split[1]);
								entity.y = int.Parse(split[2]);

								for (int i = 3; i < split.Length; ++i)
								{
									entity.data.Add(ParseMetadata(split[i]));
								}

								byte type;
								if (!byte.TryParse(split[0], out type))
								{
									entity.type = CompressedLevel.DataParse.EntityIndex[split[0]];
								}

								retval.entities.Add(entity);

								entLocalCount++;
								entGlobalCount++;
								entSectionCount++;

								currentType = split[0];

								if (!typeLocalCount.ContainsKey(currentType))
									typeLocalCount.Add(currentType, 0);
								if (!typeGlobalCount.ContainsKey(currentType))
									typeGlobalCount.Add(currentType, 0);
								if (!typeSectionCount.ContainsKey(currentType))
									typeSectionCount.Add(currentType, 0);

								typeLocalCount[currentType]++;
								typeGlobalCount[currentType]++;
								typeSectionCount[currentType]++;


							}
							break;
						case "meta":
						case "metadata":
						{
							retval.metadata =new Dictionary<byte, byte>();

							string readLine = "";

							while (readLine != "end")
							{
								readLine = NextLine(reader);

								if (readLine == "end")
									break;

								split = readLine.Split(';');

								byte value = ParseMetadata(split[1]);
								retval.metadata.Add(byte.Parse(split[0]), value);

							}
						}
						break;
					}

				}
			}

			return retval;
		}

		/// <summary>
		/// Get all the art from the tilesets and compile them into C# code for ease of access
		/// </summary>
		/// <param name="_path"></param>
		private static void CompileTilesets(string _path)
		{
			string[] getFiles = Directory.GetFiles(_path);

			foreach (string s in getFiles)
			{
				string ext = Path.GetExtension(s);

				string name = "TILE_" + Path.GetFileNameWithoutExtension(s);

				switch (ext)
				{
					case ".bmp":
						throw new Exception(); // BMP no longer supported
					case ".png":
					{
						Bitmap map = new Bitmap(s);

						List<Color> palette = new List<Color>(map.Palette.Entries);

						palettesFromSprites.Add(name, palette.ToArray());

						Tileset tileset = new Tileset(map.Width >> (3 + Settings.BrickTileSize), map.Height >> (3 + Settings.BrickTileSize));


						uint getValueSmall(int x, int y)
						{
							string n = name;
							return (uint)palette.IndexOf(map.GetPixel(x, y));
						};

						// Todo: Move to a new system
						tileset.AddTiles(
							GetArrayFromSprite(map.Width, map.Height, Settings.BrickTileSize > 1).GetEnumerator());

						ArtTilesets.Add(name, tileset);

						map.Dispose();

						break;
					}
					case ".ase":

						using (AsepriteReader read = new AsepriteReader(s))
						{

							int index = 0;

							//foreach (var array in read.GetSprites(_largeTiles: Settings.BrickTileSize > 1))
							//{
							//	string tName =  $"{name}_{index++}";

							//	var tileset = new Tileset(read.Width >> 3, read.Height >> 3);
							//	tileset.AddTiles(array.Cast<uint>().GetEnumerator());

							//}
						}
						break;
				}


			}
		}
		private static void CompileSprites(string _path, CompileToC _compiler)
		{
			string[] getFiles = Directory.GetFiles(_path);

			foreach (string s in getFiles)
			{
				string ext = Path.GetExtension(s);

				string name = "SPR_" + Path.GetFileNameWithoutExtension(s);

				switch (ext)
				{
					case ".bmp":
						throw new Exception(); // BMP no longer supported
					case ".png":
						{
						Bitmap map = new Bitmap(s);

						List<Color> palette = new List<Color>(map.Palette.Entries);

						palettesFromSprites.Add(name, palette.ToArray());

						_compiler.BeginArray(CompileToC.ArrayType.UInt, name);


						_compiler.AddRange(Enumerable.ToArray(GetArrayFromSprite(map.Width, map.Height)));

						_compiler.EndArray();

						map.Dispose();

						break;
					}
					case ".ase":
						bool separateTags = true;
						using (AsepriteReader read = new AsepriteReader(s))
						{

							if (separateTags && read.TagNames.Length > 0)
							{
								foreach (var tag in read.Tags)
								{
									_compiler.BeginArray(CompileToC.ArrayType.UInt, $"{name}_{tag.name}");

									//foreach (var array in read.GetSprites(tag.name))
									//{
									//	_compiler.AddRange(array);
									//}

									_compiler.AddValueDefine($"{name}_{tag.name}_len", tag.end - tag.start + 1);
									_compiler.EndArray();
								}
							}
							else
							{
								_compiler.BeginArray(CompileToC.ArrayType.UInt, $"{name}");

								//foreach (var array in read.GetSprites())
								//{
								//	_compiler.AddRange(array);
								//}

								foreach (var tag in read.Tags)
								{
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
		private static void CompilePalettes(string _path, CompileToC _compiler)
		{
			string[] getFiles = Directory.GetFiles(_path);

			List<string> addedIn = new List<string>();

			foreach (string s in getFiles)
			{
				string ext = Path.GetExtension(s);

				string name = "PAL_" + Path.GetFileNameWithoutExtension(s);

				if (addedIn.Contains(name))
					continue;

				addedIn.Add(name);

				switch (ext)
				{
					case ".bmp": // Palettes are okay with .bmp
					case ".png":
					{
						Bitmap map = new Bitmap(s);

						List<Color> palette = new List<Color>(map.Palette.Entries);

						for (int i = 0; i < map.Height; ++i)
						{
							_compiler.BeginArray(CompileToC.ArrayType.UShort, name + (map.Height == 1 ? "" : "_" + i.ToString()));

							for (int j = 0; j < 16; ++j)
								_compiler.AddValue(map.GetPixel(j, i).ToGBA(0));

							_compiler.EndArray();
						}

						map.Dispose();

						break;
					}
					case ".pal":
						using (var sr = new StreamReader(File.Open(s, FileMode.Open)))
						{
							sr.ReadLine();
							sr.ReadLine();

							int count = int.Parse(sr.ReadLine());

							for (int i = 0; i < count / 16; ++i)
							{
								_compiler.BeginArray(CompileToC.ArrayType.UShort, name + (count == 16 ? "" : "_" + i.ToString()));

								for (int j = 0; j < 16; ++j)
								{
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
		private static void CompileParticles(string _path, CompileToC _compiler)
		{
			string[] getFiles = Directory.GetFiles(_path);

			int index = 0;

			void add_particle(string name, int length)
			{
				length = Math.Min(length, 16) - 1;

				_compiler.AddValueDefine($"PART_{name}", index | (length << 12));

				index += length;
			}

			_compiler.BeginArray(CompileToC.ArrayType.UInt, "particles");

			foreach (string s in getFiles)
			{
				string ext = Path.GetExtension(s);

				string name = Path.GetFileNameWithoutExtension(s);


				switch (ext)
				{
					case ".bmp":
						throw new Exception(); // BMP no longer supported
					case ".png":
					{
						Bitmap map = new Bitmap(s);

						List<Color> palette = new List<Color>(map.Palette.Entries);

						uint getIdx(int x, int y)
						{
							x = (x & 7) | (map.Width  - (x & ~0x7) - 8);
							y = (y & 7) | (map.Height - (y & ~0x7) - 8);

							return (uint)palette.IndexOf(map.GetPixel(x, y));
						}

						_compiler.AddRange(Enumerable.ToArray(GetArrayFromSprite(map.Width, map.Height)));


						add_particle(name, (map.Width * map.Height) >> 6);


						map.Dispose();

						break;
					}
					case ".ase":
						using (AsepriteReader read = new AsepriteReader(s))
						{
							foreach (var tag in read.Tags)
							{
								//foreach (var array in read.GetSprites(tag.name, _readFramesBackwards: true))
								//{
								//	_compiler.AddRange(array);
								//}

								add_particle(tag.name, tag.end - tag.start + 1);
							}

							break;
						}
				}

			}

			_compiler.EndArray();

		}
		private static void CompileBackgrounds(string _path, CompileToC _compiler)
		{
			string[] getFiles = Directory.GetDirectories(_path);

			foreach (string s in getFiles)
			{
				CompileBackground(s, _compiler, true);
			}

			List<BackgroundTiles> unique = new List<BackgroundTiles>();

			foreach (var str in tilesets.Keys)
			{
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
		private static void CompileBackground(string _path, CompileToC _compiler, bool _saveTiles)
		{
			string name = Path.GetFileName(_path);

			if (backgroundsCompiled.Contains(name)) // prevent backgrounds from being recompiled
				return;

			string dataPath = null;
			string ext = null;

			foreach (var s in Directory.GetFiles(_path))
			{
				ext = Path.GetExtension(s);

				dataPath = s;
				break;
			}

			if (dataPath == null)
				return;

			string otherTileset = null;

			BackgroundTiles tiles = null;
			if (otherTileset == null && tilesets.ContainsKey(name))
				otherTileset = name;

			if (otherTileset != null)
			{
				if (otherTileset != name)
					CompileBackground(Path.Combine(_path, otherTileset), _compiler, false);

				tiles = tilesets[otherTileset];
			}

			switch (ext)
			{
				case ".bmp":
					throw new Exception(); // BMP no longer supported
				case ".png":
					{
					// Only mark a background 
					if (_saveTiles)
						backgroundsCompiled.Add(name);

					Bitmap map = new Bitmap(dataPath);
					if (map.Width % 256 != 0 || map.Height % 256 != 0)
					{
						map.Dispose();
						break;
					}

					Background bg = new Background(map, tiles);

					string tileName = $"BGT_{name}";

					if (otherTileset != null && otherTileset != name)
					{
						_compiler.AddValueDefine(tileName, otherTileset);
					}

					tilesets.Add(tileName, bg.tileset);

					if (_saveTiles)
					{
						_compiler.BeginArray(CompileToC.ArrayType.UShort, "BG_" + name);

						_compiler.AddRange(Enumerable.ToArray(bg.Data()));

						_compiler.EndArray();

						_compiler.AddValueDefine($"BG_{name}_size", ((map.Width * map.Height) >> 16) - 1);
					}

					map.Dispose();

					break;
				}
				case ".ase":
					using (AsepriteReader read = new AsepriteReader(dataPath))
					{
						if (read.Width % 256 != 0 || read.Height % 256 != 0)
							break;

						// Only mark a background 
						if (_saveTiles)
							backgroundsCompiled.Add(name);

						List<FloatColor> palette = new List<FloatColor>(read.ColorPalette);

						if (otherTileset == null)
						{
							otherTileset = $"BGT_{name}";

							if (read.LayerNames.Length > 1)
								otherTileset += "_" + read.LayerNames[0];
						}

						BackgroundTiles tileset = null;

						foreach (var layer in read.LayerNames)
						{
							string tileName = $"BGT_{name}";

							if (read.LayerNames.Length > 1)
								tileName += "_" + layer.Replace(" ", "");

							Background bg = new Background(read, tiles, layer);
							tileset = bg.tileset;

							if (otherTileset != name)
							{
								_compiler.AddValueDefine(tileName, otherTileset);
							}

							if (_saveTiles)
							{
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

		private static void CompileTitleCards(string _path, CompileToC _compiler)
		{
			if (!Directory.Exists(_path))
				return;

			// Get all the cards that the user wants to use, and compile only those.
			string[] allCards = File.ReadAllLines(Path.Combine(_path, "order.txt"));

			List<string> addedCards = new List<string>();

			// foreach card, search for it and compile it if it exists.
			foreach (var cardName in allCards)
			{
				if (string.IsNullOrWhiteSpace(cardName))
					continue;

				string name, tag = null;
				if (cardName.Contains(":"))
				{
					tag = cardName.Split(':')[1];
					name = cardName.Split(':')[0];
				}
				else
					name = cardName;

				foreach (string s in Directory.GetFiles(_path))
				{
					// Found card, now compile it and stop searching for others of the same name
					if (Path.GetFileNameWithoutExtension(s) == name)
					{
						addedCards.Add(name);
						CompileTitleCard(s, _compiler, tag);
						break;
					}
				}
			}

			_compiler.BeginArray(CompileToC.ArrayType.UShortPtr, "INTRO_CARDS");

			foreach (var str in addedCards)
			{
				//_compiler.AddValue($"(unsigned short*)CARD_{str}");
				//_compiler.AddValue($"(unsigned short*)CARDTILE_{str}");
			}
			//_compiler.AddValue(0);

			_compiler.EndArray();
		}
		private static void CompileTitleCard(string _path, CompileToC _compiler, string _aseLayer = null)
		{
			string name = Path.GetFileNameWithoutExtension(_path);

			BackgroundTiles tiles = null;
			Background bg = null;

			switch (Path.GetExtension(_path))
			{
				case ".bmp":
					throw new Exception(); // BMP no longer supported
				case ".png":
					{
					if (_aseLayer != null)
						break;

					Bitmap map = new Bitmap(_path);
					if (map.Width != 240 || map.Height != 160)
					{
						map.Dispose();
						break;
					}

					bg = new Background(map, tiles);

					map.Dispose();

					break;
				}
				case ".ase":
					using (AsepriteReader read = new AsepriteReader(_path))
					{
						if (read.Width != 240 || read.Height != 160)
							break;

						List<FloatColor> palette = new List<FloatColor>(read.ColorPalette);

						if (_aseLayer != null && read.LayerNames.Contains(_aseLayer))
						{
							bg = new Background(read, tiles, _aseLayer);
						}
						else
						{
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

		public static IEnumerable<uint> GetArrayFromSprite(int _width, int _height, bool _largeTiles = false, bool _background = false)
		{
			yield break;
			if (_largeTiles)
			{
				for (int yL = 0; yL < _height >> 3; yL += 2)
				{
					for (int xL = 0; xL < _width >> 3; ++xL)
					{
						for (int y = 0; y < 16; ++y)
						{
							uint tempValue = 0;

							//for (int i = 7; i >= 0; --i)
							//	tempValue = (tempValue << 4) | _values((xL << 3) + i, (yL << 3) + y);

							//yield return tempValue;
						}
					}
				}
			}
			else if (_background)
			{
				for (int yW = 0; yW < _height >> 3; yW += 32)
				{
					for (int xW = 0; xW < _width >> 3; xW += 32)
					{
						for (int yL = 0; yL < 32; ++yL)
						{
							for (int xL = 0; xL < 32; ++xL)
							{
								for (int y = 0; y < 8; ++y)
								{
									//uint tempValue = 0;

									//for (int i = 7; i >= 0; --i)
									//	tempValue = (tempValue << 4) | _values(((xL + xW) << 3) + i, ((yL + yW) << 3) + y);

									//yield return tempValue;
								}
							}
						}
					}
				}
			}
			else
			{
				for (int yL = 0; yL < _height >> 3; ++yL)
				{
					for (int xL = 0; xL < _width >> 3; ++xL)
					{
						for (int y = 0; y < 8; ++y)
						{
							//uint tempValue = 0;

							//for (int i = 7; i >= 0; --i)
							//	tempValue = (tempValue << 4) | _values((xL << 3) + i, (yL << 3) + y);

							//yield return tempValue;
						}
					}
				}
			}

		}

	}
}
