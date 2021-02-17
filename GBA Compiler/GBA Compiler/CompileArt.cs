using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace GBA_Compiler
{
    public delegate uint IndexOnSprite(int x, int y);
    public static class CompileArt
    {
        private static T GetXY<T>(this T[] _array, int x, int y, int width)
        {
            return _array[x + (y * width)];
        }
        private static void SetXY<T>(this T[] _array, int x, int y, int width, T value)
        {
            _array[x + (y * width)] = value;
        }
        private static void Flip<T>(this T[] _array, bool X, int width)
        {
            int height = _array.Length / width;

            if (height * width != _array.Length)
                return;

            if (X)
            {
                for (int x = 0; x < width / 2; ++x)
                {
                    for (int y = 0; y < height; ++y)
                    {
                        int otherX = width - x - 1;

                        T temp = _array.GetXY(x, y, width);

                        _array.SetXY(x, y, width, _array.GetXY(otherX, y, width));

                        _array.SetXY(otherX, y, width, temp);
                    }
                }
            }
            else
            {
                for (int y = 0; y < height / 2; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        int otherY = height - y - 1;

                        T temp = _array.GetXY(x, y, width);

                        _array.SetXY(x, y, width, _array.GetXY(x, otherY, width));

                        _array.SetXY(x, otherY, width, temp);
                    }
                }
            }
        }

        private class Tile
        {
            public enum FlipStyle { X = 1, Y = 2, None = 0}
            private byte[] bitData;
            private uint[] rawData;
            private FlipStyle flipped;

            public uint[] RawData => rawData;

            public Tile(uint[] _GBA, int _index)
            {
                bitData = new byte[64];
                rawData = new uint[8];

                for (int i = 0; i < 8; ++i)
                {
                    rawData[i] = _GBA[i + _index];

                    bitData[i << 3] =       (byte) (_GBA[i + _index] & 0x0000000F);
                    bitData[(i << 3) + 1] = (byte)((_GBA[i + _index] & 0x000000F0) >> 4);
                    bitData[(i << 3) + 2] = (byte)((_GBA[i + _index] & 0x00000F00) >> 8);
                    bitData[(i << 3) + 3] = (byte)((_GBA[i + _index] & 0x0000F000) >> 12);
                    bitData[(i << 3) + 4] = (byte)((_GBA[i + _index] & 0x000F0000) >> 16);
                    bitData[(i << 3) + 5] = (byte)((_GBA[i + _index] & 0x00F00000) >> 20);
                    bitData[(i << 3) + 6] = (byte)((_GBA[i + _index] & 0x0F000000) >> 24);
                    bitData[(i << 3) + 7] = (byte)((_GBA[i + _index] & 0xF0000000) >> 28);
                }
            }

            public void Flip(FlipStyle _x)
            {
                if (_x == FlipStyle.X)
                {
                    flipped ^= FlipStyle.X;
                    bitData.Flip(true, 8);
                }
                else if (_x == FlipStyle.Y)
                {
                    flipped ^= FlipStyle.Y;
                    bitData.Flip(false, 8);
                }
                else if (_x != FlipStyle.None)
                {
                    Flip(FlipStyle.X);
                    Flip(FlipStyle.Y);
                }
            }
            public void Unflip()
            {
                if (flipped != FlipStyle.None)
                    Flip(flipped);
            }

            public bool EqualTo(Tile _other, FlipStyle flippable)
            {
                Unflip();
                _other.Unflip();

                if (Enumerable.SequenceEqual(bitData, _other.bitData))
                    return true;

                if ((flippable & FlipStyle.X) != FlipStyle.None)
                {
                    Flip(FlipStyle.X);

                    if (Enumerable.SequenceEqual(bitData, _other.bitData))
                        return true;

                    if ((flippable & FlipStyle.Y) != FlipStyle.None)
                    {
                        Flip(FlipStyle.Y);

                        if (Enumerable.SequenceEqual(bitData, _other.bitData))
                            return true;
                        
                        Flip(FlipStyle.X);

                        if (Enumerable.SequenceEqual(bitData, _other.bitData))
                            return true;
                    }

                }
                else if ((flippable & FlipStyle.Y) != FlipStyle.None)
                {
                    Flip(FlipStyle.Y);

                    if (Enumerable.SequenceEqual(bitData, _other.bitData))
                        return true;
                }

                return false;
            }

            public ushort GetFlipOffset(Tile _other)
            {
                if (EqualTo(_other, FlipStyle.X | FlipStyle.Y))
                    return (ushort)flipped;

                return 0;
            }

        }
        private class BGTileSet
        {
            private class CompareTiles : IEqualityComparer<Tile>
            {
                public bool Equals(Tile x, Tile y)
                {
                    return x.EqualTo(y, Tile.FlipStyle.X | Tile.FlipStyle.Y);
                }

                public int GetHashCode(Tile obj)
                {
                    return obj.GetHashCode();
                }
            }

            private List<Tile> tiles = new List<Tile>();

            public IEnumerable<uint> Data(string _name)
            {
                if (tiles.Count > 192)
                    throw new Exception();
                if (tiles.Count > 128)
                    Console.WriteLine($"WARNING -- Background {_name} has a lot of tiles ({tiles.Count}).  It's recommended that you lower tile count");

                foreach (var tile in tiles)
                    foreach (var v in tile.RawData)
                        yield return v;
            }

            public Tile AddTile(Tile _tile)
            {
                if (!tiles.Contains(_tile, new CompareTiles()))
                {
                    tiles.Add(_tile);
                    return _tile;
                }
                else
                {
                    foreach (var t in tiles)
                    {
                        if (t.EqualTo(_tile, Tile.FlipStyle.X | Tile.FlipStyle.Y))
                            return t;
                    }

                    throw new Exception();
                }
            }
            
            public Tile GetTile(Tile _version)
            {
                foreach (var t in tiles)
                {
                    if (t.EqualTo(_version, Tile.FlipStyle.X | Tile.FlipStyle.Y))
                        return t;
                }

                return null;
            }
            public ushort GetIndex(Tile _version)
            {

                return (ushort)tiles.IndexOf(GetTile(_version));
            }
        }
        private class Background
        {
            public BGTileSet tileset { get; private set; }

            private int width, height;
            private Tile[,] tiles;

            private ushort[,] rawData;

            private List<ushort[]> palettesBase;
            private int[,] paletteIdx;

            public Background(Bitmap _map, BGTileSet _tiles)
            {
                width = _map.Width >> 3;
                height = _map.Height >> 3;

                tiles = new Tile[width, height];

                rawData = new ushort[_map.Width, _map.Height];

                for (int y = 0; y < _map.Height; ++y)
                    for (int x = 0; x < _map.Width; ++x)
                        rawData[x, y] = _map.GetPixel(x, y).ToGBA();
                
                palettesBase = new List<ushort[]>();
                List<Color> palette = new List<Color>(_map.Palette.Entries);

                for (int i = 0; i < palette.Count; i += 16)
                {
                    List<ushort> pal = new List<ushort>();

                    bool hasColor = false;

                    for (int j = 0; j < 16; ++j)
                    {
                        ushort color = palette[i + j].ToGBA();
                        pal.Add(color);

                        hasColor |= color != 0;
                    }

                    if (!hasColor) break;

                    palettesBase.Add(pal.ToArray());
                }

                tileset = _tiles??new BGTileSet();

                SetTiles();
            }
            public Background(AsepriteReader _read, BGTileSet _tiles, string _layer)
            {
                width = _read.Width >> 3;
                height = _read.Height >> 3;

                tiles = new Tile[width, height];

                rawData = new ushort[_read.Width, _read.Height];

                for (int y = 0; y < _read.Height; ++y)
                    for (int x = 0; x < _read.Width; ++x)
                        rawData[x, y] = _read.ReadColor(x, y, _layer: _layer).ToGBA();

                palettesBase = new List<ushort[]>();
                List<FloatColor> palette = new List<FloatColor>(_read.Colors);

                for (int i = 0; i < palette.Count; i += 16)
                {
                    List<ushort> pal = new List<ushort>();

                    bool hasColor = false;

                    for (int j = 0; j < 16; ++j)
                    {
                        ushort color = palette[i + j].ToGBA();
                        pal.Add(color);

                        hasColor |= color != 0;
                    }

                    if (!hasColor) break;

                    palettesBase.Add(pal.ToArray());
                }

                tileset = _tiles ?? new BGTileSet();

                SetTiles();
            }


            private void SetTiles()
            {
                paletteIdx = new int[width, height];

                for (int yL = 0; yL < height; yL += 8)
                {
                    for (int xL = 0; xL < width; xL += 8)
                    {
                        int p;

                        for (p = 0; p < palettesBase.Count; ++p)
                        {
                            bool safe = true;
                            for (int y = 0; y < 8; ++y)
                            {
                                for (int x = 0; x < 8; ++x)
                                {
                                    if (rawData[xL + x, yL + y] != 0x8000 && !palettesBase[p].Contains(rawData[xL + x, yL + y]))
                                    {
                                        safe = false;
                                        break;
                                    }
                                }
                                if (!safe)
                                    break;
                            }

                            if (safe)
                                break;
                        }

                        if (p == palettesBase.Count)
                            throw new Exception();
                    }
                }

                uint getOffset(int x, int y)
                {
                    return (uint)Array.IndexOf(palettesBase[paletteIdx[x >> 3, y >> 3]], rawData[x, y]);
                };

                AddTiles(GetArrayFromSprite(width << 3, height << 3, getOffset).GetEnumerator());
            }
            public void AddTiles(IEnumerator<uint> _tileData)
            {
                int x = 0, y = 0;
                do
                {
                    List<uint> tileRaw = new List<uint>();

                    for (int i = 0; i < 8 && _tileData.MoveNext(); ++i)
                    {
                        tileRaw.Add(_tileData.Current);
                    }

                    if (tileRaw.Count < 8)
                        break;

                    var tile = new Tile(tileRaw.ToArray(), 0);

                    tileset.AddTile(tile);

                    tiles[x, y] = tile;

                    x = (++x) % width;
                    if (x == 0)
                        ++y;

                } while (true);
            }

            public IEnumerable<ushort> Data()
            {
                for (int y = 0; y < height; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        var tile = tiles[x, y];
                        var ogTile = tileset.GetTile(tile);

                        ushort value = (ushort)((paletteIdx[x, y] << 12) | (tile.GetFlipOffset(ogTile) << 10) | tileset.GetIndex(tile));

                        yield return value;
                    }
                }

                yield break;
            }
        }

        private static Dictionary<string, Color[]> palettesFromSprites = new Dictionary<string, Color[]>();

        private static Dictionary<string, BGTileSet> tilesets = new Dictionary<string, BGTileSet>();
        private static List<string> backgroundsCompiled = new List<string>();

        public static void Compile(string _path)
        {
            string toSavePath = Path.Combine(Compiler.Path, "source");

#if !DEBUG
            bool needsRecompile = false;

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
#endif

            tilesets = new Dictionary<string, BGTileSet>();
            backgroundsCompiled = new List<string>();

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

                            _compiler.BeginArray(CompileToC.ArrayType.UInt, name);


                            _compiler.AddRange(Enumerable.ToArray(GetArrayFromSprite(map.Width, map.Height,
                                (x, y) => { return (uint)palette.IndexOf(map.GetPixel(x, y)); } )));

                            _compiler.EndArray();

                            map.Dispose();

                            break;
                        }
                    case ".ase":
                        bool separateTags = true;
                        using (AsepriteReader read = new AsepriteReader(s))
                        {
                            if (separateTags && read.TagNames.Length > 0)
                            {
                                foreach (var tag in read.Tags)
                                {
                                    _compiler.BeginArray(CompileToC.ArrayType.UInt, $"{name}_{tag.name}");

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
                                _compiler.BeginArray(CompileToC.ArrayType.UInt, $"{name}");

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
                                    _compiler.AddValue(map.GetPixel(j, i).ToGBA(0));
                                

                                _compiler.EndArray();
                            }

                            map.Dispose();

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

            _compiler.BeginArray(CompileToC.ArrayType.UInt, "particles");

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

                            uint getIdx(int x, int y)
                            {
                                x = (x & 7) | (map.Width  - (x & ~0x7) - 8);
                                y = (y & 7) | (map.Height - (y & ~0x7) - 8);

                                return (uint)palette.IndexOf(map.GetPixel(x, y));
                            }

                            _compiler.AddRange(Enumerable.ToArray(GetArrayFromSprite(map.Width, map.Height, getIdx)));

                            _compiler.AddValueDefine($"PART_{name}", index);
                            _compiler.AddValueDefine($"PART_{name}_L", Math.Min((map.Width * map.Height) >> 6, 4));

                            map.Dispose();

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
            string[] getFiles = Directory.GetDirectories(_path);

            foreach (string s in getFiles)
            {
                CompileBackground(s, _compiler, true);
            }

            List<BGTileSet> unique = new List<BGTileSet>();

            foreach (var str in tilesets.Keys)
            {
                if (unique.Contains(tilesets[str]))
                    continue;

                unique.Add(tilesets[str]);

                var array = tilesets[str].Data(str).ToArray();

                _compiler.AddValueDefine(str + "_len", array.Length >> 3);

                _compiler.BeginArray(CompileToC.ArrayType.UInt, str);
                _compiler.AddRange(array);
                _compiler.EndArray();
            }
        }
        private static void CompileBackground(string _path, CompileToC _compiler, bool _saveTiles)
        {
            string name = Path.GetFileName(_path);

            if (backgroundsCompiled.Contains(name)) // prevent backgrounds from being recompiled
                return;

            string dataPath = null;
            string ext = null;

            foreach (var s in Directory.GetFiles(_path))
            {
                ext = Path.GetExtension(s);
                if (ext == ".bmp" || ext == ".ase")
                {
                    dataPath = s;
                    break;
                }
            }

            if (dataPath == null)
                return;

            string otherTileset = null;

            BGTileSet tiles = null;
            if (otherTileset == null && tilesets.ContainsKey(name))
                otherTileset = name;
            
            if (otherTileset != null)
            {
                if (otherTileset != name)
                    CompileBackground(Path.Combine(_path, otherTileset), _compiler, false);

                tiles = tilesets[otherTileset];
            }

            switch (ext)
            {
                case ".bmp":
                    {
                        // Only mark a background 
                        if (_saveTiles)
                            backgroundsCompiled.Add(name);

                        Bitmap map = new Bitmap(dataPath);
                        if (map.Width % 256 != 0 || map.Height % 256 != 0)
                            break;

                        List<Color> palette = new List<Color>(map.Palette.Entries);

                        Background bg = new Background(map, tiles);

                        string tileName = $"BGT_{name}";

                        if (otherTileset != null && otherTileset != name)
                        {
                            _compiler.AddValueDefine(tileName, otherTileset);
                        }

                        tilesets.Add(tileName, bg.tileset);

                        if (_saveTiles)
                        {
                            _compiler.BeginArray(CompileToC.ArrayType.UShort, "BG_" + name);

                            _compiler.AddRange(Enumerable.ToArray(bg.Data()));

                            _compiler.EndArray();
                        }

                        map.Dispose();

                        break;
                    }
                case ".ase":
                    using (AsepriteReader read = new AsepriteReader(dataPath))
                    {
                        if (read.Width % 256 != 0 || read.Height % 256 != 0)
                            break;

                        // Only mark a background 
                        if (_saveTiles)
                            backgroundsCompiled.Add(name);

                        List<FloatColor> palette = new List<FloatColor>(read.Colors);

                        if (otherTileset == null)
                        {
                            otherTileset = $"BGT_{name}";

                            if (read.LayerNames.Length > 1)
                                otherTileset += "_" + read.LayerNames[0];
                        }

                        BGTileSet tileset = null;

                        foreach (var layer in read.LayerNames)
                        {
                            string tileName = $"BGT_{name}";

                            if (read.LayerNames.Length > 1)
                                tileName += "_" + layer.Replace(" ", "");

                            Background bg = new Background(read, tiles, layer);
                            tileset = bg.tileset;

                            if (otherTileset != name)
                            {
                                _compiler.AddValueDefine(tileName, otherTileset);
                            }

                            if (_saveTiles)
                            {
                                string exName = $"BG_{name}";
                                if (read.LayerNames.Length > 1)
                                    exName += "_" + layer.Replace(" ", "");

                                _compiler.BeginArray(CompileToC.ArrayType.UShort, exName);

                                _compiler.AddRange(Enumerable.ToArray(bg.Data()));

                                _compiler.EndArray();
                            }
                        }

                        tilesets.Add(otherTileset, tileset);

                        break;

                    }
                    break;
            }
        }

        public static IEnumerable<uint> GetArrayFromSprite(int _width, int _height, IndexOnSprite _values)
        {
            
            for (int yL = 0; yL < _height >> 3; ++yL)
            {
                for (int xL = 0; xL < _width >> 3; ++xL)
                {
                    for (int y = 0; y < 8; ++y)
                    {
                        uint tempValue = 0;

                        for (int i = 7; i >= 0; --i)
                            tempValue = (tempValue << 4) | _values((xL << 3) + i, (yL << 3) + y);

                        yield return tempValue;
                    }
                }
            }
        }
    }
}
