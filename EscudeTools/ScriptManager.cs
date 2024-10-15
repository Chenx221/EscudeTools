using System.Reflection;
using System.Text;

namespace EscudeTools
{
    public class ScriptMessage
    {
        public byte[] Data { get; set; }        // MESS領域 (消息区域) //这个还不知道怎么解析为人能看懂的
        public uint Size { get; set; }          // MESS領域サイズ (消息区域大小)
        public List<uint> Offset { get; set; }      // MESSオフセット (消息偏移)
        public uint Count { get; set; }         // MESS数 (消息数量)
        public string[] DataString { get; set; } // 给人看的Data内容
    }

    public class ScriptFile
    {
        public byte[] Code { get; set; }        // CODE領域 (代码区域)
        public uint CodeSize { get; set; }      // CODE領域サイズ (代码区域大小)

        public byte[] Text { get; set; }        // TEXT領域 (文本区域)
        public uint TextSize { get; set; }      // TEXT領域サイズ (文本区域大小)
        public uint[] TextOffset { get; set; }  // TEXTオフセット (文本偏移)
        public uint TextCount { get; set; }     // TEXT数 (文本数量)

        public uint MessCount { get; set; }     // MESS数 (消息数量)
        public string[] TextString { get; set; } // 给人看的Text内容
        public List<Command> Commands { get; set; } // 给人看的Code内容
    }

    public class Command
    {
        public uint Offset { get; set; }
        public byte Instruction { get; set; }
        public string InstructionString { get; set; }
        public List<Object> Parameter { get; set; }
        public String Helper { get; set; }
    }

    public class ScriptManager
    {
        static readonly byte[] MessHeader = [0x40, 0x6D, 0x65, 0x73, 0x73, 0x3A, 0x5F, 0x5F]; //@mess:__
        static readonly byte[] FileHeader = [0x40, 0x63, 0x6F, 0x64, 0x65, 0x3A, 0x5F, 0x5F]; //@code:__
        private ScriptMessage sm;
        private bool smEncrypted;
        private string smName = string.Empty;
        private ScriptFile sf;
        private string sfName = string.Empty;
        private int messIndex = 0;
        private bool enableCommandHelper = true;

        public ScriptMessage GetSM()
        {
            return sm;
        }

        public ScriptFile GetSF()
        {
            return sf;
        }

        public bool ToggleCommandHelper()
        {
            enableCommandHelper = !enableCommandHelper;
            return enableCommandHelper;
        }

        public bool LoadScriptFile(string path)
        {
            if (!File.Exists(path))
                return false;
            sf ??= new ScriptFile();
            sfName = Path.GetFileNameWithoutExtension(path);
            using FileStream fs = new(path, FileMode.Open);
            using BinaryReader br = new(fs);
            byte[] head = br.ReadBytes(8);
            if (!head.SequenceEqual(FileHeader))
                return false;
            sf.CodeSize = ReadUInt32(br);
            sf.TextCount = ReadUInt32(br);
            sf.TextSize = ReadUInt32(br);
            sf.MessCount = ReadUInt32(br);
            if (sf.MessCount > 0)
                if (sm == null)
                    if (!LoadScriptMess(Path.ChangeExtension(path, ".001")))
                        return false;

            if (fs.Length < sf.CodeSize + sf.TextSize + 16 + sf.TextCount * 4)
                return false;

            sf.Code = br.ReadBytes((int)sf.CodeSize);
            sf.TextOffset = new uint[sf.TextCount];
            for (int i = 0; i < sf.TextCount; i++)
            {
                sf.TextOffset[i] = br.ReadUInt32();
            }
            sf.Text = br.ReadBytes((int)sf.TextSize);
            sf.TextString = new string[sf.TextCount];
            for (int i = 0; i < sf.TextCount; i++)
            {
                sf.TextString[i] = ReadStringFromTextData(sf.Text, (int)sf.TextOffset[i]);
            }
            sf.Commands = [];
            for (int i = 0; i < sf.CodeSize;)
            {
                Command c = new()
                {
                    Parameter = [],
                    Instruction = sf.Code[i++],
                    Offset = (uint)i
                };
                if (enableCommandHelper)
                {
                    c.InstructionString = Define.GetInstructionString(c.Instruction, out int paramNum);
                    for (int j = 0; j < paramNum; j++)
                    {
                        c.Parameter.Add(Define.TyperHelper(c.Instruction, sf.Code, i));
                        i += 4;
                    }
                    if (sm != null)
                        c.Helper = Define.SetCommandStr(c, sf, sm, ref messIndex);
                }
                sf.Commands.Add(c);
            }
            return true;
        }

