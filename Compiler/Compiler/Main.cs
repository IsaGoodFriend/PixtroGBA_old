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

		private static bool Error;

		static void Main(string[] _args) {

			Arguments = _args;
			RootPath = Directory.GetCurrentDirectory().Replace('/', '\\');

#if DEBUG
			RootPath = @"C:\Users\IsaGoodFriend\source\HomeBrew\PixtroGBA\Engine";
#endif
			// Make sure directory for build sources exists
			Directory.CreateDirectory(Path.Combine(RootPath, "build/source"));
			
			// Check the engine.h header file for information on how to compile level (and other data maybe in the future idk)
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

			if (Error)
				return;

			using (StreamWriter sw = cmd.StandardInput) {
				if (Arguments.Contains("-clean"))
					sw.WriteLine("make clean");
				else
					sw.WriteLine("make");

				if (Arguments.Contains("-run"))
					sw.WriteLine($"{GameName}.gba");
			}

			cmd.WaitForExit();
		}

		public static void ErrorLog(object log) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("ERROR -- ");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(log.ToString());

			Error = true;
		}
		public static void WarningLog(object log) {
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("WARNING -- ");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(log.ToString());
		}
		public static void Log(object log) {
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(log.ToString());
			
		}
		public static void DebugLog(object log) {
			if (HasArgument("-log")) {
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine(log.ToString());
			}
		}
	}
}