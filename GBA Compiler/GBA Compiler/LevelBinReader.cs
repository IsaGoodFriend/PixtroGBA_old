using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GBA_Compiler {
	public class LevelBinReader : IDisposable {
		BinaryReader reader;

		public LevelBinReader(string _path) {

			reader = new BinaryReader(File.Open(_path, FileMode.Open));

			var signature = reader.ReadString();

			if (signature != "PIX  GBA") {
				reader.Dispose();
				throw new FileLoadException();
			}
		}

		public void Dispose() {
			reader.Dispose();
		}
	}
}