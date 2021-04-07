using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using Newtonsoft.Json;

namespace GBA_Compiler {
	public class PointConverter : JsonConverter<Point> {
		public override Point ReadJson(JsonReader reader, Type objectType, Point existingValue, bool hasExistingValue, JsonSerializer serializer) {
			
			var points = (reader.Value as string).Split(',');

			return new Point(int.Parse(points[0].Trim()), int.Parse(points[1].Trim()));
		}

		public override void WriteJson(JsonWriter writer, Point value, JsonSerializer serializer) {
		}
	}
	[JsonConverter(typeof(PointConverter))]
	public struct Point {
		public int X, Y;

		public Point(int x, int y) { X = x; Y = y; }
	}
	public static class Compiler {
		
		public static bool LargeTiles { get; private set; }
		
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
			RootPath = @"C:\Users\IsaGoodFriend\source\HomeBrew\PixtroGBA\Engine";
#endif

			foreach (string s in File.ReadAllLines(Path.Combine(RootPath, @"source\engine.h"))) {
				if (s.StartsWith("#define")) {
					string removeComments = s;
					if (removeComments.Contains("/"))
						removeComments = removeComments.Substring(0, removeComments.IndexOf('/'));

					string[] split = removeComments.Replace('\t', ' ').Split(new char[] {' ' }, StringSplitOptions.RemoveEmptyEntries);

					switch (split[1]) {
						case "LARGE_TILES":
							LargeTiles = true;
							break;
					}
				}
			}

			if (RootPath.EndsWith("\\"))
				RootPath = RootPath.Substring(0, RootPath.Length - 1);

			GameName = System.IO.Path.GetFileName(RootPath);

			if (Arguments.Contains("-d")) {
				File.Delete(GameName + ".gba");
			}

			ArtCompiler.Compile(RootPath + "\\art");
			LevelCompiler.Compile(RootPath + "\\levels", RootPath + "\\art\\tilesets");

#if DEBUG
			Console.WriteLine("Finished");
			//Console.ReadLine();
			return;
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

				//sw.Write(@"C:\devkitPro\tools\bin\bin2s -H test.h ");

				//foreach (var file in Directory.GetFiles(Path.Combine(RootPath, "art", "bin"))) {
					//sw.Write(Path.Combine("art\\bin", Path.GetFileName(file)) + " ");
				//}

				//sw.WriteLine();
				//return;

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