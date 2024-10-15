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
        public List<Command> commands { get; set; } // 给人看的Code内容
    }

    public class Command
    {
        public byte Instruction { get; set; }
        public string InstructionString { get; set; }
        public List<int> Parameter { get; set; }
    }

    public static class Instructions
    {
        public const byte INST_POP = 1;
        public const byte INST_POP_N = 2;
        public const byte INST_POP_RET = 3;
        public const byte INST_PUSH_INT = 4;
        public const byte INST_PUSH_FLOAT = 5;
        public const byte INST_PUSH_RET = 6;
        public const byte INST_PUSH_TEXT = 7;
        public const byte INST_PUSH_MESS = 8;
        public const byte INST_PUSH_GVAR = 9;
        public const byte INST_PUSH_LVAR = 10;
        public const byte INST_STORE_GVAR = 11;
        public const byte INST_STORE_LVAR = 12;
        public const byte INST_ENTER = 13;
        public const byte INST_LEAVE = 14;
        public const byte INST_JMP = 15;
        public const byte INST_JMPZ = 16;
        public const byte INST_CALL = 17;
        public const byte INST_RET = 18;
        public const byte INST_LOG_OR = 19;
        public const byte INST_LOG_AND = 20;
        public const byte INST_LOG_NOT = 21;
        public const byte INST_OR = 22;
        public const byte INST_XOR = 23;
        public const byte INST_AND = 24;
        public const byte INST_NOT = 25;
        public const byte INST_CMP_EQ = 26;
        public const byte INST_CMP_NE = 27;
        public const byte INST_CMP_LT = 28;
        public const byte INST_CMP_LE = 29;
        public const byte INST_CMP_GT = 30;
        public const byte INST_CMP_GE = 31;
        public const byte INST_SHL = 32;
        public const byte INST_SHR = 33;
        public const byte INST_ADD = 34;
        public const byte INST_SUB = 35;
        public const byte INST_MUL = 36;
        public const byte INST_DIV = 37;
        public const byte INST_MOD = 38;
        public const byte INST_NEG = 39;
        public const byte INST_NAME = 40;
        public const byte INST_TEXT = 41;
        public const byte INST_PAGE = 42;
        public const byte INST_OPTION = 43;
        public const byte INST_PROC = 44;
        public const byte INST_LINE = 45;
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
            if(sf.MessCount > 0)
                if(sm.Data.Length == 0) // 怎么判断加没加载呢？这个写的我自己都不是很确定
                    LoadScriptMess(Path.ChangeExtension(path, ".001"));

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
            sf.commands = new List<Command>();
            for (int i = 0; i < sf.CodeSize;)
            {
                Command c = new()
                {
                    Parameter = [],
                    Instruction = sf.Code[i++]
                };
                int paramNum = 0;
                c.InstructionString = GetInstructionString(c.Instruction, out paramNum);
                for (int j = 0; j < paramNum; j++)
                {
                    c.Parameter.Add(BitConverter.ToInt32(sf.Code, i));
                    i += 4;
                }
                sf.commands.Add(c);
            }
            return true;
        }

        private static string GetInstructionString(byte instruction, out int paramNum)
        {
            paramNum = 0;
            string instructionString = instruction switch
            {
                Instructions.INST_POP => "INST_POP",
                Instructions.INST_POP_N => "INST_POP_N",
                Instructions.INST_POP_RET => "INST_POP_RET",
                Instructions.INST_PUSH_INT => "INST_PUSH_INT",
                Instructions.INST_PUSH_FLOAT => "INST_PUSH_FLOAT",
                Instructions.INST_PUSH_RET => "INST_PUSH_RET",
                Instructions.INST_PUSH_TEXT => "INST_PUSH_TEXT",
                Instructions.INST_PUSH_MESS => "INST_PUSH_MESS",
                Instructions.INST_PUSH_GVAR => "INST_PUSH_GVAR",
                Instructions.INST_PUSH_LVAR => "INST_PUSH_LVAR",
                Instructions.INST_STORE_GVAR => "INST_STORE_GVAR",
                Instructions.INST_STORE_LVAR => "INST_STORE_LVAR",
                Instructions.INST_ENTER => "INST_ENTER",
                Instructions.INST_LEAVE => "INST_LEAVE",
                Instructions.INST_JMP => "INST_JMP",
                Instructions.INST_JMPZ => "INST_JMPZ",
                Instructions.INST_CALL => "INST_CALL",
                Instructions.INST_RET => "INST_RET",
                Instructions.INST_LOG_OR => "INST_LOG_OR",
                Instructions.INST_LOG_AND => "INST_LOG_AND",
                Instructions.INST_LOG_NOT => "INST_LOG_NOT",
                Instructions.INST_OR => "INST_OR",
                Instructions.INST_XOR => "INST_XOR",
                Instructions.INST_AND => "INST_AND",
                Instructions.INST_NOT => "INST_NOT",
                Instructions.INST_CMP_EQ => "INST_CMP_EQ",
                Instructions.INST_CMP_NE => "INST_CMP_NE",
                Instructions.INST_CMP_LT => "INST_CMP_LT",
                Instructions.INST_CMP_LE => "INST_CMP_LE",
                Instructions.INST_CMP_GT => "INST_CMP_GT",
                Instructions.INST_CMP_GE => "INST_CMP_GE",
                Instructions.INST_SHL => "INST_SHL",
                Instructions.INST_SHR => "INST_SHR",
                Instructions.INST_ADD => "INST_ADD",
                Instructions.INST_SUB => "INST_SUB",
                Instructions.INST_MUL => "INST_MUL",
                Instructions.INST_DIV => "INST_DIV",
                Instructions.INST_MOD => "INST_MOD",
                Instructions.INST_NEG => "INST_NEG",
                Instructions.INST_NAME => "INST_NAME",
                Instructions.INST_TEXT => "INST_TEXT",
                Instructions.INST_PAGE => "INST_PAGE",
                Instructions.INST_OPTION => "INST_OPTION",
                Instructions.INST_PROC => "INST_PROC",
                Instructions.INST_LINE => "INST_LINE",
                _ => "UNKNOWN INSTRUCTION"
            };

            switch (instruction)
            {
                case Instructions.INST_POP_N:
                case Instructions.INST_PUSH_INT:
                case Instructions.INST_PUSH_FLOAT:
                case Instructions.INST_PUSH_TEXT:
                case Instructions.INST_PUSH_MESS:
                case Instructions.INST_PUSH_GVAR:
                case Instructions.INST_PUSH_LVAR:
                case Instructions.INST_STORE_GVAR:
                case Instructions.INST_STORE_LVAR:
                case Instructions.INST_ENTER:
                case Instructions.INST_JMP:
                case Instructions.INST_JMPZ:
                case Instructions.INST_CALL:
                case Instructions.INST_NAME:
                case Instructions.INST_TEXT:
                case Instructions.INST_PROC:
                case Instructions.INST_LINE:
                    paramNum = 1;
                    break;
                case Instructions.INST_OPTION:
                    paramNum = 2;
                    break;
            }

            return instructionString;
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