        private static uint ReadUInt32(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(4);
            if (bytes.Length < 4)
                throw new EndOfStreamException("Unexpected end of stream while reading UInt32.");
            return BitConverter.ToUInt32(bytes, 0);
        }

        private bool LoadScriptMess(string path)
        {
            if (!File.Exists(path))
                return false;
            sm ??= new ScriptMessage();
            smName = Path.GetFileNameWithoutExtension(path);
            using FileStream fs = new(path, FileMode.Open);
            using BinaryReader br = new(fs);
            byte[] head = br.ReadBytes(8);
            smEncrypted = head.SequenceEqual(MessHeader);
            if (smEncrypted)
            {
                sm.Count = ReadUInt32(br);
                sm.Size = ReadUInt32(br);
                sm.Offset = [];
                for (int i = 0; i < sm.Count; i++)
                {
                    sm.Offset.Add(br.ReadUInt32());
                }
                byte[] encryptData = br.ReadBytes((int)sm.Size);
                for (int i = 0; i < encryptData.Length; i++)
                {
                    encryptData[i] ^= 0x55;
                }
                sm.Data = encryptData;
            }
            else
            {
                fs.Position = 0;
                sm.Count = 0;
                sm.Size = (uint)fs.Length;
                sm.Data = br.ReadBytes((int)sm.Size);
                sm.Offset = [];
                uint offset = 0;
                for (uint i = 0; i < sm.Size; i++)
                {
                    if (ISKANJI(sm.Data[i]))
                        i++;
                    else
                    {
                        if (sm.Data[i] == '\r')
                        {
                            if (sm.Data[i + 1] != '\n')
                            {
                                sm.Data[i] = (byte)'\n';
                            }
                            else
                            {
                                sm.Data[i++] = (byte)'\0';
                            }
                        }
                        if (sm.Data[i] == '\n')
                        {
                            sm.Data[i] = (byte)'\0';
                            if (sm.Count < 4096)
                            {
                                sm.Offset.Add(offset);
                            }
                            sm.Count++;
                            offset = i + 1;
                        }
                    }
                }
            }
            sm.DataString = new string[sm.Count];
            for (int i = 0; i < sm.Count; i++)
            {
                sm.DataString[i] = ReadStringFromTextData(sm.Data, (int)sm.Offset[i]);
            }
            return true;
        }

        private static bool ISKANJI(byte x)
        {
            return (((x) ^ 0x20) - 0xa1) <= 0x3b;
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
            string resourceName = "EscudeTools.empty.db";
            using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new Exception($"Error, No resource with name {resourceName} found.");
            using FileStream fileStream = new(outputPath, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);
        }

        public bool ExportDatabase(int outputType, string? storePath)
        {
            storePath ??= Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Unable to determine the directory."); //导出位置
            throw new NotImplementedException();
            //switch (outputType)
            //{
            //    case 0: //sf
            //        if (string.IsNullOrEmpty(sfName))
            //            return false;
            //        ExtractEmbeddedDatabase(Path.Combine(storePath, sfName + ".db"));
            //        return SqliteProcess(sf, Path.Combine(storePath, sfName + ".db"));
            //    case 1: //sm
            //        if (string.IsNullOrEmpty(smName))
            //            return false;
            //        ExtractEmbeddedDatabase(Path.Combine(storePath, smName + ".db"));
            //        return SqliteProcess(sm, Path.Combine(storePath, smName + ".db"));
            //    default:
            //        throw new NotSupportedException("Unsupported output type.");
            //}
        }

        public bool ExportMessDatabase(int outputType, string? storePath)
        {
            storePath ??= Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Unable to determine the directory."); //导出位置
            throw new NotImplementedException();
            //switch (outputType)
            //{
            //    case 0: //sf
            //        if (string.IsNullOrEmpty(sfName))
            //            return false;
            //        ExtractEmbeddedDatabase(Path.Combine(storePath, sfName + ".db"));
            //        return SqliteProcess(sf, Path.Combine(storePath, sfName + ".db"));
            //    case 1: //sm
            //        if (string.IsNullOrEmpty(smName))
            //            return false;
            //        ExtractEmbeddedDatabase(Path.Combine(storePath, smName + ".db"));
            //        return SqliteProcess(sm, Path.Combine(storePath, smName + ".db"));
            //    default:
            //        throw new NotSupportedException("Unsupported output type.");
            //}
        }

        //private bool SqliteProcess(ScriptMessage sm, string path)
        //{
        //    using SqliteConnection connection = new($"Data Source={path};");
        //    connection.Open();

        //    using (SqliteCommand createTableCommand = connection.CreateCommand())
        //    {
        //        StringBuilder createTableQuery = new();
        //        createTableQuery.Append($"CREATE TABLE IF NOT EXISTS {smName} (");

