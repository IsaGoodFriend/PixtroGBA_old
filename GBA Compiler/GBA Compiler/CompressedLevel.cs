using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GBA_Compiler {
	public class LevelParse {
		public class TileWrapping {

			public int[] Palettes;
			public byte CollisionType;
			public string Tileset;
			public char[] Connections;
			public Point[] Mapping;
			public Dictionary<string, Point[]> TileOffsets;
		}
		public Dictionary<char, TileWrapping> Wrapping;

		public Dictionary<string, byte> EntityIndex;

		public string Include, Exclude;
		public string IncludeRegex, ExcludeRegex;

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
			ushort[] retvals = new ushort[width * height];
			int i = 0;

			for (int y = 0; y < height; ++y) {
				for (int x = 0; x < width; ++x) {
					data[x, y] = levelData[layer, x, y] == ' ' ? 0 : (uint)characters.IndexOf(levelData[layer, x, y]) + 1;
				}
			}
			for (int y = 0; y < height; ++y) {
				for (int x = 0; x < width; ++x) {
					if (levelData[layer, x, y] == ' ') {
						retvals[i++] = 0;
						continue;
					}

					var wrapping = DataParse.Wrapping[levelData[layer, x, y]];

					Tile tile = null;
					LevelTileset tileset = ArtCompiler.LevelTilesets[wrapping.Tileset];
					uint value = data.GetWrapping(x, y, connect[levelData[layer, x, y]], wrapping.Mapping), testValue;

					foreach (var key in wrapping.TileOffsets.Keys) {
						testValue = Convert.ToUInt32(key.Replace("*", "0"), 2);

						if ((testValue & value) != testValue)
							continue;

						testValue = ~Convert.ToUInt32(key.Replace("*", "1"), 2);

						if ((testValue & (value)) != 0)
							continue;
						
						var point = wrapping.TileOffsets[key].GetRandom(Randomizer);

						tile = tileset.GetOGTile(point.X, point.Y);
						break;
					}

					Tile mappedTile = tileset.GetTile(tile);

					retvals[i++] = (ushort)((fullTileset.IndexOf(mappedTile) + 1) | (mappedTile.GetFlipOffset(tile) << 10) | (wrapping.Palettes[layer] << 12));
				}
			}

			ushort last = retvals[0];
			byte count = 0;

			foreach (var v in retvals) {
				if (v != last || count == 255) {
					yield return count;
					yield return (byte)(last       & 0xFF);
					yield return (byte)((last >> 8));

					last = v;
					count = 0;
				}
				++count;
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