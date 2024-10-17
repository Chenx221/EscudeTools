using Microsoft.Data.Sqlite;
using System.Reflection;

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
        public Object Parameter { get; set; }
        public String Helper { get; set; }
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
                    IsProcSet = false
                };
                if (enableCommandHelper)
                {
                    c.InstructionString = Define.GetInstructionString(c.Instruction, out int paramNum);
                    for (int j = 0; j < paramNum; j++)
                    {
                        c.Parameter = Define.TyperHelper(c.Instruction, sf.Code, i);
                        i += 4;
                    }
                    if (sm != null)
                        c.Helper = Define.SetCommandStr(c, sf, sm, ref messIndex);
                }
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

        public bool ExportDatabase(string? storePath)
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

        public bool ExportMessDatabase(string? storePath)
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

        private bool SqliteProcess(ScriptFile sf, string path)
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
            Offset INTEGER,
            Instruction INTEGER,
            InstructionString TEXT,
            Parameter TEXT,
            Helper TEXT
        );";
            using (var createTableCmd = new SqliteCommand(createTableQuery, connection))
            {
                createTableCmd.ExecuteNonQuery();
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
                insertCmd.Parameters.AddWithValue("@Parameter", command.Parameter ?? "");
                insertCmd.Parameters.AddWithValue("@Helper", command.Helper ?? "");

                insertCmd.ExecuteNonQuery();
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
    }
}
