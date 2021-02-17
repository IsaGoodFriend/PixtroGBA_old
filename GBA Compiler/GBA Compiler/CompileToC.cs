using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GBA_Compiler
{
    public class CompileToC
    {
        public enum ArrayType
        {
            Char,
            UShort,
            Short,
            UInt,
            Int,
        }
        public enum CompileOptions
        {
            None = 0,

            CompileEmptyArrays = 1,
            
            Pretty = 0,
            Compact = 2,

            AddDefine = 4
        }
        public CompileOptions options = CompileOptions.AddDefine;

        private bool inArray;
        private List<string> source, header;

        private List<long> arrayValues;
        private string arrayHeader;
        ArrayType arrayType;

        public int ArrayLength { get { return arrayValues.Count; } }

        public CompileToC()
        {
            source = new List<string>();
            header = new List<string>();
            arrayValues = new List<long>();
        }

        public void AddValue(long _value)
        {
            if (!inArray)
                throw new Exception();

            arrayValues.Add(_value);
        }
        public void AddRange(long[] _value)
        {
            if (!inArray)
                throw new Exception();

            arrayValues.AddRange(_value);
        }
        public void AddRange(ushort[] _value)
        {
            if (!inArray)
                throw new Exception();

            arrayValues.AddRange(_value.Select(x => (long)x).ToArray());
        }

        public void AddValueDefine(string _name, int _value)
        {
            string def = $"#define {_name}";

            int len = def.Length;

            while (len < 40)
            {
                def += '\t';
                len = (len & 0xFFFC) + 4;
            }
            def += $"{_value}\n";

            header.Add(def);
        }

        public void BeginArray(ArrayType _type, string _name)
        {
            if (inArray)
                throw new Exception();
            inArray = true;

            arrayValues.Clear();

            arrayHeader = _name;
            arrayType = _type;
        }
        public void EndArray()
        {
            if (!inArray)
                throw new Exception();
            inArray = false;

            if (arrayValues.Count == 0 && (options & CompileOptions.CompileEmptyArrays) == CompileOptions.None)
                return;

            string end = (options & CompileOptions.Compact) == CompileOptions.Compact ? "" : "\n";
            string valueType = "";

            switch (arrayType)
            {
                case ArrayType.Char:
                    valueType = "unsigned char";
                    break;
                case ArrayType.Int:
                case ArrayType.UInt:
                    valueType = "int";
                    break;
                case ArrayType.Short:
                case ArrayType.UShort:
                    valueType = "short";
                    break;
            }
            if (arrayType.ToString().StartsWith("U"))
                valueType = "unsigned " + valueType;

            source.Add($"const {valueType} {arrayHeader}[{arrayValues.Count}] = {{ {end}");

            for (int i = 0; i < arrayValues.Count; ++i)
            {
                source.Add(CompileToString(arrayValues[i]) + ", " + ((i & 0xF) == 0xF ? $"{end}" : ""));
                
            }
            
            source.Add($"}}; {end}");

            header.Add($"extern const {valueType} {arrayHeader}[{arrayValues.Count}];\n");
        }

        public void SaveTo(string _path, string _name)
        {
            using (var sw = new StreamWriter(File.Open($"{_path}\\{_name}.c", FileMode.Create)))
            {
                sw.WriteLine($"#include \"{_name}.h\" ");
                foreach (var str in source)
                {
                    sw.Write(str);
                }
            }
            using (var sw = new StreamWriter(File.Open($"{_path}\\{_name}.h", FileMode.Create)))
            {
                if ((options & CompileOptions.AddDefine) != CompileOptions.None)
                {
                    sw.WriteLine($"#ifndef _{_name.ToUpper()}");
                    sw.WriteLine($"#define _{_name.ToUpper()}");
                }

                foreach (var str in header)
                {
                    sw.Write(str);
                }

                if ((options & CompileOptions.AddDefine) != CompileOptions.None)
                    sw.WriteLine("\n#endif");
            }
        }

        private string CompileToString(long obj)
        {
            bool neg = obj < 0 && (arrayType == ArrayType.Short || arrayType == ArrayType.Int);
            obj = Math.Abs(obj);

            string retval = "";

            switch (arrayType)
            {
                case ArrayType.Char:
                    obj = (char)obj;
                    retval = Convert.ToString(obj, 16);//.ToString("X2");
                    break;
                case ArrayType.Short:
                    obj = (short)obj & 0x7FFF;
                    retval = (obj).ToString("X4");
                    break;
                case ArrayType.UShort:
                    obj = (short)obj & 0xFFFF;
                    retval = (obj).ToString("X4");
                    break;
                case ArrayType.Int:
                    obj = (short)obj & 0x7FFFFFFF;
                    retval = (obj).ToString("X8");
                    break;
                case ArrayType.UInt:
                    obj = (short)obj & 0xFFFFFFFF;
                    retval = (obj).ToString("X8");
                    break;
            }

            return (neg ? "-" : "") + $"0x{retval}";
        }
    }
}
