using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Drawing;
using System.Collections;
using DSDecmp;

namespace Pixtro.Compiler {
	public class LevelPackMetadata {
		public class TileWrapping {
			// TODO: Create feature that lets users copy mapping data from one version to another

			public int[] Palettes;
			public string Tileset;
			public char MappingCopy;
			public byte CollisionType;

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

		[JsonIgnore]
		public LevelBrickset fullTileset = null;

		public Dictionary<string, int> EntityIndex;

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
	public class Brick : Tile
	{
		public char collisionChar;
		public int collisionType, collisionShape, palette;

		public class CompareBricks : IEqualityComparer<Brick>
		{
			public bool Equals(Brick x, Brick y)
			{
				if (!x.EqualTo(y, Tile.FlipStyle.Both))
					return false;

				if (x.collisionChar != y.collisionChar && x.collisionShape != y.collisionShape && x.collisionType != y.collisionType && x.palette != y.palette)
					return false;

				return true;
			}

			public int GetHashCode(Brick obj)
			{
				return obj.GetHashCode();
			}
		}

		public Brick(int pixelSize) : base(pixelSize) { }
		public Brick(Tile tileCopy) : base(tileCopy.sizeOfTile)
		{
			LoadInData(tileCopy.RawData, 0);
		}
	}
	public class LevelBrickset : IEnumerable<Brick>
	{
		List<Brick> bricks = new List<Brick>();
		List<Tile> rawTiles = new List<Tile>();

		public IReadOnlyList<Tile> RawTiles => rawTiles;

		public LevelBrickset()
		{

		}

		public bool Contains(Brick brick)
		{
			return bricks.Contains(brick, new Brick.CompareBricks());
		}
		public void AddNewBrick(Brick brick)
		{
			if (!Contains(brick))
			{
				bricks.Add(brick);
				int size = brick.sizeOfTile / 8;
				size *= size;

				for (int i = 0; i < size; ++i)
				{
					var tile = new Tile(8);
					tile.LoadInData(brick.RawData, i * 8);

					if (!rawTiles.Contains(tile, new Tileset.CompareTiles()))
					{
						rawTiles.Add(tile);
					}
				}
			}
		}

		public ushort GetIndex(Tile tile, char type) => GetIndex(GetBrick(tile, type));

		public ushort GetIndex(Brick brick)
		{
			return (ushort)bricks.IndexOf(brick);
		}
		public Brick GetBrick(Tile tile, char type)
		{
			foreach (var b in bricks)
			{
				if (b.collisionChar != type)
					continue;

				if (b.EqualTo(tile, Tile.FlipStyle.Both))
					return b;
			}
			return null;
		}

		public IEnumerator<Brick> GetEnumerator()
		{
			return bricks.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator<Brick>)bricks;
		}
	}
	public class CompressedLevel {

		private const int multValue = 57047;

		public static uint RNGSeed;
		private static int RandomFromPoint(Point point)
		{
			ulong tempVal = (ulong)(RNGSeed + (uint)point.X);
			tempVal = (tempVal * multValue) % int.MaxValue;
			tempVal += (ulong)point.Y;
			tempVal = (tempVal * multValue) % int.MaxValue;

			return (int)tempVal;
		}

		public static Random Randomizer;

		public static LevelPackMetadata DataParse;

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
			List<byte> bytes = new List<byte>(Enumerable.ToArray(GetBinary()));

			while ((bytes.Count & 0x3) != 0)
				bytes.Add(0xFF);

			return bytes.ToArray();
		}
		private IEnumerable<byte> GetBinary() {
			foreach (var b in Header())
				yield return b;

			if ((metadata.Count & 0x1) == 1)
			{
				yield return 3;
				yield return 0xFF;
				yield return 0xFF;
			}
			else
			{
				yield return 1;
			}

			for (int i = 0; i < layers; ++i)
				foreach (var b in VisualLayer(i))
					yield return b;

			foreach (var b in Entities())
				yield return b;
			
			yield break;
		}

