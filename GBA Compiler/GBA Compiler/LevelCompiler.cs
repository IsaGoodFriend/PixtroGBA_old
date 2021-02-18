using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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

			List<LevelParse> parseData = JsonConvert.DeserializeObject<List<LevelParse>>(File.ReadAllText(_path + "\\meta_level.json"));
			CompressedLevel.Randomizer = new Random();

			foreach (var level in Directory.GetFiles(_path, "*", SearchOption.AllDirectories)) {
				var ext = Path.GetExtension(level);

				var localPath = level.Replace(_path, "").Replace('/', '\\');
				if (localPath.StartsWith("\\"))
					localPath = localPath.Substring(1);

				if (localPath.StartsWith("meta"))
					continue;

				CompressedLevel compressed = null;

				switch (ext) {
					case ".txt":
						compressed = CompileLevelTxt(level);
						break;
					case ".json": // Ogmo editor
						throw new NotImplementedException();
					default: // Compressed Binary File
						CompileLevelBin(level, compiler);
						break;
				}

				if (compressed == null)
					continue;
				
				foreach (var p in parseData) {
					if (p.Matches(localPath)) {
						CompressedLevel.DataParse = p;
						break;
					}
				}

				compiler.BeginArray(CompileToC.ArrayType.Char, $"LVL_{Path.GetFileNameWithoutExtension(localPath)}");

				compiler.AddRange(compressed.BinaryData());

				compiler.EndArray();
			}

			compiler.SaveTo(toSavePath, "levels");
		}
		private static CompressedLevel CompileLevelBin(string _path, CompileToC _compiler) {

			// TODO: Add support for this at some point.

			throw new NotImplementedException();

			//using (BinaryReader reader = new BinaryReader(File.Open(_path, FileMode.Open))){


			//	string name = reader.ReadString();

			//	while (!string.IsNullOrWhiteSpace(name)) {

			//		CompressedLevel level = new CompressedLevel();

			//		level.Width = reader.ReadByte();
			//		level.Height = reader.ReadByte();
			//		level.

			//		_compiler.BeginArray(CompileToC.ArrayType.Char, $"LVL_{name}");
			//		_compiler.AddRange(level.BinaryData());
			//		_compiler.EndArray();

			//		name = reader.ReadString();
			//	}


			//}

			//return null;
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

							for (int i = 0; i < retval.Height; ++i) {
								retval.AddLine(line, i, reader.ReadLine());
							}
							break;
						}
						case "entities":
							string ent = "";

							while (ent != "end") {
								ent = reader.ReadLine();

								if (ent == "end" || string.IsNullOrWhiteSpace(ent))
									continue;

								string[] split = ent.Split(';');

								var entity = new CompressedLevel.Entity();

								entity.type = int.Parse(split[0].Trim());
								entity.x = int.Parse(split[1].Trim());
								entity.y = int.Parse(split[2].Trim());

								for (int i = 3; i < split.Length; ++i) {
									entity.data.Add(byte.Parse(split[i]));
								}

								retval.entities.Add(entity);
							}
							break;
					}

				}
			}

			return retval;
		}
	}
}