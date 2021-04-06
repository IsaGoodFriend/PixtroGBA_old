using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Text;

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

			entGlobalCount = 0;
			entSectionCount = new Dictionary<LevelParse, int>();
			typeLocalCount = new Dictionary<string, int>();
			typeGlobalCount = new Dictionary<string, int>();
			typeSectionCount = new Dictionary<LevelParse, Dictionary<string, int>>();

			List<LevelParse> parseData = JsonConvert.DeserializeObject<List<LevelParse>>(File.ReadAllText(_path + "\\meta_level.json"));
			foreach (var p in parseData) {
				foreach (char c in p.Wrapping.Keys)
					p.Wrapping[c].FinalizeMasks();
			}

			CompressedLevel.Randomizer = new Random();

			foreach (var level in Directory.GetFiles(_path, "*", SearchOption.AllDirectories)) {
				var ext = Path.GetExtension(level);

				var localPath = level.Replace(_path, "").Replace('/', '\\');
				if (localPath.StartsWith("\\"))
					localPath = localPath.Substring(1);

				if (localPath.StartsWith("meta"))
					continue;

				CompressedLevel compressed = null;

				foreach (var p in parseData) {
					if (p.Matches(localPath)) {
						CompressedLevel.DataParse = p;
						if (!typeSectionCount.ContainsKey(p)) {
							typeSectionCount.Add(p, new Dictionary<string, int>());
							entSectionCount.Add(p, 0);
						}
						break;
					}
				}

				entLocalCount = 0;
				typeLocalCount.Clear();

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

				compiler.BeginArray(CompileToC.ArrayType.Char, $"LVL_{Path.GetFileNameWithoutExtension(localPath)}");

				compiler.AddRange(compressed.BinaryData());

				compiler.EndArray();
			}

			foreach (var parse in parseData) {
				if (parse.fullTileset == null)
					return;
				
				compiler.BeginArray(CompileToC.ArrayType.UInt, "TILESET_" + parse.Name);
				
				foreach (var tile in parse.fullTileset){
					compiler.AddRange(tile.RawData);
				}

				compiler.EndArray();

				compiler.AddValueDefine($"TILESET_{parse.Name}_len", (parse.fullTileset.Count) * (Compiler.LargeTiles ? 4 : 1));
			}

			compiler.SaveTo(toSavePath, "levels");
		}


		private static string currentType;

		private static int entGlobalCount, entLocalCount;
		private static Dictionary<LevelParse, int> entSectionCount;

		private static Dictionary<string, int> typeGlobalCount, typeLocalCount;
		private static Dictionary<LevelParse, Dictionary<string, int>> typeSectionCount;

		private static string NextLine(StreamReader _reader) {

			string retval;
			do
				retval = _reader.ReadLine();
			while (string.IsNullOrWhiteSpace(retval));

			return retval;
		}
		private static string[] SplitWithTrim(string str, char splitChar) {
			string[] split = str.Split(new char[]{ splitChar }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < split.Length; ++i) {
				split[i] = split[i].Trim();
			}

			return split;
		}


		private static byte ParseEntityMeta(string _value) {
			byte retval;

			if (byte.TryParse(_value, out retval))
				return retval;

			float parsedValue = 0;

			var checkEach = DataParser.MetaSplit(_value);

			MetaOperations currentOp = MetaOperations.Add;

			while (checkEach.MoveNext()) {
				var parsed = checkEach.Current;

				string[] split = parsed.Split('.');
				float tempValue = 0;

				switch (split[0]) {
					case "ent": {

						switch (split[1]) {
							case "globalcount":
								tempValue = entGlobalCount;
								break;
							case "localcount":
								tempValue = entLocalCount;
								break;
							case "sectioncount":
								tempValue = entSectionCount[CompressedLevel.DataParse];
								break;
						}

						break;
					}
					case "type": {

						switch (split[1]) {
							case "globalcount":
								tempValue = typeGlobalCount[currentType];
								break;
							case "localcount":
								tempValue = typeLocalCount[currentType];
								break;
							case "sectioncount":
								tempValue = typeSectionCount[CompressedLevel.DataParse][currentType];
								break;
						}

						break;
					}

					case "+":
						currentOp = MetaOperations.Add;
						break;
					case "-":
						currentOp = MetaOperations.Subtract;
						break;
					case "*":
						currentOp = MetaOperations.Multiply;
						break;
					case "/":
						currentOp = MetaOperations.Divide;
						break;
					default: {

						if (float.TryParse(parsed, out tempValue)) {
							break;
						}

						throw new Exception();
					}
				}

				switch (currentOp) {
					case MetaOperations.Add:
						parsedValue += tempValue;
						break;
					case MetaOperations.Subtract:
						parsedValue -= tempValue;
						break;
					case MetaOperations.Multiply:
						parsedValue *= tempValue;
						break;
					case MetaOperations.Divide:
						parsedValue /= tempValue;
						break;
				}
			}

			retval = (byte)parsedValue;

			return retval;
		}
		private static CompressedLevel CompileLevelBin(string _path, CompileToC _compiler) {

			// TODO: Add support for this at some point.

			var reader = new LevelBinReader(_path, "PIXTRO_LVL");

			string baseName = Path.GetFileNameWithoutExtension(_path);

			foreach (var node in reader.Nodes) {
				switch (node.Name) {
					case "level":
						if (node.Children[0].Name != "meta")
							continue;

						CompressedLevel level = new CompressedLevel();
						string levelName = null;

						foreach (var child in node.Children) {
							switch (child.Name) {
								case "meta":
									level.Width = child.GetInteger("width");
									level.Height = child.GetInteger("height");
									level.Layers = child.GetInteger("layers");
									levelName = child["name"] as string;
									break;
								case "layer":
									int layerIndex = child.GetInteger("index");
									string[] values = (child["data"] as string).Split('\n');

									for (int i = 0; i < values.Length; ++i) {
										level.AddLine(layerIndex, i, values[i]);
									}
									break;
								case "entity":

									var ent = new CompressedLevel.Entity();

									ent.x = child.GetInteger("x");
									ent.y = child.GetInteger("y");
									if (child.Attributes["type"] is string) {
										ent.type = CompressedLevel.DataParse.EntityIndex[child.Attributes["type"] as string];
									}
									else {
										ent.type = (byte)child.GetInteger("type");
									}

									level.entities.Add(ent);

									foreach (var attr in child.Attributes.Keys) {
										switch (attr) {
											case "x":
											case "y":
											case "name":
											case "type":
												break;
											default:
												if (child.Attributes[attr] is string) {
													ent.data.Add(ParseEntityMeta(child.Attributes[attr] as string));
												}
												else {
													ent.data.Add((byte)child.GetInteger(attr));
												}

												break;
										}

									}

									entLocalCount++;
									entGlobalCount++;
									entSectionCount[CompressedLevel.DataParse]++;

									currentType = child["name"] as string;

									if (!typeLocalCount.ContainsKey(currentType)) {
										typeLocalCount.Add(currentType, 0);
										typeGlobalCount.Add(currentType, 0);
										typeSectionCount[CompressedLevel.DataParse].Add(currentType, 0);
									}
									typeLocalCount[currentType]++;
									typeGlobalCount[currentType]++;
									typeSectionCount[CompressedLevel.DataParse][currentType]++;


									break;
							}
						}

						_compiler.BeginArray(CompileToC.ArrayType.Char, $"LVL_{baseName}_{levelName}");

						_compiler.AddRange(level.BinaryData());

						_compiler.EndArray();

						break;

					case "meta":
						break;
				}
			}

			return null;
		}
		private static CompressedLevel CompileLevelTxt(string _path) {
			CompressedLevel retval = new CompressedLevel();

			using (StreamReader reader = new StreamReader(File.Open(_path, FileMode.Open))) {
				string[] split = SplitWithTrim(NextLine(reader), '-');

				retval.Width = int.Parse(split[0]);
				retval.Height = int.Parse(split[1]);
				retval.Layers = int.Parse(split[2]);

				while (!reader.EndOfStream) {
					string dataType = NextLine(reader);
					split = SplitWithTrim(dataType, '-');

					switch (split[0]) {
						case "layer": {
							int line = dataType.Contains('-') ? int.Parse(split[1]) : 0;

							for (int i = 0; i < retval.Height; ++i) {
								retval.AddLine(line, i, NextLine(reader));
							}
							break;
						}
						case "entities":
							string ent = "";

							while (ent != "end") {
								ent = NextLine(reader);

								if (ent == "end")
									break;

								split = SplitWithTrim(ent, ';');

								var entity = new CompressedLevel.Entity();

								entity.x = int.Parse(split[1]);
								entity.y = int.Parse(split[2]);

								for (int i = 3; i < split.Length; ++i) {
									entity.data.Add(ParseEntityMeta(split[i]));
								}

								entity.type = CompressedLevel.DataParse.EntityIndex[split[0]];

								retval.entities.Add(entity);

								entLocalCount++;
								entGlobalCount++;
								entSectionCount[CompressedLevel.DataParse]++;

								currentType = split[0];

								if (!typeLocalCount.ContainsKey(currentType)) {
									typeLocalCount.Add(currentType, 0);
									typeGlobalCount.Add(currentType, 0);
									typeSectionCount[CompressedLevel.DataParse].Add(currentType, 0);
								}
								typeLocalCount[currentType]++;
								typeGlobalCount[currentType]++;
								typeSectionCount[CompressedLevel.DataParse][currentType]++;

								
							}
							break;
					}

				}
			}

			return retval;
		}
	}
}