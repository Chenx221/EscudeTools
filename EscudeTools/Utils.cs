﻿//部分代码来自Garbro
using System.Reflection;
using System.Text;

namespace EscudeTools
{
    public class Const
    {
        //Types
        public const string INT8 = "System.SByte";
        public const string UINT8 = "System.Byte";
        public const string INT16 = "System.Int16";
        public const string UINT16 = "System.UInt16";
        public const string INT32 = "System.Int32";
        public const string UINT32 = "System.UInt32";
        public const string DOUBLE = "System.Double";
        public const string FLOAT = "System.Single";
        public const string INT64 = "System.Int64";
        public const string UINT64 = "System.UInt64";
        public const string STRING = "System.String";
        public const string DELEGATE = "System.MulticastDelegate";
        public const string CHAR = "System.Char";

        //Attributes
        public const string PSTRING = "PString";
        public const string CSTRING = "CString";
        public const string UCSTRING = "UCString";
        public const string FSTRING = "FString";
        public const string STRUCT = "StructField";
        public const string IGNORE = "Ignore";
        public const string FARRAY = "FArray";
        public const string PARRAY = "PArray";
        public const string RARRAY = "RArray";
    }
    public class Utils
    {
        public static string ReadStringFromTextData(byte[] sheet_text, int offset)
        {
            return ReadStringFromTextData(sheet_text, offset, -1);
        }

        public static string ReadStringFromTextData(byte[] sheet_text, int offset, int length_limit)
        {
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding? shiftJis = provider.GetEncoding("shift-jis") ?? throw new InvalidOperationException("Shift-JIS encoding not supported.");
            return ReadStringFromTextData(sheet_text, offset, length_limit, shiftJis);
        }

        public static string ReadStringFromTextData(byte[] sheet_text, int offset, int length_limit, Encoding enc)
        {
            List<byte> stringBytes = [];
            int end = (length_limit != -1) ? Math.Min(offset + length_limit, sheet_text.Length) : sheet_text.Length;
            for (int i = offset; i < end && sheet_text[i] != 0x00; i++)
            {
                stringBytes.Add(sheet_text[i]);
            }
            return enc.GetString(stringBytes.ToArray());
        }

        public static byte[] ReadBytes(BinaryReader reader, ulong length)
        {
            const int bufferSize = 8192;
            byte[] data = new byte[length];
            ulong bytesRead = 0;
            while (bytesRead < length)
            {
                int toRead = (int)Math.Min(bufferSize, length - bytesRead);
                int read = reader.Read(data, (int)bytesRead, toRead);
                if (read == 0)
                    break;
                bytesRead += (ulong)read;
            }
            return data;
        }

        public static uint ToUInt32<TArray>(TArray value, int index) where TArray : IList<byte>
        {
            return (uint)(value[index] | value[index + 1] << 8 | value[index + 2] << 16 | value[index + 3] << 24);
        }

        public static uint ReadUInt32(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(4);
            if (bytes.Length < 4)
                throw new EndOfStreamException("Unexpected end of stream while reading UInt32.");
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static void ExtractEmbeddedDatabase(string outputPath)
        {
            if (File.Exists(outputPath))
            {
                Console.WriteLine($"File {outputPath} already exists. Do you want to overwrite it? (y/n)");
                string? input = Console.ReadLine();
                if (input?.ToLower() != "y")
                {
                    Console.WriteLine("Task cancelled, Exporting database aborted.");
                    return;
                }
            }
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "EscudeTools.empty.db";
            using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new Exception($"Error, No resource with name {resourceName} found.");
            using FileStream fileStream = new(outputPath, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);
        }

        public static string GetSQLiteColumnType(ushort type)
        {
            if (type == 1)
                return "INTEGER";
            //else if (type == 4 && size == 4)
            //    return "TEXT";
            else
                return "TEXT";
            //return type switch
            //{
            //    // int
            //    0x1 => "INTEGER",
            //    // float
            //    0x2 => "REAL",
            //    // string
            //    0x3 => "TEXT",
            //    // bool
            //    0x4 => "INTEGER",
            //    _ => throw new NotSupportedException($"Unsupported column type: {type}"),
            //};
        }

        public static bool ISKANJI(byte x)
        {
            return (((x) ^ 0x20) - 0xa1) <= 0x3b;
        }

        public static ushort GetColumnTypeFromSQLite(string v)
        {
            return v[^2] switch
            {
                '1' => 0x1,
                '2' => 0x2,
                '3' => 0x3,
                '4' => 0x4,
                _ => throw new NotSupportedException($"Unsupported column type: {v}"),
            };

        }

        public static ushort GetColumnSize(string v)
        {
            return v[^1] switch
            {
                '1' => 0x1,
                '2' => 0x2,
                '3' => 0x3,
                '4' => 0x4,
                _ => throw new NotSupportedException($"Unsupported column Size: {v}"),
            };
        }

        public static byte RotByteR(byte v, int count)
        {
            count &= 7;
            return (byte)(v >> count | v << (8 - count));
        }

        public static byte RotByteL(byte v, int count)
        {
            count &= 7;
            return (byte)(v << count | v >> (8 - count));
        }

        public static int ToBigEndian(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
        public static void CopyOverlapped(byte[] data, int src, int dst, int count)
        {
            if (dst > src)
            {
                while (count > 0)
                {
                    int preceding = Math.Min(dst - src, count);
                    Buffer.BlockCopy(data, src, data, dst, preceding);
                    dst += preceding;
                    count -= preceding;
                }
            }
            else
            {
                Buffer.BlockCopy(data, src, data, dst, count);
            }
        }
        public static dynamic Reverse(dynamic Data)
        {
            byte[] Arr = BitConverter.GetBytes(Data);
            Array.Reverse(Arr, 0, Arr.Length);
            string type = Data.GetType().FullName;
            return type switch
            {
                Const.INT8 or Const.UINT8 => Data,
                Const.INT16 => BitConverter.ToInt16(Arr, 0),
                Const.UINT16 => BitConverter.ToUInt16(Arr, 0),
                Const.INT32 => BitConverter.ToInt32(Arr, 0),
                Const.UINT32 => BitConverter.ToUInt32(Arr, 0),
                Const.INT64 => BitConverter.ToInt64(Arr, 0),
                Const.UINT64 => BitConverter.ToUInt64(Arr, 0),
                Const.DOUBLE => BitConverter.ToDouble(Arr, 0),
                Const.FLOAT => BitConverter.ToSingle(Arr, 0),
                _ => throw new Exception("Unk Data Type."),
            };
        }
        public static void WriteTo(uint Value, byte[] Binary, uint At)
        {
            BitConverter.GetBytes(Value).CopyTo(Binary, At);
        }

        public static int SearchLzwEntryList(List<LzwEntry> lle, string keyword)
        {
            foreach (LzwEntry le in lle)
            {
                if (le.Name == keyword)
                    return lle.IndexOf(le);
            }
            return -1;
        }
    }
}
