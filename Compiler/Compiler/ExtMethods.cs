using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using Newtonsoft.Json;

namespace GBA_Compiler {
    public static class ExtMethods {

		public static T GetXY<T>(this T[] _array, int x, int y, int width) {
			return _array[x + (y * width)];
		}
		public static void SetXY<T>(this T[] _array, int x, int y, int width, T value) {
			_array[x + (y * width)] = value;
		}
		public static void Flip<T>(this T[] _array, bool X, int width) {
			int height = _array.Length / width;
			
			// Only flip if array is rectangular based on width variable
			if (height * width != _array.Length)
				return;

			if (X) {
				for (int x = 0; x < width / 2; ++x) {
					int otherX = (width - x) - 1;
					for (int y = 0; y < height; ++y) {

						T temp = _array.GetXY(x, y, width);

						_array.SetXY(x, y, width, _array.GetXY(otherX, y, width));

						_array.SetXY(otherX, y, width, temp);
					}
				}
			}
			else {
				for (int y = 0; y < height / 2; ++y) {
					int otherY = (height - y) - 1;
					for (int x = 0; x < width; ++x) {

						T temp = _array.GetXY(x, y, width);

						_array.SetXY(x, y, width, _array.GetXY(x, otherY, width));

						_array.SetXY(x, otherY, width, temp);
					}
				}
			}
		}
        public static uint GetWrapping<T>(this T[,] array, int x, int y, T[] check, params Point[] points) {

			int width = array.GetLength(0);
			int height = array.GetLength(1);

			uint retval = 0;

			foreach (var p in points) {
				retval <<= 1;

				Point ex = new Point(Clamp(x + p.X, 0, width - 1), Clamp(y + p.Y, 0, height - 1));

				if (check.Contains(array[ex.X, ex.Y]))
					retval |= 1;
			}

			return retval;
		}
		public static T GetRandom<T>(this T[] array, Random random) {
			return array[random.Next(0, array.Length)];
		}
		public static int Clamp(int value, int min, int max) {
			return Math.Min(Math.Max(value, min), max);
		}
		public static ushort ToGBA(this Color _color, ushort _transparent = 0x8000) {
			if (_color.R == 0 && _color.G == 0 && _color.B == 0)
				return _transparent;

			int r = (_color.R & 0xF8) >> 3;
			int g = (_color.G & 0xF8) >> 3;
			int b = (_color.B & 0xF8) >> 3;


			return (ushort)(r | (g << 5) | (b << 10));
		}
        
    }
}