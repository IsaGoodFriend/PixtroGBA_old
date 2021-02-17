using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace GBA_Compiler
{
    public delegate int IndexOnSprite(int x, int y);
    public static class CompileArt
    {
        private static Dictionary<string, Color[]> palettesFromSprites = new Dictionary<string, Color[]>();

        public static void Compile(string _path)
        {
            bool needsRecompile = false;

            string toSavePath = Path.Combine(Compiler.Path, "source");

            long editTime = File.GetLastWriteTime(toSavePath + "\\sprites.c").Ticks;

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

            var compiler = new CompileToC();

            Compiler.Log("Compiling sprites");
            CompileSprites(Path.Combine(_path, "sprites"), compiler);

            Compiler.Log("Compiling palettes");
            CompilePalettes(Path.Combine(_path, "palettes"), compiler);

            Compiler.Log("Compiling particles");
            compiler.options |= CompileToC.CompileOptions.CompileEmptyArrays;
            CompileParticles(Path.Combine(_path, "particles"), compiler);
            compiler.options &= ~CompileToC.CompileOptions.CompileEmptyArrays;

            Compiler.Log("Compiling backgrounds");
            CompileBackgrounds(Path.Combine(_path, "backgrounds"), compiler);

            Compiler.Log("Saving art to file");
            compiler.SaveTo(toSavePath, "sprites");
        }

        private static void CompileSprites(string _path, CompileToC _compiler)
        {
            string[] getFiles = Directory.GetFiles(_path);

            foreach (string s in getFiles)
            {
                string ext = Path.GetExtension(s);

                string name = "SPR_" + Path.GetFileNameWithoutExtension(s);

                switch (ext)
                {
                    case ".bmp":
                        {
                            Bitmap map = new Bitmap(s);

                            List<Color> palette = new List<Color>(map.Palette.Entries);

                            palettesFromSprites.Add(name, palette.ToArray());

                            _compiler.BeginArray(CompileToC.ArrayType.UShort, name);


                            _compiler.AddRange(Enumerable.ToArray(GetArrayFromSprite(map.Width, map.Height,
                                (x, y) => { return palette.IndexOf(map.GetPixel(x, y)); } )));

                            _compiler.EndArray();

                            break;
                        }
                    case ".ase":
                        bool separateTags = true;
                        using (AsepriteReader read = new AsepriteReader(s))
                        {
                            if (separateTags)
                            {
                                foreach (var tag in read.Tags)
                                {
                                    _compiler.BeginArray(CompileToC.ArrayType.UShort, $"{name}_{tag.name}");

                                    foreach (var array in read.GetSprites(tag.name))
                                    {
                                        _compiler.AddRange(array);
                                    }

                                    _compiler.AddValueDefine($"{name}_{tag.name}__L", tag.end - tag.start + 1);
                                    _compiler.EndArray();
                                }
                            }
                            else
                            {
                                _compiler.BeginArray(CompileToC.ArrayType.UShort, $"{name}");

                                foreach (var array in read.GetSprites())
                                {
                                    _compiler.AddRange(array);
                                }

                                foreach (var tag in read.Tags)
                                {
                                    _compiler.AddValueDefine($"{name}_{tag.name}__S", tag.start * ((read.Width * read.Height) >> 2));
                                    _compiler.AddValueDefine($"{name}_{tag.name}__L", tag.end - tag.start + 1);
                                }

                                _compiler.EndArray();
                                
                            }
                        }
                        break;
                }


            }

        }
        private static void CompilePalettes(string _path, CompileToC _compiler)
        {
            string[] getFiles = Directory.GetFiles(_path);

            List<string> addedIn = new List<string>();

            foreach (string s in getFiles)
            {
                string ext = Path.GetExtension(s);

                if (ext != ".bmp" && ext != ".pal")
                {
                    continue;
                }

                string name = "PAL_" + Path.GetFileNameWithoutExtension(s);

                if (addedIn.Contains(name))
                    continue;

                addedIn.Add(name);

                switch (ext)
                {
                    case ".bmp":
                        {
                            Bitmap map = new Bitmap(s);

                            List<Color> palette = new List<Color>(map.Palette.Entries);

                            for (int i = 0; i < map.Height; ++i)
                            {
                                _compiler.BeginArray(CompileToC.ArrayType.UShort, name + (map.Height == 1 ? "" : "_" + i.ToString()));

                                for (int j = 0; j < 16; ++j)
                                {
                                    Color raw = map.GetPixel(j, i);

                                    _compiler.AddValue(raw.ToGBA());
                                }

                                _compiler.EndArray();
                            }


                            break;
                        }
                    case ".pal":
                        using (var sr = new StreamReader(File.Open(s, FileMode.Open)))
                        {
                            sr.ReadLine();
                            sr.ReadLine();

                            int count = int.Parse(sr.ReadLine());

                            for (int i = 0; i < count / 16; ++i)
                            {
                                _compiler.BeginArray(CompileToC.ArrayType.UShort, name + (count == 16 ? "" : "_" + i.ToString()));

                                for (int j = 0; j < 16; ++j)
                                {
                                    string[] read = sr.ReadLine().Split(' ');

                                    byte r = (byte)((int.Parse(read[0]) & 0xF8) >> 3);
                                    byte g = (byte)((int.Parse(read[1]) & 0xF8) >> 3);
                                    byte b = (byte)((int.Parse(read[2]) & 0xF8) >> 3);

                                    _compiler.AddValue((ushort)(r | (g << 5) | (b << 10)));
                                }

                                _compiler.EndArray();
                            }
                        }
                        break;
                }
            }
        }
        private static void CompileParticles(string _path, CompileToC _compiler)
        {
            string[] getFiles = Directory.GetFiles(_path);

            _compiler.BeginArray(CompileToC.ArrayType.UShort, "particles");

            foreach (string s in getFiles)
            {
                string ext = Path.GetExtension(s);

                string name = Path.GetFileNameWithoutExtension(s);

                int index = _compiler.ArrayLength;

                switch (ext)
                {
                    case ".bmp":
                        {
                            Bitmap map = new Bitmap(s);

                            List<Color> palette = new List<Color>(map.Palette.Entries);

                            int getIdx(int x, int y)
                            {
                                x = (x & 7) | (map.Width  - (x & ~0x7) - 8);
                                y = (y & 7) | (map.Height - (y & ~0x7) - 8);

                                return palette.IndexOf(map.GetPixel(x, y));
                            }

                            _compiler.AddRange(Enumerable.ToArray(GetArrayFromSprite(map.Width, map.Height, getIdx)));

                            _compiler.AddValueDefine($"PART_{name}", index);
                            _compiler.AddValueDefine($"PART_{name}_L", Math.Min((map.Width * map.Height) >> 6, 4));

                            break;
                        }
                    case ".ase":
                        using (AsepriteReader read = new AsepriteReader(s))
                        {
                            foreach (var tag in read.Tags)
                            {
                                foreach (var array in read.GetSprites(tag.name))
                                {
                                    _compiler.AddRange(array);
                                }

                                _compiler.AddValueDefine($"PART_{tag.name}", index);
                                _compiler.AddValueDefine($"PART_{tag.name}_L", Math.Min(tag.end - tag.start + 1, 4));
                            }

                            break;
                        }
                }

            }

            _compiler.EndArray();

        }
        private static void CompileBackgrounds(string _path, CompileToC _compiler)
        {
            string[] getFiles = Directory.GetFiles(_path);

            foreach (string s in getFiles)
            {
                string ext = Path.GetExtension(s);

                string name = Path.GetFileNameWithoutExtension(s);

                switch (ext)
                {
                    case ".bmp":
                        {
                            Bitmap map = new Bitmap(s);

                            List<Color> palette = new List<Color>(map.Palette.Entries);

                            palettesFromSprites.Add(name, palette.ToArray());

                            _compiler.BeginArray(CompileToC.ArrayType.UShort, "BGT_" + name);



                            _compiler.AddRange(Enumerable.ToArray(GetArrayFromSprite(map.Width, map.Height,
                                (x, y) => { return palette.IndexOf(map.GetPixel(x, y)); })));

                            _compiler.EndArray();

                            break;
                        }
                    case ".ase":
                        break;
                }
            }
        }

        public static IEnumerable<ushort> GetArrayFromSprite(int _width, int _height, IndexOnSprite _values)
        {
            
            for (int yL = 0; yL < _height >> 3; ++yL)
            {
                for (int xL = 0; xL < _width >> 3; ++xL)
                {
                    for (int y = 0; y < 8; ++y)
                    {
                        ushort tempValue = 0;
                        for (int i = 3; i >= 0; --i)
                            tempValue = (ushort)((tempValue << 4) | _values((xL << 3) + i, (yL << 3) + y));

                        yield return tempValue;

                        for (int i = 7; i >= 4; --i)
                            tempValue = (ushort)((tempValue << 4) | _values((xL << 3) + i, (yL << 3) + y));

                        yield return tempValue;
                    }
                }
            }
        }
    }
}
