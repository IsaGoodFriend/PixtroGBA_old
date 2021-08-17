using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using Newtonsoft.Json;

namespace Pixtro.Compiler
{
	public struct FloatColor
	{
		public float R, G, B, A;

		public FloatColor(byte r, byte g, byte b, byte a)
		{
			R = r / 255f;
			G = g / 255f;
			B = b / 255f;
			A = a / 255f;

			if (A == 0)
			{
				R = 0;
				G = 0;
				B = 0;
			}
		}

		public static FloatColor FlattenColor(FloatColor colorA, FloatColor colorB, BlendType blend)
		{
			FloatColor color = colorA;

			if (colorB.A <= 0)
				return colorA;

			switch (blend)
			{
				case BlendType.Normal:
					color.R = colorB.R;
					color.G = colorB.G;
					color.B = colorB.B;
					color.A = Math.Max(colorA.A, colorB.A);
					break;
			}

			return color;
		}

		public ushort ToGBA(ushort _transparent = 0x8000)
		{
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
		public Color ToGBAColor()
		{
			if (A <= 0.5f)
			{
				return Color.FromArgb(0, 0, 0, 0);
			}

			byte r = (byte)(Math.Floor(R * 31) * 4);
			byte g = (byte)(Math.Floor(G * 31) * 4);
			byte b = (byte)(Math.Floor(B * 31) * 4);

			return Color.FromArgb(255, r, g, b);
		}

		public override string ToString()
		{
			return $"{{{R:0.00} - {G:0.00} - {B:0.00} :: {A:0.00}}}";
		}
	}
	internal static class Settings
	{
		public static bool Clean { get; set; }

		public static string ProjectPath { get; set; }
		public static string EnginePath { get; set; }
		public static string GamePath { get; set; }

		public static int BrickTileSize { get; set; }

		public static void SetInitialArguments(string[] args)
		{
			BrickTileSize = 1;
			Clean = false;

			for (int i = 0; i < args.Length; ++i)
			{
				string[] arg = args[i].Split('=');
				switch (arg[0])
				{
					case "-p":
					case "--projectPath":
						ProjectPath = arg.Length > 1 ? arg[1] : args[++i];
						return;
				}
			}
		}
		public static void SetArguments(string[] args)
		{
			for (int i = 0; i < args.Length; ++i)
			{
				string[] arg = args[i].Split('=');

				string exArg () => arg.Length > 1 ? arg[1] : args[++i];

				switch (arg[0])
				{
					case "-t":
					case "--brickSize":
						BrickTileSize = int.Parse(exArg());

						break;
					case "-c":
					case "--clean":
						Clean = true;

						break;
					case "-p":
					case "--projectPath":
						ProjectPath = exArg();

						break;
					case "-g":
					case "--outputPath":
					case "--gamePath":
						GamePath = exArg();

						break;
					case "-e":
					case "--enginePath":
						EnginePath = exArg();

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
	public class PointConverter : JsonConverter<Point> {
		public override Point ReadJson(JsonReader reader, Type objectType, Point existingValue, bool hasExistingValue, JsonSerializer serializer) {
			
			var points = (reader.Value as string).Split(',');

			return new Point(int.Parse(points[0].Trim()), int.Parse(points[1].Trim()));
		}

		public override void WriteJson(JsonWriter writer, Point value, JsonSerializer serializer) {
		}
	}
	public static class Compiler {

		private static bool Error;


		public static void Main(string[] _args) {
			Compile(Directory.GetCurrentDirectory(), _args);
		}

		public static void Compile(string projectPath, string args)
		{
			string[] argSplit = args.Split(new char[]{ ' ' }, StringSplitOptions.RemoveEmptyEntries);

			Compile(projectPath, argSplit);
		}

		public static void Compile(string projectPath, string[] args) {


			Console.Clear();

			Settings.SetInitialArguments(args);
			Settings.ProjectPath = Settings.GamePath = projectPath.Replace('/', '\\');
			Settings.EnginePath = Path.Combine(Settings.ProjectPath, "");
			Settings.GamePath = Path.Combine(Settings.GamePath, Path.GetFileName(Settings.GamePath) + ".gba");

			// Check the engine.h header file for information on how to compile level (and other data maybe in the future idk)
			foreach (string s in File.ReadAllLines(Path.Combine(Settings.ProjectPath, @"source\engine.h")))
			{
				if (s.StartsWith("#define"))
				{
					string removeComments = s;
					if (removeComments.Contains("/"))
						removeComments = removeComments.Substring(0, removeComments.IndexOf('/'));

					string[] split = removeComments.Replace('\t', ' ').Split(new char[] {' ' }, StringSplitOptions.RemoveEmptyEntries);

					switch (split[1])
					{
						case "LARGE_TILES":
							Settings.BrickTileSize = 2;
							break;
					}
				}
			}

			Settings.SetArguments(args);

			// Make sure directory for build sources exists
			Directory.CreateDirectory(Path.Combine(Settings.ProjectPath, "build/source"));

			if (Settings.Clean)
			{
				// Todo : Add cleaning functionality
			}
			else
			{
				try
				{
					FullCompiler.Compile();
				}
				catch (Exception e)
				{
					ErrorLog(e);
					Error = true;
				}
			}

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

				sw.WriteLine($"make -C {Settings.ProjectPath} -f {Settings.EnginePath}/Makefile {(Settings.Clean ? "clean" : "")}");

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