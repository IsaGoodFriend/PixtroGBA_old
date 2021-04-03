using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GBA_Compiler {

	public class BinNode {
		public string Name;
		public Dictionary<string, object> Attributes;
		public List<BinNode> Children;

		public object this[string name] {
			get { return Attributes[name]; }
		}

        public BinNode GetChild(string _name) {
            foreach (var child in Children)
                if (child.Name == _name)
                    return child;

            throw new Exception();
		}

        public int GetInteger(string _name) {
            var obj = Attributes[_name];

            switch (obj) {
                case int i:
                    return i;
                case short i:
                    return i;
                case byte i:
                    return i;
                case long i:
                    return (int)i;
                case uint i:
                    return (int)i;
                case ushort i:
                    return i;
                case sbyte i:
                    return i;
                case ulong i:
                    return (int)i;
                case float i:
                    return (int)i;
                case double i:
                    return (int)i;
            }

            throw new Exception();
        }
        public float GetFloat(string _name) {
            var obj = Attributes[_name];

            switch (obj) {
                case int i:
                    return i;
                case short i:
                    return i;
                case byte i:
                    return i;
                case long i:
                    return i;
                case uint i:
                    return i;
                case ushort i:
                    return i;
                case sbyte i:
                    return i;
                case ulong i:
                    return i;
                case float i:
                    return i;
                case double i:
                    return (float)i;
            }

            throw new Exception();
        }
    }

	public class LevelBinReader {
		

		string[] textLookup;
		public List<BinNode> Nodes;

		public string Header { get; private set; }

		public LevelBinReader(string _path, string headerRequired = null) {
            BinaryReader reader = new BinaryReader(File.Open(_path, FileMode.Open));

			Header = reader.ReadString();

			if (headerRequired != null && Header != headerRequired) {
				reader.Dispose();
				throw new FileLoadException();
			}

			textLookup = new string[reader.ReadUInt16()];

			for (int i = 0; i < textLookup.Length; ++i) {
				textLookup[i] = reader.ReadString();
			}

			Nodes = new List<BinNode>(GetNodes(reader));

            reader.Dispose();
		}

		private IEnumerable<BinNode> GetNodes(BinaryReader reader) {

            int count = reader.ReadUInt16();

            for (int i = 0; i < count; ++i) {

				BinNode retval = new BinNode {
					Name = textLookup[reader.ReadUInt16()]
				};
				int attrCount =  reader.ReadByte();
                
                for (int j = 0; j < attrCount; ++j) {
                    var attrName = textLookup[reader.ReadInt16()];

                    byte b = reader.ReadByte();
                    object value = null;

                    switch (b) {
                        case 0:
                            value = reader.ReadBoolean();
                            break;
                        case 1:
                            value = reader.ReadByte();
                            break;
                        case 2:
                            value = reader.ReadInt16();
                            break;
                        case 3:
                            value = reader.ReadInt32();
                            break;
                        case 4:
                            value = reader.ReadSingle();
                            break;
                        case 5:
                            value = textLookup[reader.ReadInt16()];
                            break;
                        case 6:
                            value = reader.ReadString();
                            break;
                        case 7:

                            StringBuilder builder = new StringBuilder();
                            short bytesCount = reader.ReadInt16();
                            for (short ind = 0; ind < bytesCount; ind += 2) {
                                byte repeatingCount = reader.ReadByte();
                                char character = (char)reader.ReadByte(); // Direct cast
                                builder.Append(character, repeatingCount);
                            }
                            value = builder.ToString();

                            break;
                        case 8:
                            value = reader.ReadInt64();
                            break;
                        case 9:
                            value = reader.ReadDouble();
                            break;
                    }
                    retval.Attributes.Add(attrName, value);
                }
                List<BinNode> children = new List<BinNode>();

                children.AddRange(GetNodes(reader));

                yield return retval;
            }

			yield break;
		}

	}
}