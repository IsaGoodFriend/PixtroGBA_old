using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixtro.Compiler
{
	public class Tile
	{
		[Flags]
		public enum FlipStyle { X = 1, Y = 2, Both = X | Y, None = 0 }

		private byte[] bitData;
		private uint[] rawData;
		private FlipStyle flipped;

		internal int sizeOfTile = 0;

		public bool IsAir { get; private set; }

		public uint[] RawData => rawData;

		public byte this[int x, int y]
		{
			get
			{
				return bitData[x + (y * sizeOfTile)];
			}
		}

		public Tile(int pixelSize)
		{
			IsAir = true;
			sizeOfTile = pixelSize;

			pixelSize /= 8;

			bitData = new byte[64 * pixelSize * pixelSize];
			rawData = new uint[8 * pixelSize * pixelSize];
		}
		
		public void LoadInData(uint[] _GBA, int _index)
		{
			int size = sizeOfTile / 8;
			size *= size;

			IsAir = true;
			for (int j = 0; j < 8 * size; ++j)
			{
				rawData[j] = _GBA[j + _index];

				IsAir &= rawData[j] == 0;

				int i;
				if (size > 1)
				{
					i = (j >> 1) % 8;
					i += (j % 2) * 16;
					i += (j & 0xFF0) >> 1;
				}
				else
					i = j;
				i += _index;

				bitData[j << 3]        = (byte)(_GBA[i] & 0x0000000F);
				bitData[(j << 3) + 1]   = (byte)((_GBA[i] & 0x000000F0) >> 4);
				bitData[(j << 3) + 2]   = (byte)((_GBA[i] & 0x00000F00) >> 8);
				bitData[(j << 3) + 3]   = (byte)((_GBA[i] & 0x0000F000) >> 12);
				bitData[(j << 3) + 4]   = (byte)((_GBA[i] & 0x000F0000) >> 16);
				bitData[(j << 3) + 5]   = (byte)((_GBA[i] & 0x00F00000) >> 20);
				bitData[(j << 3) + 6]   = (byte)((_GBA[i] & 0x0F000000) >> 24);
				bitData[(j << 3) + 7]   = (byte)((_GBA[i] & 0xF0000000) >> 28);

			}
		}

		public void Flip(FlipStyle _x)
		{
			if (_x == FlipStyle.X)
			{
				flipped ^= FlipStyle.X;
				bitData.Flip(true, sizeOfTile);
			}
			else if (_x == FlipStyle.Y)
			{
				flipped ^= FlipStyle.Y;
				bitData.Flip(false, sizeOfTile);
			}
			else if (_x != FlipStyle.None)
			{
				Flip(FlipStyle.X);
				Flip(FlipStyle.Y);
			}
		}
		public void Unflip()
		{
			Flip(flipped);
		}

		public bool EqualTo(Tile _other, FlipStyle flippable)
		{
			if (_other == null)
				return false;

			Unflip();
			_other.Unflip();

			// values aren't the same size, so return false
			if (_other.sizeOfTile != sizeOfTile || bitData.Length != _other.bitData.Length)
				return false;

			if (Enumerable.SequenceEqual(bitData, _other.bitData))
				return true;

			// Check mirrored on X axis
			if (flippable.HasFlag(FlipStyle.X))
			{
				Flip(FlipStyle.X);

				if (Enumerable.SequenceEqual(bitData, _other.bitData))
					return true;

				if (flippable.HasFlag(FlipStyle.Y))
				{
					Flip(FlipStyle.Y);

					if (Enumerable.SequenceEqual(bitData, _other.bitData))
						return true;
				}

				Unflip();
			}
			// Check mirrored on the y axis only
			if (flippable.HasFlag(FlipStyle.Y))
			{
				Flip(FlipStyle.Y);

				if (Enumerable.SequenceEqual(bitData, _other.bitData))
					return true;
			}

			return false;
		}

		public FlipStyle GetFlipDifference(Tile _other)
		{
			if (sizeOfTile != _other.sizeOfTile)
				throw new Exception();

			Unflip();
			_other.Unflip();

			if (Enumerable.SequenceEqual(bitData, _other.bitData))
				return FlipStyle.None;

			Flip(FlipStyle.X);
			if (Enumerable.SequenceEqual(bitData, _other.bitData))
				return FlipStyle.X;

			Flip(FlipStyle.Y);
			if (Enumerable.SequenceEqual(bitData, _other.bitData))
				return FlipStyle.Both;

			Flip(FlipStyle.X);
			if (Enumerable.SequenceEqual(bitData, _other.bitData))
				return FlipStyle.Y;

			throw new Exception();
		}
		public ushort GetFlipOffset(Tile _other)
		{
			return (ushort)GetFlipDifference(_other);
		}

	}
	public class Tileset
	{
		internal class TileCounts
		{
			public Tile tile;
			public int count;

			public static implicit operator Tile(TileCounts c) => c.tile;
			public static implicit operator TileCounts(Tile t) => new TileCounts() { tile = t, count = 0 };
		}

		public Tile.FlipStyle flipAcceptance;

		int _width;
		private Tile[,] originalLayout;
		internal List<TileCounts> tiles = new List<TileCounts>();

		public Tileset(int width, int height)
		{
			_width = width;
			originalLayout = new Tile[width, height];
		}

		internal class CompareTileCount : IEqualityComparer<TileCounts>
		{
			public Tile.FlipStyle flipStyle = Tile.FlipStyle.Both;

			public bool Equals(TileCounts x, TileCounts y)
			{
				return x.tile.EqualTo(y.tile, flipStyle);
			}

			public int GetHashCode(TileCounts obj)
			{
				return obj.tile.GetHashCode();
			}
		}
		internal class CompareTiles : IEqualityComparer<Tile>
		{
			public CompareTiles()
			{

			}
			public CompareTiles(Tile.FlipStyle style)
			{
				flipStyle = style;
			}
			public Tile.FlipStyle flipStyle = Tile.FlipStyle.Both;

			public bool Equals(Tile x, Tile y)
			{
				return x.EqualTo(y, flipStyle);
			}

			public int GetHashCode(Tile obj)
			{
				return obj.GetHashCode();
			}
		}


		public void SetTile(Tile tile, int x, int y)
		{
			var otherTile = originalLayout[x, y];

			if (otherTile == null && tile == null)
				return;
			
			if (otherTile != null && tile.EqualTo(otherTile, flipAcceptance))
				return;

			TileCounts uniqueTileOld = null, uniqueTileNew = null;

			foreach (var t in tiles)
			{
				if (tile != null && t.tile.EqualTo(tile, flipAcceptance))
					uniqueTileNew = t;
				if (otherTile != null && t.tile.EqualTo(otherTile, flipAcceptance))
					uniqueTileOld = t;
			}

			if (uniqueTileOld != null)
			{
				uniqueTileOld.count--;
				if (uniqueTileOld.count == 0)
				{
					tiles.Remove(uniqueTileOld);
				}
			}

			if (tile != null)
			{
				if (uniqueTileNew == null)
				{
					tiles.Add(tile);
					uniqueTileNew = tiles[tiles.Count - 1];
				}

				uniqueTileNew.count++;
			}

			originalLayout[x, y] = tile;
		}

		public void AddTiles(IEnumerator<uint> tileData)
		{
			int x = 0, y = 0;
			do
			{
				List<uint> tileRaw = new List<uint>();

				for (int i = 0; i < 8 * Settings.BrickTileSize * Settings.BrickTileSize; ++i)
				{
					tileRaw.Add(tileData.Current);
					tileData.MoveNext();
				}

				if (tileRaw.Count < 8)
					break;

				var tile = new Tile(8 * Settings.BrickTileSize);
				tile.LoadInData(tileRaw.ToArray(), 0);

				SetTile(tile, x, y);

				x = (++x) % _width;
				if (x == 0)
					++y;


			} while (true);
		}

		public Tile GetUniqueTile(Tile version)
		{

			foreach (var t in tiles)
			{
				if (t.tile.EqualTo(version, Tile.FlipStyle.Both))
					return t;
			}

			return null;
		}
		public Tile GetUniqueTile(int x, int y)
		{
			if (originalLayout == null)
				return null;

			return GetUniqueTile(GetOriginalTile(x, y));
		}
		public Tile GetOriginalTile(int x, int y)
		{
			if (originalLayout == null)
				return null;

			return originalLayout[x, y];
		}
		public Tile.FlipStyle GetFlipIndex(int x, int y)
		{
			Tile ogTile = GetOriginalTile(x, y),
				uniqueTile = GetUniqueTile(x, y);

			return ogTile.GetFlipDifference(uniqueTile);
		}
		public virtual ushort GetIndex(Tile _version)
		{
			return (ushort)tiles.IndexOf(GetUniqueTile(_version));
		}
		public virtual IEnumerable<uint> Data(string _name)
		{

			foreach (var tile in tiles)
				foreach (var v in tile.tile.RawData)
					yield return v;
		}
	}
}
