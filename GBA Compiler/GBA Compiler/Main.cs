using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GBA_Compiler {
	public struct Point {
		public int X, Y;

		public Point(int x, int y) { X = x; Y = y; }
	}
	public static class Compiler {
		public static uint GetWrapping<T>(this T[,] array, int x, int y, T[] check, params Point[] points) {

			int width = array.GetLength(0);
			int height = array.GetLength(1);

			uint retval = 0;

			foreach (var p in points) {
				retval <<= 1;

				Point ex = new Point(Clamp(x + p.X, 0, width), Clamp(y + p.Y, 0, height));

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
		public static string[] Arguments { get; private set; }
		public static bool HasArgument(string _arg) {
			return Arguments.Contains(_arg);
		}
		public static string RootPath { get; private set; }

		public static string GameName { get; private set; }

		static void Main(string[] _args) {
			Arguments = _args;
			RootPath = Directory.GetCurrentDirectory().Replace('/', '\\');

#if DEBUG
			RootPath = @"C:\Users\IsaGoodFriend\OneDrive\Documents\DevKitPro\Projects\PixtroGBA\Engine";
#endif

			if (RootPath.EndsWith("\\"))
				RootPath = RootPath.Substring(0, RootPath.Length - 1);

			GameName = System.IO.Path.GetFileName(RootPath);

			ArtCompiler.Compile(RootPath + "\\art");
			LevelCompiler.Compile(RootPath + "\\levels");

#if DEBUG
			//Console.WriteLine("Finished");
			//Console.ReadLine();
			//return;
#endif

			Process cmd = new Process();
			ProcessStartInfo info = new ProcessStartInfo();
			info.FileName = "cmd.exe";
			info.RedirectStandardInput = true;
			info.UseShellExecute = false;

			cmd.StartInfo = info;
			cmd.Start();

			using (StreamWriter sw = cmd.StandardInput) {
#if DEBUG
				sw.WriteLine("cd " + RootPath);
#endif

				sw.Write(@"C:\devkitPro\tools\bin\bin2s -H test.h ");

				foreach (var file in Directory.GetFiles(Path.Combine(RootPath, "art", "bin"))) {
					sw.Write(Path.Combine("art\\bin", Path.GetFileName(file)) + " ");
				}

				sw.WriteLine();

				Console.ReadLine();

				sw.WriteLine("cd " + RootPath);



				if (Arguments.Contains("-clean"))
					sw.WriteLine("make clean");
				else
					sw.WriteLine("make");

				if (Arguments.Contains("-run"))
					sw.WriteLine($"{GameName}.gba");

			}

			cmd.WaitForExit();
		}

		public static void Log(object log) {
			if (HasArgument("-log")) {
				Console.WriteLine(log.ToString());
			}
		}
	}
}