using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GBA_Compiler
{
    public static class Compiler
    {
        public static ushort ToGBA(this Color _color, ushort _transparent = 0x8000)
        {
            if (_color.R == 0 && _color.G == 0 && _color.B == 0)
                return _transparent;

            int r = (_color.R & 0xF8) >> 3;
            int g = (_color.G & 0xF8) >> 3;
            int b = (_color.B & 0xF8) >> 3;


            return (ushort)(r | (g << 5) | (b << 10));
        }
        public static string[] Arguments { get; private set; }
        public static bool HasArgument(string _arg)
        {
            return Arguments.Contains(_arg);
        }
        public static string Path { get; private set; }

        public static string GameName { get; private set; }

        static void Main(string[] _args)
        {
            Arguments = _args;
            Path = Directory.GetCurrentDirectory().Replace('/', '\\');

#if DEBUG
            Path = @"C:\Users\IsaGoodFriend\OneDrive\Documents\DevKitPro\Projects\PixtroGBA\Engine";
#endif

            if (Path.EndsWith("\\"))
                Path = Path.Substring(0, Path.Length - 1);

            GameName = System.IO.Path.GetFileName(Path);

            CompileArt.Compile(Path + "\\art");

#if DEBUG
            Console.WriteLine("Finished");
            Console.ReadLine();
            return;
#endif

            Process cmd = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.UseShellExecute = false;

            cmd.StartInfo = info;
            cmd.Start();

            using (StreamWriter sw = cmd.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    if (Arguments.Contains("-clean"))
                        sw.WriteLine("make clean");
                    else
                        sw.WriteLine("make");

                    if (Arguments.Contains("-run"))
                        sw.WriteLine($"{GameName}.gba");
                }
            }

            cmd.WaitForExit();
        }

        public static void Log(object log)
        {
            if (HasArgument("-log"))
            {
                Console.WriteLine(log.ToString());
            }
        }
    }
}
