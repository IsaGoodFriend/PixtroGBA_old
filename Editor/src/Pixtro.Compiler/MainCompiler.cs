using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using Newtonsoft.Json;

namespace Pixtro.Compiler {
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

		internal static class Settings
		{
			public static bool Clean { get; set; }

			public static string ProjectPath { get; set; }
			public static string EnginePath { get; set; }
			public static string GamePath { get; set; }

			public static bool LargeTiles { get; set; }
			public static bool DeleteGame { get; set; }

			public static void SetInitialArguments()
			{
				LargeTiles = false;
				DeleteGame = false;
				Clean = false;
			}
			public static void SetArguments(string[] args)
			{
				for (int i = 0; i < args.Length; ++i)
				{
					string[] arg = args[i].Split('=');

					switch (arg[0])
					{
						case "-c":
							Clean = true;

							break;
						case "-d":
							DeleteGame = true;

							break;
						case "-p":
						case "--projectPath":
							ProjectPath = arg.Length > 1 ? arg[1] : args[++i];
							
							break;
						case "-g":
						case "--outputPath":
						case "--gamePath":
							GamePath = arg.Length > 1 ? arg[1] : args[++i];

							break;
						case "-e":
						case "--enginePath":
							EnginePath = arg.Length > 1 ? arg[1] : args[++i];

							break;
					}
				}

			}
			public static void SetFolders()
			{
				if (ProjectPath.EndsWith("\\"))
					ProjectPath = ProjectPath.Substring(0, ProjectPath.Length - 1);
				if (EnginePath.EndsWith("\\"))
					EnginePath = ProjectPath.Substring(0, EnginePath.Length - 1);
				if (GamePath.EndsWith("\\"))
					GamePath = ProjectPath.Substring(0, GamePath.Length - 1);
			}
		}

		private static bool Error;


		public static void Main(string[] _args) {
			Compile(Directory.GetCurrentDirectory(), _args);
		}

		public static void Compile(string projectPath, string args)
		{
			string[] argSplit = args.Split(new char[]{ ' ' }, StringSplitOptions.RemoveEmptyEntries);

			Compile(projectPath, argSplit);
		}

		public static void Compile(string projectPath, string[] _args) {

			Console.Clear();

			Settings.SetInitialArguments();
			Settings.ProjectPath = Settings.GamePath = projectPath.Replace('/', '\\');
			Settings.EnginePath = Path.Combine(Settings.ProjectPath, "");
			Settings.GamePath = Path.Combine(Settings.GamePath, Path.GetFileName(Settings.GamePath) + ".gba");

			Settings.SetArguments(_args);

			// Make sure directory for build sources exists
			Directory.CreateDirectory(Path.Combine(Settings.ProjectPath, "build/source"));
			
			// Check the engine.h header file for information on how to compile level (and other data maybe in the future idk)
			//foreach (string s in File.ReadAllLines(Path.Combine(RootPath, @"source\engine.h"))) {
			//	if (s.StartsWith("#define")) {
			//		string removeComments = s;
			//		if (removeComments.Contains("/"))
			//			removeComments = removeComments.Substring(0, removeComments.IndexOf('/'));

			//		string[] split = removeComments.Replace('\t', ' ').Split(new char[] {' ' }, StringSplitOptions.RemoveEmptyEntries);

			//		switch (split[1]) {
			//			case "LARGE_TILES":
			//				LargeTiles = true;
			//				break;
			//		}
			//	}
			//}


			if (Settings.DeleteGame && File.Exists(Settings.GamePath)) {
				File.Delete(Settings.GamePath);
			}

			ArtCompiler.Compile(Settings.ProjectPath + "\\art");
			LevelCompiler.Compile(Settings.ProjectPath + "\\levels", Settings.ProjectPath + "\\art\\tilesets");

			if (Error)
				return;

			Process cmd = new Process();
			ProcessStartInfo info = new ProcessStartInfo();
			info.FileName = "cmd.exe";
			info.RedirectStandardInput = true;
			info.UseShellExecute = false;

			cmd.StartInfo = info;
			cmd.Start();

			using (StreamWriter sw = cmd.StandardInput) {

				if (Settings.Clean)
					sw.WriteLine("make clean");
				else
					sw.WriteLine($"make -C {Settings.ProjectPath} -f {Settings.EnginePath}/Makefile");

				//if (Arguments.Contains("-run"))
				//	sw.WriteLine($"{GameName}.gba");
			}

			cmd.WaitForExit();
		}

		public static void ErrorLog(object log) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("ERROR -- ");
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine(log.ToString());

			Error = true;
		}
		public static void WarningLog(object log) {
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("WARNING -- ");
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine(log.ToString());
		}
		public static void Log(object log) {
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine(log.ToString());
			
		}
		public static void DebugLog(object log) {
			//if (HasArgument("-log")) {
			//	Console.ForegroundColor = ConsoleColor.White;
			//	Console.WriteLine(log.ToString());
			//}
		}
	}
}