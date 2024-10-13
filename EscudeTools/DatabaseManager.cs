using Microsoft.Data.Sqlite;
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
                            Console.WriteLine($"Invalid text offset: {textOffset:X}, sheet: {debugInfo1}, recordNum: {i}, type: {j}");
                            throw new Exception("Invalid text offset"); //应该不会再发生这种情况了
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
                    offset += sheet.col[j].size; //较小概率还有问题
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
                    string targetFile = Path.Combine(storePath, dbName + ".db");
                    ExtractEmbeddedDatabase(targetFile);
                    return SqliteProcess(db, targetFile);
                case 1: //csv
                    foreach (var s in db)
                    {
                        bool status = CsvProcess(s, Path.Combine(storePath, dbName + "_" + s.name + ".csv"));
                        if (!status)
                        {
                            throw new IOException($"Failed to export {s.name} CSV file.");
                        }
                        else
                        {
                            Console.WriteLine($"{s.name} CSV file exported successfully.");
                        }

                    }
                    return true;
                default:
                    throw new NotSupportedException("Unsupported output type.");
            }
        }

        private static void ExtractEmbeddedDatabase(string outputPath)
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
            //foreach (var rn in assembly.GetManifestResourceNames())
            //{
            //    Console.WriteLine(rn);
            //}
            string resourceName = "EscudeTools.empty.db";
            using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new Exception($"Error, No resource with name {resourceName} found.");
            using FileStream fileStream = new(outputPath, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);
        }

        private static bool SqliteProcess(Sheet[] db, string path)
        {
            //db含有多个sheet，每个sheet中col存放标题（对应数据库中应该是字段），records存放数据（对应数据库中应该是记录）
            using SqliteConnection connection = new($"Data Source={path};");
            connection.Open();

            foreach (var sheet in db)
            {
                using (SqliteCommand createTableCommand = connection.CreateCommand())
                {
                    StringBuilder createTableQuery = new();
                    createTableQuery.Append($"CREATE TABLE IF NOT EXISTS {sheet.name} (");

                    // Add columns to the create table query
                    foreach (var column in sheet.col)
                    {
                        createTableQuery.Append($"{column.name} {GetSQLiteColumnType(column.type)}, ");
                    }

                    createTableQuery.Remove(createTableQuery.Length - 2, 2); // Remove the last comma and space
                    createTableQuery.Append(");");

                    createTableCommand.CommandText = createTableQuery.ToString();
                    createTableCommand.ExecuteNonQuery();
                }

                using SqliteCommand insertDataCommand = connection.CreateCommand();
                StringBuilder insertDataQuery = new();
                insertDataQuery.Append($"INSERT INTO {sheet.name} (");

                // Add column names to the insert data query
                foreach (var column in sheet.col)
                {
                    insertDataQuery.Append($"{column.name}, ");
                }

                insertDataQuery.Remove(insertDataQuery.Length - 2, 2); // Remove the last comma and space
                insertDataQuery.Append(") VALUES (");

                // Add parameter placeholders to the insert data query
                for (int i = 0; i < sheet.cols; i++)
                {
                    insertDataQuery.Append($"@param{i}, ");
                }

                insertDataQuery.Remove(insertDataQuery.Length - 2, 2); // Remove the last comma and space
                insertDataQuery.Append(");");

                insertDataCommand.CommandText = insertDataQuery.ToString();

                // Add data parameters to the insert data command
                for (int i = 0; i < sheet.records.values.Length; i++)
                {
                    var record = (Record)sheet.records.values[i];
                    for (int j = 0; j < sheet.cols; j++)
                    {
                        var parameter = new SqliteParameter($"@param{j}", record.values[j]);
                        insertDataCommand.Parameters.Add(parameter);
                    }

                    insertDataCommand.ExecuteNonQuery();
                    insertDataCommand.Parameters.Clear();
                }
            }
            return true;
        }
        private static string GetSQLiteColumnType(ushort type)
        {
            return type switch
            {
                // int
                0x1 => "INTEGER",
                // float
                0x2 => "REAL",
                // string
                0x3 => "TEXT",
                // bool
                0x4 => "INTEGER",
                _ => throw new NotSupportedException($"Unsupported column type: {type}"),
            };
            throw new NotImplementedException();
        }
        private static bool CsvProcess(Sheet s, string path)
        {
            //s是单张表，col存放标题（对应csv应该是标题），records存放数据（对应数据库中应该是数据）
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("File path cannot be null or empty.", nameof(path));

            // 创建一个StringBuilder来构建CSV内容
            StringBuilder csvContent = new();

            // 写入列标题
            foreach (Column column in s.col)
            {
                csvContent.Append(column.name);
                csvContent.Append(',');
            }
            csvContent.AppendLine(); // 换行

            // 写入记录数据
            foreach (Record record in s.records.values.Cast<Record>())
            {
                foreach (object value in record.values)
                {
                    csvContent.Append(value);
                    csvContent.Append(',');
                }
                csvContent.AppendLine(); // 换行
            }

            // 将CSV内容写入文件
            try
            {
                File.WriteAllText(path, csvContent.ToString());
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to export CSV file: {ex.Message}");
                return false;
            }

            throw new NotImplementedException();
        }
    }
}
