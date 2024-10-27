using Microsoft.Data.Sqlite;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

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
        //提示
        //颜色值转换可以看看https://argb-int-calculator.netlify.app/
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
                int order = 0;
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
                    Sheet sheet = ProcessSheet(sheet_struct, sheet_data, sheet_text, order++, sheets.Count);
                    sheets.Add(sheet);
                    nextBytes = br.ReadBytes(4);
                    if (nextBytes.Length < 4)
                        return false;
                }
            }
            db = [.. sheets];
            return true;
        }

        private static Sheet ProcessSheet(byte[] sheet_struct, byte[] sheet_data, byte[] sheet_text, int order, int debugInfo1 = 0)
        {
            Sheet sheet = new();
            //process struct
            uint nameOffset = BitConverter.ToUInt32(sheet_struct, 0);
            sheet.name = Utils.ReadStringFromTextData(sheet_text, (int)nameOffset) + $"_{order:D2}";//注意，末尾会添加_xx标记顺序
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
                column.name = Utils.ReadStringFromTextData(sheet_text, (int)columnNameOffset) + $"_{column.type}{column.size}"; //给repack留条活路
                sheet.col[i] = column;
                offset += 8;
            }
            uint columnSizes = 0;
            for (int i = 0; i < sheet.cols; i++)
            {
                columnSizes += sheet.col[i].size;
            }
            //process data
            offset = 0;
            int recordNum = (int)(sheet_data.Length / (columnSizes));//fix bug
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
                            record.values[j] = Utils.ReadStringFromTextData(sheet_text, (int)textOffset);
                        }

                    }
                    else
                    {
                        string n = sheet.col[j].name;
                        if (sheet.col[j].size == 1)
                            record.values[j] = sheet_data[offset];
                        else if (sheet.col[j].size == 2)
                            record.values[j] = BitConverter.ToInt16(sheet_data, offset);
                        else if (sheet.col[j].size == 4 && sheet.col[j].type == 1 && n[..^3] == "色") //无奈
                            record.values[j] = BitConverter.ToUInt32(sheet_data, offset);
                        else
                            record.values[j] = BitConverter.ToInt32(sheet_data, offset);

                    }
                    offset += sheet.col[j].size; //较小概率还有问题
                }
                recordFather.values[i] = record;
            }
            sheet.records = recordFather;
            return sheet;
        }

        public bool ExportDatabase(string? storePath)
        {
            if (db.Length == 0)
                return false;
            storePath ??= Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Unable to determine the directory."); //导出位置
            string targetFile = Path.Combine(storePath, dbName + ".db");
            Utils.ExtractEmbeddedDatabase(targetFile);
            return SqliteProcess(db, targetFile);
        }

        private static bool SqliteProcess(Sheet[] db, string path)
        {
            using SqliteConnection connection = new($"Data Source={path};");
            connection.Open();
            using var transaction = connection.BeginTransaction();
            foreach (var sheet in db)
            {
                using (SqliteCommand createTableCommand = connection.CreateCommand())
                {
                    StringBuilder createTableQuery = new();
                    createTableQuery.Append($"CREATE TABLE IF NOT EXISTS {sheet.name} (");

                    // Add columns to the create table query
                    foreach (var column in sheet.col)
                    {
                        createTableQuery.Append($"{column.name} {Utils.GetSQLiteColumnType(column.type)}, ");
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
            transaction.Commit();
            return true;
        }

        public static bool ExportMDB(string sqlitePath)
        {
            if (!File.Exists(sqlitePath))
                return false;
            using SqliteConnection connection = new($"Data Source={sqlitePath};");
            connection.Open();

            var tableNames = new List<string>();
            var orders = new List<int>();
            using (var command = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string s = reader.GetString(0);
                    orders.Add(int.Parse(s[^2..]));
                    tableNames.Add(s);
                }
            }
            var combined = tableNames
                .Select((name, index) => new { Name = name, Order = orders[index] })
                .OrderBy(x => x.Order)
                .ToList();
            tableNames = combined.Select(x => x.Name).ToList();
            string outputPath = Path.Combine(Path.GetDirectoryName(sqlitePath), Path.GetFileNameWithoutExtension(sqlitePath) + ".bin");
            using FileStream fs = new(outputPath, FileMode.Create);
            using BinaryWriter bw = new(fs);
            bw.Write(fileSignature);//文件头
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding? shiftJis = provider.GetEncoding("shift-jis");
            foreach (var tableName in tableNames)
            {
                uint colsNum = 0;
                using (var command = new SqliteCommand($"PRAGMA table_info({tableName});", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        colsNum++;
                    }
                }
                uint structSize = 8 + 8 * colsNum;
                bw.Write(structSize);//结构体大小
                uint textOffset = 0;
                bool flag = true; //只有sheetname和columnname需要text表？
                bool flag2 = true; //第一行是空的吗
                List<string> text = []; //只能放不重复
                List<string> textMulti = []; //允许重复字符串
                ushort[] types = new ushort[colsNum];
                ushort[] sizes = new ushort[colsNum];
                string[] cnames = new string[colsNum];
                using (var command = new SqliteCommand($"PRAGMA table_info({tableName});", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        types[reader.GetInt32(0)] = Utils.GetColumnTypeFromSQLite(reader.GetString(1));
                        sizes[reader.GetInt32(0)] = Utils.GetColumnSize(reader.GetString(1));
                        cnames[reader.GetInt32(0)] = reader.GetString(1);
                    }
                }
                int recordCount = 0;
                using (var command = new SqliteCommand($"SELECT COUNT(*) FROM {tableName};", connection))
                {
                    recordCount = Convert.ToInt32(command.ExecuteScalar());
                }
                uint dataSize = (uint)(sizes.Sum(x => (uint)x) * recordCount);

                using (var command = new SqliteCommand($"SELECT * FROM {tableName};", connection))
                using (var reader = command.ExecuteReader())
                {
                    reader.Read();
                    if (reader.GetFieldType(1) == typeof(int))
                    {
                        flag2 = reader.GetInt32(1) == 0;
                    }
                    else if (reader.GetFieldType(1) == typeof(string))
                    {
                        flag2 = string.IsNullOrEmpty(reader.GetString(1));
                    }
                }


                List<uint> textOffset1 = [];
                for (int i = 0; i < cnames.Length; i++)
                {
                    if (types[i] != 4)
                        continue;
                    if (textOffset == 0 && flag2)
                    {
                        textOffset++;
                        flag = false;
                    }

                    using var colCommand = new SqliteCommand($"SELECT {cnames[i]} FROM {tableName};", connection);
                    using var colReader = colCommand.ExecuteReader();
                    bool first = flag2;
                    while (colReader.Read())
                    {
                        if (first)
                        {
                            first = false;
                            continue;
                        }
                        string s = colReader.GetString(0);
                        int index = textMulti.IndexOf(s);//fix bug
                        textMulti.Add(s);
                        if (string.IsNullOrEmpty(s))// empty
                        {
                            textOffset1.Add(0);
                        }
                        else if (index == -1) // 如果字符串不存在
                        {
                            text.Add(s);
                            textOffset1.Add(textOffset); // 记录偏移量
                            textOffset += (uint)(shiftJis.GetBytes(s).Length + 1);
                        }
                        else
                        {
                            textOffset1.Add(textOffset1[index]); // 重复字符串处理
                        }
                    }
                }
                int index1 = textMulti.IndexOf(tableName[..^3]);
                textMulti.Add(tableName[..^3]);
                if (index1 == -1)
                {
                    text.Add(tableName[..^3]);//表名
                    textOffset1.Add(textOffset);
                    textOffset += (uint)shiftJis.GetBytes(tableName[..^3]).Length + 1;
                }
                else
                    textOffset1.Add(textOffset1[index1]);

                foreach (string c in cnames)
                {
                    index1 = textMulti.IndexOf(c[..^3]);
                    textMulti.Add(c[..^3]);
                    if (index1 == -1)
                    {
                        text.Add(c[..^3]);//列名
                        textOffset1.Add(textOffset);
                        textOffset += (uint)shiftJis.GetBytes(c[..^3]).Length + 1;
                    }
                    else
                        textOffset1.Add(textOffset1[index1]);
                }
                //
                bw.Write(textOffset1[textOffset1.Count - cnames.Length - 1]); //表名在text中的偏移
                bw.Write(colsNum);//列数
                for (int i = 0; i < colsNum; i++)
                {
                    bw.Write(types[i]);//类型
                    bw.Write(sizes[i]);//类型大小
                    bw.Write(textOffset1[textOffset1.Count - cnames.Length + i]); //列名在text中的偏移
                }
                bw.Write(dataSize);//数据大小
                //填充垃圾
                if (flag2)
                {
                    byte[] zeroBytes = new byte[sizes.Sum(x => (uint)x)];
                    bw.Write(zeroBytes);
                }

                //填充数据
                using (var command = new SqliteCommand($"SELECT * FROM {tableName};", connection))
                using (var reader = command.ExecuteReader())
                {
                    int index = 0;
                    bool first = flag2;
                    while (reader.Read())
                    {
                        if (first)
                        {
                            first = false;
                            continue;
                        }
                        int j = 0;
                        for (int i = 0; i < colsNum; i++)
                        {
                            int type = types[i];
                            int size = sizes[i];
                            string cname = cnames[i][..^3];
                            if (type == 4)
                            {
                                if (flag2)
                                    bw.Write(textOffset1[index + (recordCount - 1) * j++]);//fix bug
                                else
                                    bw.Write(textOffset1[index + (recordCount) * j++]);
                            }
                            else if (cname == "色" && size == 4)
                                bw.Write((uint)reader.GetInt64(i));
                            else if (size == 1)
                                bw.Write(reader.GetByte(i));
                            else if (size == 2)
                                bw.Write((ushort)(reader.GetInt16(i)));
                            else
                                bw.Write(reader.GetInt32(i));
                        }
                        index++;
                    }
                }
                bw.Write(textOffset);//文本大小
                //bool flag = true; //只有sheetname和columnname需要text表？
                //bool flag2 = true; //第一行是空的吗
                if (!flag)
                {
                    bw.Write((byte)0);//垃圾
                }
                foreach (var str in text)//文本
                {
                    bw.Write(shiftJis.GetBytes(str));
                    bw.Write((byte)0);
                }
            }
            bw.Write(stopBytes);
            return true;
        }
    }
}
