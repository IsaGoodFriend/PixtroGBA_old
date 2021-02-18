using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace GBA_Compiler {
	public static class LevelCompiler {
		public static void Compile(string _path) {
			string toSavePath = Path.Combine(Compiler.RootPath, "source/engine");

#if !DEBUG
			bool needsRecompile = false;

			long editTime = File.GetLastWriteTime(toSavePath + "\\levels.c").Ticks;

			foreach (var file in Directory.GetFiles(_path, "*", SearchOption.AllDirectories))
			{
				if (File.GetLastWriteTime(file).Ticks > editTime)
				{
					needsRecompile = true;
					break;
				}
			}
			if (!needsRecompile)
			{
				Compiler.Log("Skipping sprite compiling");
				return;
			}
#endif
			var compiler = new CompileToC();

			foreach (var level in Directory.GetFiles(_path, "*", SearchOption.AllDirectories)) {
				var ext = Path.GetExtension(level);

				CompressedLevel compressed = null;

				switch (ext) {
					case ".txt":
						CompileLevelTxt(level);
						break;
					case ".json": // Ogmo editor
						break;
					default: // Compressed Binary File
						break;
				}

				if (compressed == null)
					continue;

				var loc = level.Replace(_path, "").Replace('/', '\\');
				if (loc.StartsWith("\\"))
					loc = loc.Substring(1);

				compiler.BeginArray(CompileToC.ArrayType.Char, $"LVL_{loc}");

				compiler.AddRange(compressed.BinaryData());

				compiler.EndArray();
			}

			compiler.SaveTo(toSavePath, "levels");
		}

		private static CompressedLevel CompileLevelTxt(string _path) {
			CompressedLevel retval = new CompressedLevel();

			using (StreamReader reader = new StreamReader(File.Open(_path, FileMode.Open))) {
				string[] header = reader.ReadLine().Split('-');

				retval.Width = int.Parse(header[0].Trim());
				retval.Height = int.Parse(header[1].Trim());
				retval.Layers = int.Parse(header[2].Trim());

				while (!reader.EndOfStream) {
					string dataType = reader.ReadLine();

					while (string.IsNullOrWhiteSpace(dataType))
						dataType = reader.ReadLine();

					switch (dataType.Split('-')[0].Trim()) {
						case "layer": {
							int line = dataType.Contains('-') ? int.Parse(dataType.Split('-')[1].Trim()) : 0;

							for (int i = 0; i < retval.Height; ++i){
								retval.AddLine(line, i, reader.ReadLine());
							}
							break;
						}
						case "entities":
							string ent = "";


							while (ent != "end") {
								ent = reader.ReadLine();

								if (ent == "ent" || string.IsNullOrWhiteSpace(ent)) continue;


							}
							break;
					}

				}
			}

			return retval;
		}
	}
}