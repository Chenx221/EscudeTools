using Microsoft.Data.Sqlite;
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
        public byte[] Parameter { get; set; }
        public string Helper { get; set; }
        public bool IsProcSet { get; set; }
    }

    public class ScriptManager
    {
        static readonly byte[] MessHeader = [0x40, 0x6D, 0x65, 0x73, 0x73, 0x3A, 0x5F, 0x5F]; //@mess:__
        static readonly byte[] FileHeader = [0x40, 0x63, 0x6F, 0x64, 0x65, 0x3A, 0x5F, 0x5F]; //@code:__
        private ScriptMessage sm;
        private bool smEncrypted;
        private string name = string.Empty;
        private ScriptFile sf;
        private int messIndex = 0;

        public ScriptMessage GetSM()
        {
            return sm;
        }

        public ScriptFile GetSF()
        {
            return sf;
        }

        public bool LoadScriptFile(string path)
        {
            if (!File.Exists(path))
                return false;
            sf = new ScriptFile();
            name = Path.GetFileNameWithoutExtension(path);
            using FileStream fs = new(path, FileMode.Open);
            using BinaryReader br = new(fs);
            byte[] head = br.ReadBytes(8);
            if (!head.SequenceEqual(FileHeader))
                return false;
            sf.CodeSize = Utils.ReadUInt32(br);
            sf.TextCount = Utils.ReadUInt32(br);
            sf.TextSize = Utils.ReadUInt32(br);
            sf.MessCount = Utils.ReadUInt32(br);
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
                sf.TextString[i] = Utils.ReadStringFromTextData(sf.Text, (int)sf.TextOffset[i]);
            }
            sf.Commands = [];
            for (int i = 0; i < sf.CodeSize;)
            {
                Command c = new()
                {
                    Instruction = sf.Code[i++],
                    Offset = (uint)i,
                    IsProcSet = false,
                };
                c.InstructionString = Define.GetInstructionString(c.Instruction, out int paramNum);
                if (paramNum > 0)
                {
                    c.Parameter = new byte[paramNum * 4];
                    Buffer.BlockCopy(sf.Code, i, c.Parameter, 0, 4 * paramNum);
                    i += 4 * paramNum;
                }
                if (sm != null)
                    c.Helper = Define.SetCommandStr(c, sf, sm, ref messIndex);
                else
                    c.Helper = Define.SetCommandStr(c, sf, null, ref messIndex);
                sf.Commands.Add(c);
            }
            return true;
        }

        private bool LoadScriptMess(string path)
        {
            if (!File.Exists(path))
                return false;
            sm = new ScriptMessage();
            using FileStream fs = new(path, FileMode.Open);
            using BinaryReader br = new(fs);
            byte[] head = br.ReadBytes(8);
            smEncrypted = head.SequenceEqual(MessHeader);
            if (smEncrypted)
            {
                sm.Count = Utils.ReadUInt32(br);
                sm.Size = Utils.ReadUInt32(br);
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
                    if (Utils.ISKANJI(sm.Data[i]))
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
                sm.DataString[i] = Utils.ReadStringFromTextData(sm.Data, (int)sm.Offset[i]);
            }
            return true;
        }

        //此导出功能导出的sqlite数据库
        public bool ExportDatabase(string storePath)
        {
            if (sf.Code == null)
                return false;
            storePath ??= Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Unable to determine the directory.");
            if (string.IsNullOrEmpty(name))
                return false;
            string targetPath = Path.Combine(storePath, "script.db");
            if (!File.Exists(targetPath))
                Utils.ExtractEmbeddedDatabase(targetPath);
            return SqliteProcess(sf, targetPath);
        }

        //从ScriptMessage中导出游戏文本
        public bool ExportMessDatabase(string storePath)
        {
            if (sf == null)
                return false;
            if (sm == null)
                return true;
            storePath ??= Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Unable to determine the directory.");
            if (string.IsNullOrEmpty(name))
                return false;
            string targetPath = Path.Combine(storePath, "script_sm.db");
            if (!File.Exists(targetPath))
                Utils.ExtractEmbeddedDatabase(targetPath);
            return SqliteProcess(sm, targetPath);
        }

        //从ScriptFile中导出Text部分，剩余指令部分导出至.dat文件，以便重新封包
        public bool ExportTextDatabase(string storePath)
        {
            if (sf == null)
                return false;
            //分成两个文件，一个是放text的sqlite数据库，一个是放code的dat文件

            //dat
            string datPath = Path.Combine(storePath, name + ".dat");
            if (File.Exists(datPath))
                return false;
            using FileStream fs = new(datPath, FileMode.Create);
            using BinaryWriter bw = new(fs);
            bw.Write(FileHeader);//文件头
            bw.Write(sf.CodeSize);//代码区大小
            bw.Write(sf.TextCount);//文本数量
            byte[] empty4B = new byte[4];
            bw.Write(empty4B);//文本大小(占位)
            bw.Write(sf.MessCount);//消息数量
            bw.Write(sf.Code);//代码区

            //sqlite
            storePath ??= Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Unable to determine the directory.");
            if (string.IsNullOrEmpty(name))
                return false;
            string targetPath = Path.Combine(storePath, "script_text.db");
            if (!File.Exists(targetPath))
                Utils.ExtractEmbeddedDatabase(targetPath);
            return SqliteProcess(sf.TextString, targetPath);
        }

        private bool SqliteProcess(ScriptFile sf, string path)
        {
            using SqliteConnection connection = new($"Data Source={path};");
            connection.Open();

            string checkTableExistsQuery = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{name}';";
            using (var checkTableCmd = new SqliteCommand(checkTableExistsQuery, connection))
            {
                var result = checkTableCmd.ExecuteScalar();
                if (result != null) return false;
            }

            string createTableQuery = $@"
        CREATE TABLE {name} (
            Offset INTEGER,
            Instruction INTEGER,
            InstructionString TEXT,
            Parameter BLOB,
            Helper TEXT
        );";
            using (var createTableCmd = new SqliteCommand(createTableQuery, connection))
            {
                createTableCmd.ExecuteNonQuery();
            }

            string checkTextTableExistsQuery = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{name}__text';";
            using (var checkTextTableCmd = new SqliteCommand(checkTextTableExistsQuery, connection))
            {
                var result = checkTextTableCmd.ExecuteScalar();
                if (result == null)
                {
                    string createTextTableQuery = $@"
            CREATE TABLE {name}__text (
                Text TEXT
            );";
                    using var createTextTableCmd = new SqliteCommand(createTextTableQuery, connection);
                    createTextTableCmd.ExecuteNonQuery();
                }
                else
                    return false;
            }
            if (sm != null)
            {
                string checkMessTableExistsQuery = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{name}__mess';";
                using var checkMessTableCmd = new SqliteCommand(checkMessTableExistsQuery, connection);
                var result = checkMessTableCmd.ExecuteScalar();
                if (result == null)
                {
                    string createMessTableQuery = $@"
            CREATE TABLE {name}__mess (
                Mess TEXT
            );";
                    using var createMessTableCmd = new SqliteCommand(createMessTableQuery, connection);
                    createMessTableCmd.ExecuteNonQuery();
                }
                else
                    return false;
            }

            string insertQuery = $"INSERT INTO {name} (Offset, Instruction, InstructionString, Parameter, Helper) VALUES (@Offset, @Instruction, @InstructionString, @Parameter, @Helper);";

            using var transaction = connection.BeginTransaction();
            using var insertCmd = new SqliteCommand(insertQuery, connection, transaction);

            foreach (var command in sf.Commands)
            {
                insertCmd.Parameters.Clear();
                insertCmd.Parameters.AddWithValue("@Offset", command.Offset);
                insertCmd.Parameters.AddWithValue("@Instruction", command.Instruction);
                insertCmd.Parameters.AddWithValue("@InstructionString", command.InstructionString);
                insertCmd.Parameters.AddWithValue("@Parameter", (command.Parameter == null)
        ? DBNull.Value
        : command.Parameter);
                insertCmd.Parameters.AddWithValue("@Helper", command.Helper ?? "");

                insertCmd.ExecuteNonQuery();
            }

            string insertQuerySub = $"INSERT INTO {name}__text (Text) VALUES (@Text);";

            using var insertCmdSub = new SqliteCommand(insertQuerySub, connection, transaction);

            foreach (string ts in sf.TextString)
            {
                insertCmdSub.Parameters.Clear();
                insertCmdSub.Parameters.AddWithValue("@Text", ts ?? "");
                insertCmdSub.ExecuteNonQuery();
            }

            if (sm != null)
            {
                string insertQuerySub2 = $"INSERT INTO {name}__mess (Mess) VALUES (@Mess);";
                using var insertCmdSub2 = new SqliteCommand(insertQuerySub2, connection, transaction);

                foreach (string ds in sm.DataString)
                {
                    insertCmdSub2.Parameters.Clear();
                    insertCmdSub2.Parameters.AddWithValue("@Mess", ds ?? "");
                    insertCmdSub2.ExecuteNonQuery();
                }
            }

            transaction.Commit();
            return true;
        }

        private bool SqliteProcess(ScriptMessage sm, string path)
        {
            using SqliteConnection connection = new($"Data Source={path};");
            connection.Open();

            string checkTableExistsQuery = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{name}';";
            using (var checkTableCmd = new SqliteCommand(checkTableExistsQuery, connection))
            {
                var result = checkTableCmd.ExecuteScalar();
                if (result != null) return true;
            }

            string createTableQuery = $@"
        CREATE TABLE {name} (
            DataString TEXT
        );";
            using (var createTableCmd = new SqliteCommand(createTableQuery, connection))
            {
                createTableCmd.ExecuteNonQuery();
            }

            string insertQuery = $"INSERT INTO {name} (DataString) VALUES (@DataString);";

            using var transaction = connection.BeginTransaction();
            using var insertCmd = new SqliteCommand(insertQuery, connection, transaction);

            foreach (var ds in sm.DataString)
            {
                insertCmd.Parameters.Clear();
                insertCmd.Parameters.AddWithValue("@DataString", ds ?? "");
                insertCmd.ExecuteNonQuery();
            }

            transaction.Commit();
            return true;
        }

        private bool SqliteProcess(string[] ts, string path)
        {
            using SqliteConnection connection = new($"Data Source={path};");
            connection.Open();

            string checkTableExistsQuery = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{name}';";
            using (var checkTableCmd = new SqliteCommand(checkTableExistsQuery, connection))
            {
                var result = checkTableCmd.ExecuteScalar();
                if (result != null) return true;
            }

            string createTableQuery = $@"
        CREATE TABLE {name} (
            Text TEXT
        );";
            using (var createTableCmd = new SqliteCommand(createTableQuery, connection))
            {
                createTableCmd.ExecuteNonQuery();
            }

            string insertQuery = $"INSERT INTO {name} (Text) VALUES (@Text);";

            using var transaction = connection.BeginTransaction();
            using var insertCmd = new SqliteCommand(insertQuery, connection, transaction);

            foreach (var t in ts)
            {
                insertCmd.Parameters.Clear();
                insertCmd.Parameters.AddWithValue("@Text", t ?? "");
                insertCmd.ExecuteNonQuery();
            }

            transaction.Commit();
            return true;
        }

        public static bool Repackv1(string sqlitePath, bool scramble = true)
        {
            if (!File.Exists(sqlitePath))
                return false;
            using SqliteConnection connection = new($"Data Source={sqlitePath};");
            connection.Open();

            var tableNames = new List<string>();
            using (var command = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (!reader.GetString(0).EndsWith("__text") && !reader.GetString(0).EndsWith("__mess"))
                    {
                        tableNames.Add(reader.GetString(0));
                    }
                }
            }
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding? shiftJis = provider.GetEncoding("shift-jis");
            foreach (var tableName in tableNames)
            {
                string folder = Path.Combine(Path.GetDirectoryName(sqlitePath), "repack");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                string outputPath = Path.Combine(Path.GetDirectoryName(sqlitePath), "repack", tableName + ".bin");
                using FileStream fs = new(outputPath, FileMode.Create);
                using BinaryWriter bw = new(fs);
                ScriptFile sf = new()
                {
                    CodeSize = 0,
                    TextCount = 0,
                    TextSize = 0,
                    MessCount = 0,
                    Commands = []
                };
                using (var command = new SqliteCommand($"SELECT * FROM {tableName};", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Command c = new()
                        {
                            Instruction = reader.GetByte(1),
                            Parameter = reader.IsDBNull(3) ? [] : (byte[])reader[3]
                        };
                        sf.Commands.Add(c);
                        sf.CodeSize += 1 + (reader.IsDBNull(3) ? 0 : (uint)((byte[])reader[3]).Length);
                    }
                }
                List<string> textString = [];
                List<uint> textOffset = [];
                using (var command = new SqliteCommand($"SELECT * FROM {tableName}__text;", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string s = reader.GetString(0);
                        sf.TextCount++;
                        textString.Add(s);
                        textOffset.Add(sf.TextSize);
                        sf.TextSize += (uint)(shiftJis.GetBytes(s).Length + 1);
                    }
                }
                uint Offset = 0;
                List<uint> messOffset = [];
                List<string> messString = [];
                bool flag = false; // need .001?
                using (var command = new SqliteCommand($"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}__mess';", connection))
                {
                    using var reader = command.ExecuteReader();
                    flag = reader.Read();
                }
                if (flag)
                {
                    using var command = new SqliteCommand($"SELECT * FROM {tableName}__mess;", connection);
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string s = reader.GetString(0);
                        messString.Add(s);
                        messOffset.Add(Offset);
                        Offset += (uint)(shiftJis.GetBytes(s).Length + 1);
                        sf.MessCount++;
                    }
                }


                //准备写入
                bw.Write(FileHeader);//文件头
                bw.Write(sf.CodeSize);//代码区大小
                bw.Write(sf.TextCount);//文本数量
                bw.Write(sf.TextSize);//文本大小
                bw.Write(sf.MessCount);//消息数量
                foreach (Command c in sf.Commands)//写入代码区
                {
                    bw.Write(c.Instruction);
                    if (c.Parameter.Length > 0)
                        bw.Write(c.Parameter);
                }
                foreach (uint to in textOffset)//写入文本区偏移
                {
                    bw.Write(to);
                }
                foreach (string ts in textString)//写入文本区
                {
                    bw.Write(shiftJis.GetBytes(ts));
                    bw.Write((byte)0);
                }

                //再看看.001
                if (flag)
                {
                    string outputPath2 = Path.Combine(Path.GetDirectoryName(sqlitePath), "repack", tableName + ".001");
                    using FileStream fs2 = new(outputPath2, FileMode.Create);
                    using BinaryWriter bw2 = new(fs2);
                    if (scramble)
                    {
                        bw2.Write(MessHeader);//文件头
                        bw2.Write(sf.MessCount);//消息数量
                        bw2.Write(Offset);//消息大小
                        foreach (uint mo in messOffset)//写入消息区偏移
                        {
                            bw2.Write(mo);
                        }
                        byte[] rawData = new byte[Offset];
                        int index = 0;
                        foreach (string ms in messString)
                        {
                            byte[] data = shiftJis.GetBytes(ms);
                            Buffer.BlockCopy(data, 0, rawData, index, data.Length);
                            index += data.Length;
                            rawData[index++] = 0;
                        }
                        for (int i = 0; i < rawData.Length; i++)
                        {
                            rawData[i] ^= 0x55;
                        }
                        bw2.Write(rawData);//写入加密的消息区

                    }
                    else //ps.这是无加密情况下的处理代码，下面这块是gpt照着读取写的，出bug我不背锅
                    {
                        ScriptMessage sm = new()
                        {
                            Data = new byte[Offset],
                            Size = Offset,
                            Offset = messOffset,
                            Count = sf.MessCount
                        };
                        int index = 0;
                        foreach (string ms in messString)
                        {
                            byte[] data = shiftJis.GetBytes(ms);
                            Buffer.BlockCopy(data, 0, sm.Data, index, data.Length);
                            index += data.Length;
                            sm.Data[index++] = 0;
                        }
                        List<byte> reconstructedData = [];
                        uint currentPosition = 0;
                        for (int i = 0; i < sm.Count; i++)
                        {
                            uint offset = sm.Offset[i];
                            while (currentPosition < sm.Size && sm.Data[currentPosition] != 0)
                            {
                                if (Utils.ISKANJI(sm.Data[currentPosition]))
                                {
                                    reconstructedData.Add(sm.Data[currentPosition]);
                                    currentPosition++;
                                    continue; // Skip the next byte since it is part of the Kanji character
                                }
                                reconstructedData.Add(sm.Data[currentPosition]);
                                currentPosition++;
                            }
                            reconstructedData.Add((byte)'\n');
                            currentPosition++; // Skip the null terminator
                        }
                        while (currentPosition < sm.Size)
                        {
                            if (sm.Data[currentPosition] != 0)
                            {
                                reconstructedData.Add(sm.Data[currentPosition]);
                            }
                            currentPosition++;
                        }
                        byte[] finalData = [.. reconstructedData];
                        bw2.Write(finalData);//写入整个数据
                    }
                }
            }
            return true;
        }

        public static bool Repackv2(string sqlitePath, bool scramble = true)
        {
            if (!File.Exists(sqlitePath))
                return false;
            using SqliteConnection connection = new($"Data Source={sqlitePath};");
            connection.Open();

            var tableNames = new List<string>();
            using (var command = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    tableNames.Add(reader.GetString(0));
                }
            }
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding? shiftJis = provider.GetEncoding("shift-jis");
            foreach (var tableName in tableNames)
            {
                string folder = Path.Combine(Path.GetDirectoryName(sqlitePath), "repack");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                string outputPath = Path.Combine(Path.GetDirectoryName(sqlitePath), "repack", tableName + ".001");
                using FileStream fs = new(outputPath, FileMode.Create);
                using BinaryWriter bw = new(fs);
                ScriptMessage sm = new()
                {
                    Count = 0,
                    Size = 0
                };
                List<uint> messOffset = [];
                List<string> messString = [];
                using (var command = new SqliteCommand($"SELECT * FROM {tableName};", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string s = reader.GetString(0);
                        messString.Add(s);
                        messOffset.Add(sm.Size);
                        sm.Size += (uint)(shiftJis.GetBytes(s).Length + 1);
                        sm.Count++;
                    }
                }
                if (scramble) //mess部分好像有不少重复代码，以后有空再优化
                {
                    bw.Write(MessHeader);//文件头
                    bw.Write(sm.Count);//消息数量
                    bw.Write(sm.Size);//消息大小
                    foreach (uint mo in messOffset)//写入消息区偏移
                    {
                        bw.Write(mo);
                    }
                    byte[] rawData = new byte[sm.Size];
                    int index = 0;
                    foreach (string ms in messString)
                    {
                        byte[] data = shiftJis.GetBytes(ms);
                        Buffer.BlockCopy(data, 0, rawData, index, data.Length);
                        index += data.Length;
                        rawData[index++] = 0;
                    }
                    for (int i = 0; i < rawData.Length; i++)
                    {
                        rawData[i] ^= 0x55;
                    }
                    bw.Write(rawData);//写入加密的消息区

                }
                else //ps.这是无加密情况下的处理代码，下面这块是gpt照着读取写的，出bug我不背锅
                {
                    int index = 0;
                    foreach (string ms in messString)
                    {
                        byte[] data = shiftJis.GetBytes(ms);
                        Buffer.BlockCopy(data, 0, sm.Data, index, data.Length);
                        index += data.Length;
                        sm.Data[index++] = 0;
                    }
                    List<byte> reconstructedData = [];
                    uint currentPosition = 0;
                    for (int i = 0; i < sm.Count; i++)
                    {
                        uint offset = sm.Offset[i];
                        while (currentPosition < sm.Size && sm.Data[currentPosition] != 0)
                        {
                            if (Utils.ISKANJI(sm.Data[currentPosition]))
                            {
                                reconstructedData.Add(sm.Data[currentPosition]);
                                currentPosition++;
                                continue; // Skip the next byte since it is part of the Kanji character
                            }
                            reconstructedData.Add(sm.Data[currentPosition]);
                            currentPosition++;
                        }
                        reconstructedData.Add((byte)'\n');
                        currentPosition++; // Skip the null terminator
                    }
                    while (currentPosition < sm.Size)
                    {
                        if (sm.Data[currentPosition] != 0)
                        {
                            reconstructedData.Add(sm.Data[currentPosition]);
                        }
                        currentPosition++;
                    }
                    byte[] finalData = [.. reconstructedData];
                    bw.Write(finalData);//写入整个数据
                }
            }
            return true;
        }

        public static bool Repackv3(string sqlitePath)
        {
            if (!File.Exists(sqlitePath))
                return false;
            using SqliteConnection connection = new($"Data Source={sqlitePath};");
            connection.Open();

            var tableNames = new List<string>();
            using (var command = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    tableNames.Add(reader.GetString(0));
                }
            }
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding? shiftJis = provider.GetEncoding("shift-jis");
            foreach (var tableName in tableNames)
            {
                string folder = Path.Combine(Path.GetDirectoryName(sqlitePath), "repack");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                string outputPath = Path.Combine(Path.GetDirectoryName(sqlitePath), "repack", tableName + ".bin");
                using FileStream fs = new(outputPath, FileMode.Create);
                using BinaryWriter bw = new(fs);

                string trunkPath = Path.Combine(Path.GetDirectoryName(sqlitePath), tableName + ".dat");
                byte[] bytes = File.ReadAllBytes(trunkPath);
                uint textSizeOffset = 0x10;
                List<uint> textOffset = [];
                List<string> textString = [];
                uint Offset = 0;
                using (var command = new SqliteCommand($"SELECT * FROM {tableName};", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string s = reader.GetString(0);
                        textString.Add(s);
                        textOffset.Add(Offset);
                        Offset += (uint)(shiftJis.GetBytes(s).Length + 1);
                    }
                }
                Buffer.BlockCopy(BitConverter.GetBytes(Offset), 0, bytes, (int)textSizeOffset, 4);
                //准备写入
                bw.Write(bytes);//写入整个数据
                foreach (uint to in textOffset)//写入文本区偏移
                {
                    bw.Write(to);
                }
                foreach (string ts in textString)
                {
                    bw.Write(shiftJis.GetBytes(ts));
                    bw.Write((byte)0);
                }

            }
            return true;
        }
    }
}
