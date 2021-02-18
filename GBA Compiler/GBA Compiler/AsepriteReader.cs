using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace GBA_Compiler {

	public enum BlendType : ushort {
		Normal = 0,
		Multiply = 1,
		Screen = 2,
		Overlay = 3,
		Darken = 4,
		Lighten = 5,
		Color_Dodge = 6,
		Color_Burn = 7,
		Hard_Light = 8,
		Soft_Light = 9,
		Difference = 10,
		Exclusion = 11,
		Hue = 12,
		Saturation = 13,
		Color = 14,
		Luminosity = 15,
		Addition = 16,
		Subtract = 17,
		Divide = 18,
	}
	public struct FloatColor {
		public float R, G, B, A;

		public FloatColor(byte r, byte g, byte b, byte a) {
			R = r / 255f;
			G = g / 255f;
			B = b / 255f;
			A = a / 255f;
		}

		public static FloatColor FlattenColor(FloatColor colorA, FloatColor colorB, BlendType blend) {
			FloatColor color = colorA;

			if (colorB.A <= 0)
				return colorA;

			switch (blend) {
				case BlendType.Normal:
					color.R = colorB.R;
					color.G = colorB.G;
					color.B = colorB.B;
					color.A = Math.Max(colorA.A, colorB.A);
					break;
			}

			return color;
		}

		public ushort ToGBA(ushort _transparent = 0x8000) {
			if (A <= 0)
				return _transparent;

			int r = (int)(R * 255);
			int g = (int)(G * 255);
			int b = (int)(B * 255);

			r = (r & 0xF8) >> 3;
			g = (g & 0xF8) >> 3;
			b = (b & 0xF8) >> 3;

			return (ushort)(r | (g << 5) | (b << 10));
		}
	}

	/// <summary>
	/// Created using this guide https://github.com/aseprite/aseprite/blob/master/docs/ase-file-specs.md
	/// </summary>
	public class AsepriteReader : BinaryReader {
		private class Layer {
			public bool visible = true;
			public BlendType blending;
			public string name;

			public Cel[] cels;

			public Layer(ushort flags, ushort blend, string _name, int celCount) {
				visible = (flags & 0x1) != 0;
				blending = (BlendType)blend;
				name = _name;
				cels = new Cel[celCount];
			}

		}
		public class Tag {
			public string name;
			public int start, end;

		}
		public class Cel {
			public int X, Y, width, height;

			public byte opacity;

			public FloatColor[] colors;

			public Cel(AsepriteReader reader, FloatColor[] palette, int bpp, int bitSize) {
				X = reader.ReadInt16();
				Y = reader.ReadInt16();
				opacity = reader.ReadByte();

				bool compressed = reader.ReadUInt16() == 2;

				reader.BaseStream.Seek(7, SeekOrigin.Current);

				width = reader.ReadInt16();
				height = reader.ReadInt16();

				colors = new FloatColor[width * height];

				byte[] bits;

				if (compressed) {
					reader.BaseStream.Seek(2, SeekOrigin.Current);

					bits = reader.ReadBytes(bitSize - 2);

					using (BinaryReader read = new BinaryReader(new DeflateStream(new MemoryStream(bits, false), CompressionMode.Decompress))) {
						bits = read.ReadBytes(width * height * bpp / 8);
					}

				}
				else {
					bits = reader.ReadBytes(width * height * bpp / 8);
				}

				switch (bpp) {
					case 8:
						for (int i = 0; i < width * height; ++i)
							colors[i] = palette[bits[i]];
						break;
					case 16:
						for (int i = 0; i < width * height; ++i) {
							byte val = bits[i << 1];
							colors[i] = new FloatColor(val, val, val, bits[(i << 1) + 1]);
						}
						break;
					case 32:
						for (int i = 0; i < width * height; ++i)
							colors[i] = new FloatColor();
						break;
				}
			}
		}

		int frameCount;
		int BPP;

		public int Width { get; private set; }
		public int Height { get; private set; }

		private int transparent;

		private List<FloatColor> filePalette;
		private List<Layer> layers = new List<Layer>();
		private List<Tag> tags = new List<Tag>();

		public string[] TagNames {
			get {
				List<string> t = new List<string>();
				foreach (var tag in tags)
					t.Add(tag.name);

				return t.ToArray();
			}
		}
		public Tag[] Tags { get { return tags.ToArray(); } }
		public string[] LayerNames {
			get {
				string[] retval = new string[layers.Count];
				for (int i = 0; i < layers.Count; ++i)
					retval[i] = layers[i].name;
				return retval;
			}
		}
		public FloatColor[] Colors { get { return filePalette.ToArray(); } }

		public override float ReadSingle() {
			int value = ReadInt32();

			return value / (0x10000f);
		}
		public override string ReadString() {
			byte[] array = ReadBytes(ReadUInt16());

			return Encoding.UTF8.GetString(array);
		}
		public FloatColor ReadColor(int _x, int _y, int _frame = 0, string _layer = null) {
			FloatColor retval = new FloatColor();

			foreach (var layer in layers) {
				if (_layer != null && layer.name != _layer)
					continue;

				var cel = layer.cels[_frame];

				if (!layer.visible || cel == null)
					continue;

				if (_x < cel.X || _x >= cel.X + cel.width || _y < cel.Y || _y >= cel.Y + cel.height)
					continue;

				retval = FloatColor.FlattenColor(retval, cel.colors[(_x - cel.X) + (_y - cel.Y) * cel.width], layer.blending);
			}

			return retval;
		}

		public AsepriteReader(string _filePath) : base(File.Open(_filePath, FileMode.Open)) {
			BaseStream.Seek(4, SeekOrigin.Begin);
			if (ReadUInt16() != 0xA5E0)
				throw new FileLoadException();

			frameCount = ReadUInt16();
			Width = ReadUInt16();
			Height = ReadUInt16();

			BPP = ReadUInt16();

			BaseStream.Seek(14, SeekOrigin.Current);

			int transparent = ReadByte();

			BaseStream.Seek(3, SeekOrigin.Current);

			filePalette = new List<FloatColor>(new FloatColor[ReadUInt16()]);

			// Seek to the end of the header
			BaseStream.Seek(128, SeekOrigin.Begin);

			for (int i = 0; i < frameCount; ++i) {
				BaseStream.Seek(4, SeekOrigin.Current);

				string test = ReadUInt16().ToString("X");

				uint chunkCount = ReadUInt16();
				BaseStream.Seek(4, SeekOrigin.Current);

				if (chunkCount == 0xFFFF)
					chunkCount = ReadUInt32();
				else
					BaseStream.Seek(4, SeekOrigin.Current);

				bool usesNewPal = false;

				for (int chunk = 0; chunk < chunkCount; ++chunk) {
					long pos = BaseStream.Position;

					uint size = ReadUInt32();
					ushort type = ReadUInt16();

					if (type == 0x2019)
						usesNewPal = true;

					if (type != 0x0004 || !usesNewPal)
						ReadChunk(type, i, size);

					BaseStream.Seek(pos + size, SeekOrigin.Begin);
				}
			}

		}

		private void ReadChunk(uint type, int frameIndex, uint size) {
			long chunkStart = BaseStream.Position;

			switch (type) {
				case 0x2018: // Tag data
					{
					ushort count = ReadUInt16();
					BaseStream.Seek(8, SeekOrigin.Current);
					for (int i = 0; i < count; ++i) {
						int from = ReadUInt16(), to = ReadUInt16();
						BaseStream.Seek(13, SeekOrigin.Current);

						string name = ReadString();

						tags.Add(new Tag() { start = from, end = to, name = name });
					}
				}
				break;
				case 0x2004: // Layer Data
					{
					ushort flags = ReadUInt16();
					BaseStream.Seek(8, SeekOrigin.Current);
					ushort blend = ReadUInt16();
					BaseStream.Seek(4, SeekOrigin.Current);
					string name = ReadString();

					layers.Add(new Layer(flags, blend, name, frameCount));
				}
				break;
				case 0x2005: // Cel Data
					{
					var layer = layers[ReadUInt16()];

					long idx = BaseStream.Position;
					BaseStream.Seek(5, SeekOrigin.Current);

					Cel cel = null;

					var b = ReadUInt16();
					switch (b) {
						case 1:
							BaseStream.Seek(7, SeekOrigin.Current);

							cel = layer.cels[ReadUInt16()];
							break;
						default:
							BaseStream.Seek(idx, SeekOrigin.Begin);
							cel = new Cel(this, filePalette.ToArray(), BPP, (int)size - 20);
							break;
					}


					layer.cels[frameIndex] = cel;
				}
				break;
				case 0x2007:

					break;
				case 0x2019: // Palette Data
					{
					int palSize = ReadInt32();

					if (palSize != filePalette.Count) {
						if (palSize > filePalette.Count)
							filePalette.AddRange(new FloatColor[palSize - filePalette.Count]);
						else
							while (filePalette.Count > palSize)
								filePalette.RemoveAt(filePalette.Count - 1);
					}

					int start = ReadInt32();
					int end = ReadInt32();

					BaseStream.Seek(8, SeekOrigin.Current);

					for (; start <= end; ++start) {
						bool hasName = ReadInt16() == 1;

						FloatColor c = new FloatColor(ReadByte(), ReadByte(), ReadByte(), ReadByte());

						filePalette[start] = c;

						if (hasName)
							BaseStream.Seek(ReadUInt16(), SeekOrigin.Current);

					}

					filePalette[transparent] = new FloatColor();
				}
				break;
			}
		}

		public IEnumerable<uint[]> GetSprites(string _tag = null, string _layer = null, bool _readBackwards = false, bool _pal0IsClear = true) {
			int startFrame = 0, endFrame = frameCount - 1;

			if (_tag != null) {
				foreach (var tag in tags) {
					if (tag.name == _tag) {
						startFrame = tag.start;
						endFrame = tag.end;
						break;
					}
				}
			}

			List<ushort[]> palettes = new List<ushort[]>();

			for (int i = 0; i < filePalette.Count; i += 16) {
				ushort[] add = new ushort[16];
				for (int j = 0; j < 16; ++j) {
					add[j] = filePalette[i + j].ToGBA();
				}
				if (_pal0IsClear)
					add[0] = 0x8000;
				palettes.Add(add);
			}

			for (; startFrame <= endFrame; ++startFrame) {
				FloatColor[,] rawArt = new FloatColor[Width, Height];

				foreach (var layer in layers) {
					if (_layer != null && layer.name != _layer)
						continue;

					var cel = layer.cels[startFrame];

					if (!layer.visible || cel == null)
						continue;

					for (int y = 0; y < cel.height; ++y) {
						for (int x = 0; x < cel.width; ++x) {
							rawArt[x + cel.X, y + cel.Y] = FloatColor.FlattenColor(rawArt[x + cel.X, y + cel.Y], cel.colors[x + y * cel.height], layer.blending);
						}
					}
				}

				ushort[] pal = null;

				foreach (var testPal in palettes) {
					bool success = true;
					foreach (FloatColor c in rawArt) {
						if (!testPal.Contains(c.ToGBA())) {
							success = false;
							break;
						}
					}

					if (success) {
						pal = testPal;
						break;
					}
				}

				if (pal == null)
					continue;

				uint foreward(int x, int y) { return (uint)Array.IndexOf(pal, rawArt[x, y].ToGBA()); };
				uint backward(int x, int y) {
					x = (x & 7) | (Width  - (x & ~0x7) - 8);
					y = (y & 7) | (Height - (y & ~0x7) - 8);

					return (uint)Array.IndexOf(pal, rawArt[x, y].ToGBA());
				};

				List<uint> retval;
				if (_readBackwards)
					retval = new List<uint>(ArtCompiler.GetArrayFromSprite(Width, Height, backward));
				else
					retval = new List<uint>(ArtCompiler.GetArrayFromSprite(Width, Height, foreward));


				yield return retval.ToArray();
			}

			yield break;
		}
	}
}