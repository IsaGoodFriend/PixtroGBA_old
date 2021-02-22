using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace GBA_Compiler {
	public class LevelParse {
		public class TileWrapping {

			public int[] Palettes;
			public byte CollisionType;
			public string Tileset;
			public char[] Connections;
			public Point[] Mapping;
			public string[] MappingSpecial;
			public Dictionary<string, Point[]> TileMapping;
			public Dictionary<string, Point[]> Offsets;

			[JsonIgnore]
			public Dictionary<string, uint> EnableMask, DisableMask;

			public void FinalizeMasks() {
				EnableMask = new Dictionary<string, uint>();
				DisableMask = new Dictionary<string, uint>();

				foreach (string key in TileMapping.Keys) {
					EnableMask.Add(key, Convert.ToUInt32(key.Replace("*", "0"), 2));
					DisableMask.Add(key, ~Convert.ToUInt32(key.Replace("*", "1"), 2));
				}
				if (Offsets != null)
				foreach (string key in Offsets.Keys) {
					if (EnableMask.ContainsKey(key))
						continue;

					EnableMask.Add(key, Convert.ToUInt32(key.Replace("*", "0"), 2));
					DisableMask.Add(key, ~Convert.ToUInt32(key.Replace("*", "1"), 2));
				}
			}
		}
		public Dictionary<char, TileWrapping> Wrapping;

		public Dictionary<string, byte> EntityIndex;

		public string Include, Exclude;
		public string IncludeRegex, ExcludeRegex;

		public string Name;

		private static bool TestString(string path, string test) {

			if (test == "*")
				return true;
			else if (!test.Contains("*")) {
				return test == path;
			}
			else {
				string[] split = test.Split(new char[]{ '*' }, StringSplitOptions.RemoveEmptyEntries);

				int index = 0;

				if (!test.StartsWith("*") && !path.StartsWith(split[0])) {
					return false;
				}
				if (!test.EndsWith("*") && !path.EndsWith(split[split.Length - 1])) {
					return false;
				}

				for (int i = test.StartsWith("*") ? 0 : 1; i < split.Length - (test.EndsWith("*") ? 0 : 1); ++i) {
					int found = path.IndexOf(split[i]);

					if (found < index)
						return false;

					index = found;
				}

				return true;
			}
		}

		public bool Matches(string _path) {

			bool test = false;

			if (Include != null) {
				string[] split = Include.Split(',');

				foreach (string s in split)
					test |= TestString(_path, s);
			}
			if (IncludeRegex != null) {
				test |= Regex.IsMatch(_path, IncludeRegex);
			}

			if (!test)
				return false;

			if (Exclude != null) {
				string[] split = Exclude.Split(',');

				foreach (string s in split)
					test &= !TestString(_path, s);
			}
			if (!test)
				return false;

			if (ExcludeRegex != null) {
				test &= !Regex.IsMatch(_path, ExcludeRegex);
			}

			return test;
		}
	}
	public class CompressedLevel {

		public static Random Randomizer;

		public static LevelParse DataParse;

		public class Entity {
			public int x, y, type;

			public List<byte> data = new List<byte>();
		}

		private int width, height, layers;

		private char[,,] levelData;

		public Dictionary<byte, byte> metadata = new Dictionary<byte, byte>();
		public List<Entity> entities = new List<Entity>();

		public int Width {
			get { return width; }
			set {
				if (levelData == null) {
					width = value;

					if (width != 0 && height != 0 && layers != 0) {
						levelData = new char[layers, width, height];
					}
				}
			}
		}
		public int Height {
			get { return height; }
			set {
				if (levelData == null) {
					height = value;

					if (width != 0 && height != 0 && layers != 0) {
						levelData = new char[layers, width, height];
					}
				}
			}
		}
		public int Layers {
			get { return layers; }
			set {
				if (levelData == null) {
					layers = Math.Max(Math.Min(value, 3), 1); // can only be between one and three

					if (width != 0 && height != 0 && layers != 0) {
						levelData = new char[layers, width, height];
					}
				}
			}
		}

		public void AddLine(int layer, int line, string data) {
			for (int i = 0; i < width; ++i) {
				levelData[layer, i, line] = data[i];
			}
		}

		public byte[] BinaryData() {
			return Enumerable.ToArray(GetBinary());
		}
		private IEnumerable<byte> GetBinary() {
			foreach (var b in Header())
				yield return b;

			for (int i = 0; i < layers; ++i)
				foreach (var b in VisualLayer(i))
					yield return b;

			foreach (var b in Entities())
				yield return b;
			
			yield break;
		}

		private IEnumerable<byte> Header(){
			yield return (byte)width;
			yield return (byte)height;

			foreach (var b in metadata.Keys) {
				yield return b;
				yield return metadata[b];
			}
			yield return 0xFF;

			foreach (var b in Collision())
				yield return b;

			yield break;
		}
		private IEnumerable<byte> Collision() {

			byte[] data = new byte[width * height];

			int i = 0;

			for (int y = 0; y < height; ++y)
				for (int x = 0; x < width; ++x)
					data[i++] = (byte)(levelData[0, x, y] == ' ' ? 0 : DataParse.Wrapping[levelData[0, x, y]].CollisionType);

			byte last = data[0], count = 0;

			for (i = 0; i < width * height; ++i) {
				if (data[i] != last || count == 255) {
					yield return count;
					yield return last;

					last = data[i];
					count = 0;
				}
				++count;
			}

			yield return count;
			yield return last;

			yield break;
		}
		private IEnumerable<byte> VisualLayer(int layer) {

			List<char> characters = new List<char>(DataParse.Wrapping.Keys);
			Dictionary<char, uint[]> connect = new Dictionary<char, uint[]>();
			List<Tile> fullTileset = new List<Tile>();

			foreach (var name in ArtCompiler.LevelTilesets.Keys) {
				fullTileset.AddRange(ArtCompiler.LevelTilesets[name].tiles);
			}

			foreach (var tile in DataParse.Wrapping.Keys) {

				List<uint> conns = new List<uint>();

				foreach (var name in DataParse.Wrapping[tile].Connections)
					conns.Add((uint)characters.IndexOf(name) + 1);

				connect.Add(tile, conns.ToArray());
			}

			uint[,] data = new uint[width, height];

			for (int y = 0; y < height; ++y) {
				for (int x = 0; x < width; ++x) {
					data[x, y] = levelData[layer, x, y] == ' ' ? 0 : (uint)characters.IndexOf(levelData[layer, x, y]) + 1;
				}
			}

			ushort last = 0;
			byte count = 0;

			for (int y = 0; y < height; ++y) {
				for (int x = 0; x < width; ++x) {
					ushort retval;

					if (levelData[layer, x, y] == ' ') {
						retval = 0;
					}
					else {
						var wrapping = DataParse.Wrapping[levelData[layer, x, y]];

						Tile tile = null;
						LevelTileset tileset = ArtCompiler.LevelTilesets[wrapping.Tileset];
						uint value = data.GetWrapping(x, y, connect[levelData[layer, x, y]], wrapping.Mapping), testValue;

						float getvalue(string _s) {

							string[] split = _s.Split('.');

							switch (split[0]) {
								case "x":
									return x;
								case "y":
									return y;
								case "width":
									return width;
								case "height":
									return height;
								case "random":
									if (split.Length == 2)
										return Randomizer.Next(int.Parse(split[1]));
									else
										return Randomizer.Next(int.Parse(split[1]), int.Parse(split[2]));
							}

							return 0;
						}

						if (wrapping.MappingSpecial != null) 
							foreach (string str in wrapping.MappingSpecial) {

								var dp = new DataParser(str);

								value <<= 1;
								value |= (uint)(dp.GetBoolean(getvalue) ? 1 : 0);
							}

						foreach (var key in wrapping.TileMapping.Keys) {
							testValue = wrapping.EnableMask[key];

							if ((testValue & value) != testValue)
								continue;

							testValue = wrapping.DisableMask[key];

							if ((testValue & (value)) != 0)
								continue;

							var point = wrapping.TileMapping[key].GetRandom(Randomizer);

							if (wrapping.Offsets != null)
								foreach (var o in wrapping.Offsets.Keys) {

									testValue = wrapping.EnableMask[o];

									if ((testValue & value) != testValue)
										continue;

									testValue = wrapping.DisableMask[o];

									if ((testValue & (value)) != 0)
										continue;

									foreach (var exPoint in wrapping.Offsets[o])
										point = new Point(point.X + exPoint.X, point.Y + exPoint.Y);
								}

							tile = tileset.GetOGTile(point.X, point.Y);
							break;
						}

						Tile mappedTile = tileset.GetTile(tile);

						retval = (ushort)(((fullTileset.IndexOf(mappedTile) + 1) << (Compiler.LargeTiles ? 2 : 0)) | (mappedTile.GetFlipOffset(tile) << 10) | (wrapping.Palettes[layer] << 12));
					}

					if ((x != 0 || y != 0) && (retval != last || count == 255)) {
						yield return count;
						yield return (byte)(last       & 0xFF);
						yield return (byte)((last >> 8));

						last = retval;
						count = 0;
					}
					++count;
				}
			}

			yield return count;
			yield return (byte)(last       & 0xFF);
			yield return (byte)((last >> 8));

			yield break;
		}
		private IEnumerable<byte> Entities() {
			foreach (var ent in entities) {
				yield return (byte)ent.type;
				yield return (byte)ent.x;
				yield return (byte)ent.y;

				foreach (var b in ent.data)
					yield return b;

				yield return 0xFF;
			}
			yield break;
		}

	}
}