		private IEnumerable<byte> Header() {
			foreach (byte b in BitConverter.GetBytes((short)width))
				yield return b;
			foreach (byte b in BitConverter.GetBytes((short)height))
				yield return b;

			foreach (var b in metadata.Keys) {
				yield return b;
				yield return metadata[b];
			}
			yield return 0xFF;
			
			yield break;
		}
		private IEnumerable<byte> VisualLayer(int layer) {
			
			int x, y;
			
			List<char> characters = new List<char>(DataParse.Wrapping.Keys);
			Dictionary<char, uint[]> connect = new Dictionary<char, uint[]>();
			LevelBrickset fullTileset;

			if (DataParse.fullTileset != null)
				fullTileset = DataParse.fullTileset;

			// If there isn't a brickset created, make a new one
			else {
				fullTileset = new LevelBrickset();

				List<string> found = new List<string>();

				// Foreach tile type ('M', 'N' or whatever)
				foreach (var t in DataParse.Wrapping.Keys) {

					int collType = DataParse.Wrapping[t].CollisionType;

					foreach (var tile in ArtCompiler.ArtTilesets[DataParse.Wrapping[t].Tileset].tiles)
					{
						if (tile.tile.IsAir)
							continue;

						var brick = new Brick(tile);
						brick.collisionType = collType;
						brick.collisionChar = t;

						fullTileset.AddNewBrick(brick);
					}
						
					found.Add(DataParse.Wrapping[t].Tileset);
				}

				DataParse.fullTileset = fullTileset;
			}

			foreach (var tile in DataParse.Wrapping.Keys) {

				List<uint> conns = new List<uint>();

				foreach (var name in DataParse.Wrapping[tile].Connections)
					conns.Add((uint)characters.IndexOf(name) + 1);

				connect.Add(tile, conns.ToArray());
			}

			uint[,] data = new uint[width, height];

			for (y = 0; y < height; ++y) {
				for (x = 0; x < width; ++x) {
					data[x, y] = levelData[layer, x, y] == ' ' ? 0 : (uint)characters.IndexOf(levelData[layer, x, y]) + 1;
				}
			}

			ushort last = 0x1234;
			int count = 0;

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

			byte[] retvalArray = new byte[width * height * 2];

			for (y = 0; y < height; ++y)
			{
				for (x = 0; x < width; ++x)
				{
					ushort retval;

					if (levelData[layer, x, y] == ' ')
					{
						retval = 0;
					}
					else
					{
						var wrapping = DataParse.Wrapping[levelData[layer, x, y]];

						Tile tile = null;
						Tileset tileset = ArtCompiler.ArtTilesets[wrapping.Tileset];
						uint value = data.GetWrapping(x, y, connect[levelData[layer, x, y]], wrapping.Mapping),
						testValue;

						if (wrapping.MappingSpecial != null)
							foreach (string str in wrapping.MappingSpecial)
							{

								var dp = new DataParser(str);

								value <<= 1;
								value |= (uint)(dp.GetBoolean(getvalue) ? 1 : 0);
							}

						foreach (var key in wrapping.TileMapping.Keys)
						{
							testValue = wrapping.EnableMask[key];

							if ((testValue & value) != testValue)
								continue;

							testValue = wrapping.DisableMask[key];

							if ((testValue & (value)) != 0)
								continue;

							var point = wrapping.TileMapping[key].GetValueWrapped(RandomFromPoint(new Point(x, y)));

							if (wrapping.Offsets != null)
								foreach (var o in wrapping.Offsets.Keys)
								{

									testValue = wrapping.EnableMask[o];

									if ((testValue & value) != testValue)
										continue;

									testValue = wrapping.DisableMask[o];

									if ((testValue & (value)) != 0)
										continue;

									foreach (var exPoint in wrapping.Offsets[o])
										point = new Point(point.X + exPoint.X, point.Y + exPoint.Y);
								}

							tile = tileset.GetOriginalTile(point.X, point.Y);
							break;
						}

						Tile mappedTile = tileset.GetUniqueTile(tile??tileset.GetOriginalTile(0, 0));

						retval = (ushort)((fullTileset.GetIndex(mappedTile, levelData[layer, x, y]) + 1) | (tile.GetFlipOffset(mappedTile) << 10));
					}

					retvalArray[count + 1] = (byte)(retval >> 8);
					retvalArray[count] = (byte)(retval & 0xFF);

					count += 2;

				}
			}

			count = 0;
			retvalArray = LZUtil.Compress(retvalArray);
			yield return (byte)(retvalArray.Length & 0xFF);
			yield return (byte)(retvalArray.Length >> 8);

			foreach (var b in retvalArray)
			{
				yield return b;
				count++;
			}

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
			yield return 0xFF;
			yield break;
		}

	}
}