        //        // Add columns to the create table query
        //        foreach (var column in sheet.col)
        //        {
        //            createTableQuery.Append($"{column.name} {GetSQLiteColumnType(column.type)}, ");
        //        }

        //        createTableQuery.Remove(createTableQuery.Length - 2, 2); // Remove the last comma and space
        //        createTableQuery.Append(");");

        //        createTableCommand.CommandText = createTableQuery.ToString();
        //        createTableCommand.ExecuteNonQuery();
        //    }

        //    using SqliteCommand insertDataCommand = connection.CreateCommand();
        //    StringBuilder insertDataQuery = new();
        //    insertDataQuery.Append($"INSERT INTO {sheet.name} (");

        //    // Add column names to the insert data query
        //    foreach (var column in sheet.col)
        //    {
        //        insertDataQuery.Append($"{column.name}, ");
        //    }

        //    insertDataQuery.Remove(insertDataQuery.Length - 2, 2); // Remove the last comma and space
        //    insertDataQuery.Append(") VALUES (");

        //    // Add parameter placeholders to the insert data query
        //    for (int i = 0; i < sheet.cols; i++)
        //    {
        //        insertDataQuery.Append($"@param{i}, ");
        //    }

        //    insertDataQuery.Remove(insertDataQuery.Length - 2, 2); // Remove the last comma and space
        //    insertDataQuery.Append(");");

        //    insertDataCommand.CommandText = insertDataQuery.ToString();

        //    // Add data parameters to the insert data command
        //    for (int i = 0; i < sheet.records.values.Length; i++)
        //    {
        //        var record = (Record)sheet.records.values[i];
        //        for (int j = 0; j < sheet.cols; j++)
        //        {
        //            var parameter = new SqliteParameter($"@param{j}", record.values[j]);
        //            insertDataCommand.Parameters.Add(parameter);
        //        }

        //        insertDataCommand.ExecuteNonQuery();
        //        insertDataCommand.Parameters.Clear();
        //    }
        //    return true;
        //}

        //private bool SqliteProcess(ScriptFile sf, string path)
        //{
        //    //db含有多个sheet，每个sheet中col存放标题（对应数据库中应该是字段），records存放数据（对应数据库中应该是记录）
        //    using SqliteConnection connection = new($"Data Source={path};");
        //    connection.Open();

        //    foreach (var sheet in db)
        //    {
        //        using (SqliteCommand createTableCommand = connection.CreateCommand())
        //        {
        //            StringBuilder createTableQuery = new();
        //            createTableQuery.Append($"CREATE TABLE IF NOT EXISTS {sheet.name} (");

        //            // Add columns to the create table query
        //            foreach (var column in sheet.col)
        //            {
        //                createTableQuery.Append($"{column.name} {GetSQLiteColumnType(column.type)}, ");
        //            }

        //            createTableQuery.Remove(createTableQuery.Length - 2, 2); // Remove the last comma and space
        //            createTableQuery.Append(");");

        //            createTableCommand.CommandText = createTableQuery.ToString();
        //            createTableCommand.ExecuteNonQuery();
        //        }

        //        using SqliteCommand insertDataCommand = connection.CreateCommand();
        //        StringBuilder insertDataQuery = new();
        //        insertDataQuery.Append($"INSERT INTO {sheet.name} (");

        //        // Add column names to the insert data query
        //        foreach (var column in sheet.col)
        //        {
        //            insertDataQuery.Append($"{column.name}, ");
        //        }

        //        insertDataQuery.Remove(insertDataQuery.Length - 2, 2); // Remove the last comma and space
        //        insertDataQuery.Append(") VALUES (");

        //        // Add parameter placeholders to the insert data query
        //        for (int i = 0; i < sheet.cols; i++)
        //        {
        //            insertDataQuery.Append($"@param{i}, ");
        //        }

        //        insertDataQuery.Remove(insertDataQuery.Length - 2, 2); // Remove the last comma and space
        //        insertDataQuery.Append(");");

        //        insertDataCommand.CommandText = insertDataQuery.ToString();

        //        // Add data parameters to the insert data command
        //        for (int i = 0; i < sheet.records.values.Length; i++)
        //        {
        //            var record = (Record)sheet.records.values[i];
        //            for (int j = 0; j < sheet.cols; j++)
        //            {
        //                var parameter = new SqliteParameter($"@param{j}", record.values[j]);
        //                insertDataCommand.Parameters.Add(parameter);
        //            }

        //            insertDataCommand.ExecuteNonQuery();
        //            insertDataCommand.Parameters.Clear();
        //        }
        //    }
        //    return true;
        //}

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
    }
}
