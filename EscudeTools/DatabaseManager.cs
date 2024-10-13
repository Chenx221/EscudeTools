using System.Reflection;
using System.Text;

namespace EscudeTools
{
    public class Sheet
    {
        public string name;
        public uint cols;
        public Column[] col;
        public Record records;
    }
    public class Column
    {
        public string name;
        public ushort type;
        public ushort size;
    }
    public class Record(int columnCount)
    {
        public object[] values = new object[columnCount];  // 每列的数据值
    }
    public class DatabaseManager
    {
        static readonly byte[] fileSignature = [0x6D, 0x64, 0x62, 0x00];
        static readonly byte[] stopBytes = [0x00, 0x00, 0x00, 0x00];

        private Sheet[] db = [];
        private string dbName = string.Empty;

        public Sheet[] GetDB() { return db; }

        public bool LoadDatabase(string path)
        {
            if (db.Length > 0)
                db = [];
            if (!File.Exists(path))
                return false;
            dbName = Path.GetFileNameWithoutExtension(path);
            List<Sheet> sheets = [];
            using (FileStream fs = new(path, FileMode.Open))
            using (BinaryReader br = new(fs))
            {
                byte[] head = br.ReadBytes(4);
                if (!head.SequenceEqual(fileSignature))
                    return false;
                byte[] nextBytes = br.ReadBytes(4);
                if (nextBytes.Length < 4)
                    return false;
                while (!nextBytes.SequenceEqual(stopBytes))
                {
                    uint sheet_struct_size = BitConverter.ToUInt32(nextBytes, 0);
                    byte[] sheet_struct = br.ReadBytes((int)sheet_struct_size);
                    nextBytes = br.ReadBytes(4);
                    uint sheet_data_size = BitConverter.ToUInt32(nextBytes, 0);
                    byte[] sheet_data = br.ReadBytes((int)sheet_data_size);
                    nextBytes = br.ReadBytes(4);
                    uint sheet_text_size = BitConverter.ToUInt32(nextBytes, 0);
                    byte[] sheet_text = br.ReadBytes((int)sheet_text_size);
                    Sheet sheet = ProcessSheet(sheet_struct, sheet_data, sheet_text, sheets.Count);
                    sheets.Add(sheet);
                    nextBytes = br.ReadBytes(4);
                    if (nextBytes.Length < 4)
                        return false;
                }
            }
            db = [.. sheets];
            return true;
        }

        private static Sheet ProcessSheet(byte[] sheet_struct, byte[] sheet_data, byte[] sheet_text, int debugInfo1 = 0)
        {
            Sheet sheet = new();
            //process struct
            uint nameOffset = BitConverter.ToUInt32(sheet_struct, 0);
            sheet.name = ReadStringFromTextData(sheet_text, (int)nameOffset);
            sheet.cols = BitConverter.ToUInt32(sheet_struct, 4);
            sheet.col = new Column[sheet.cols];
            int offset = 8;
            for (int i = 0; i < sheet.cols; i++)
            {
                Column column = new()
                {
                    type = BitConverter.ToUInt16(sheet_struct, offset)
                };
                if (column.type == 0x3 || column.type == 0x2)
                    throw new NotSupportedException("Unsupported Format"); //暂时不受支持的0x2 0x3
                column.size = BitConverter.ToUInt16(sheet_struct, offset + 2);
                uint columnNameOffset = BitConverter.ToUInt32(sheet_struct, offset + 4);
                column.name = ReadStringFromTextData(sheet_text, (int)columnNameOffset);
                sheet.col[i] = column;
                offset += 8;
            }
            //process data
            offset = 0;
            int recordNum = (int)(sheet_data.Length / (4 * sheet.cols));
            Record recordFather = new(recordNum);
            for (int i = 0; i < recordNum; i++)
            {
                Record record = new((int)sheet.cols);
                for (int j = 0; j < sheet.cols; j++) //对应cols //色值处理好像有点问题？
                {
                    if (sheet.col[j].type == 4)
                    {
                        uint textOffset = BitConverter.ToUInt32(sheet_data, offset);
                        if (sheet_text.Length < textOffset)
                        {
                            record.values[j] = textOffset.ToString("X"); //let you go, i will fix you later
                            Console.WriteLine($"Invalid text offset: {textOffset:X}, sheet: {debugInfo1}, recordNum: {i}, type: {j}"); //Not Supported, May be a script specific value
                        }
                        else
                        {
                            record.values[j] = ReadStringFromTextData(sheet_text, (int)textOffset);
                        }

                    }
                    else
                    {
                        record.values[j] = BitConverter.ToInt32(sheet_data, offset);
                    }
                    offset += 4; //可能有问题
                }
                recordFather.values[i] = record;
            }
            sheet.records = recordFather;
            return sheet;
        }

        private static string ReadStringFromTextData(byte[] sheet_text, int offset)
        {
            List<byte> stringBytes = [];
            for (int i = offset; i < sheet_text.Length && sheet_text[i] != 0x00; i++)
            {
                stringBytes.Add(sheet_text[i]);
            }
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding? shiftJis = provider.GetEncoding("shift-jis");
            return shiftJis == null
                ? throw new InvalidOperationException("Shift-JIS encoding not supported.")
                : shiftJis.GetString(stringBytes.ToArray());
        }

        public bool ExportDatabase(int outputType, string? storePath)
        {
            if (db.Length == 0)
                return false;
            storePath ??= Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Unable to determine the directory."); //导出位置
            switch (outputType)
            {
                case 0: //sqlite
                    return SqliteProcess(db, Path.Combine(storePath, dbName + ".sqlite"));
                case 1: //csv
                    foreach (var s in db)
                    {
                        bool status = CsvProcess(s, Path.Combine(storePath, dbName + "_" + s.name + ".csv"));
                        if (!status)
                            throw new IOException($"Failed to export sheet: {s.name}");
                    }
                    return true;
                default:
                    throw new NotSupportedException("Unsupported output type.");
            }
        }

        private bool SqliteProcess(Sheet[] db, string path)
        {
            throw new NotImplementedException();
        }
        private bool CsvProcess(Sheet s, string path)
        {
            throw new NotImplementedException();
        }
    }
}
