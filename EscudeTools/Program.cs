using Microsoft.Data.Sqlite;

namespace EscudeTools
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
                switch (args[0])
                {
                    case "-h":
                        PrintHelp();
                        break;
                    default:
                        InvalidArgument();
                        break;
                }
            else if (args.Length == 2)
                switch (args[0])
                {
                    case "-u":
                        UnpackEscArc(args[1]);
                        break;
                    case "-r":
                        RepackEscArc(args[1]);
                        break;
                    case "-d":
                        UnpackMdb(args[1]);
                        break;
                    case "-f":
                        RepackMdb(args[1]);
                        break;
                    default:
                        InvalidArgument();
                        break;
                }
            else if (args.Length == 3)
                switch (args[0])
                {
                    case "-c":
                        EvProcess(args[1], args[2]);
                        break;
                    case "-s":
                        StProcess(args[1], args[2]);
                        break;
                }
            else if (args.Length == 4)
            {
                switch (args[0])
                {
                    case "-r":
                        if (args[2] == "-c")
                            RepackEscArc(args[1], args[3]);
                        else
                            InvalidArgument();
                        break;
                    case "-v":
                        if (args[2] == "-t")
                            UnpackScript(args[1], args[3]);
                        else
                            InvalidArgument();
                        break;
                    case "-e":
                        if (args[2] == "-t")
                            switch (args[3])
                            {
                                case "0":
                                    RepackScript0(args[1]);
                                    break;
                                case "1":
                                    RepackScript1(args[1]);
                                    break;
                                case "2":
                                    RepackScript2(args[1]);
                                    break;
                                default:
                                    InvalidArgument();
                                    break;
                            }
                        else
                            InvalidArgument();
                        break;
                    default:
                        InvalidArgument();
                        break;
                }
            }
            else if (args.Length == 5)
                switch (args[0])
                {
                    case "-e":
                        bool b;
                        if (!bool.TryParse(args[4], out b))
                        {
                            InvalidArgument();
                            break;
                        }
                        if (args[2] == "-t")
                            switch (args[3])
                            {
                                case "0":
                                    RepackScript0(args[1], b);
                                    break;
                                case "1":
                                    RepackScript1(args[1], b);
                                    break;
                                default:
                                    InvalidArgument();
                                    break;
                            }
                        else
                            InvalidArgument();
                        break;
                    default:
                        InvalidArgument();
                        break;
                }
            else
                InvalidArgument();
            Console.ReadKey();
        }
        private static void StProcess(string v1, string v2)
        {
            if (Directory.Exists(v1) && File.Exists(v2))
            {
                string graphicsDBPath = v2;
                using SqliteConnection connection = new($"Data Source={graphicsDBPath};");
                connection.Open();
                List<string> tableNames = [];
                string[] foundTN = new string[2];
                List<int> tableIds = [];
                bool found2 = false, found3 = false;
                using (var command = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection))
                using (var reader = command.ExecuteReader())
                {
                    int id = 0;
                    while (reader.Read())
                    {
                        string tableName = reader.GetString(0);
                        if (tableName.StartsWith("立ち"))
                        {
                            foundTN[0] = tableName;
                            found2 = true;
                        }
                        else if (tableName.StartsWith("表情"))
                        {
                            foundTN[1] = tableName;
                            found3 = true;
                        }
                        tableNames.Add(tableName);
                        tableIds.Add(id++);
                    }
                }
                if (!(found2 && found3)) //这里的代码未经测试
                {
                    for (int i = 0; i < tableNames.Count; i++)
                        Console.WriteLine($"{tableIds[i]}: {tableNames[i]}");
                    if (!found2)
                    {
                        Console.WriteLine("自动识别失败，请选择存放立绘信息的数据表ID: ");
                        string? input = Console.ReadLine();
                        if (int.TryParse(input, out int userInputId))
                        {
                            if (userInputId >= 0 && userInputId < tableIds.Count)
                            {
                                foundTN[0] = tableNames[userInputId];
                            }
                            else
                            {
                                Console.WriteLine("Invalid ID.");
                                return;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid input. Please enter a valid number.");
                            return;
                        }
                    }
                    if (!found3)
                    {
                        Console.WriteLine("自动识别失败，请选择存放表情信息的数据表ID: ");
                        string? input = Console.ReadLine();
                        if (int.TryParse(input, out int userInputId))
                        {
                            if (userInputId >= 0 && userInputId < tableIds.Count)
                            {
                                foundTN[1] = tableNames[userInputId];
                            }
                            else
                            {
                                Console.WriteLine("Invalid ID.");
                                return;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid input. Please enter a valid number.");
                            return;
                        }
                    }

                }
                List<StTable> stts = [];
                Face[] faces = new Face[32];
                using (var command = new SqliteCommand($"SELECT * FROM {foundTN[1]};", connection))
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0) || string.IsNullOrEmpty(reader.GetString(0)))
                            continue;
                        stts.Add(new StTable
                        {
                            name = reader.GetString(0),
                            file = reader.GetString(1),
                            option = reader.GetString(2).Split(' '),
                            coverd = (uint)reader.GetInt32(3),
                            filter = (uint)reader.GetInt32(4),
                            face = (uint)reader.GetInt32(5),
                            id = (uint)reader.GetInt32(6),
                            loc = (uint)reader.GetInt32(7),
                            order = reader.GetInt32(8),
                            link = (uint)reader.GetInt32(9)
                        });
                    }
                }
                using (var command = new SqliteCommand($"SELECT * FROM {foundTN[2]};", connection))
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0) || string.IsNullOrEmpty(reader.GetString(0)))
                            continue;
                        for (int i = 0; i < faces.Length; i++)
                        {
                            if (faces[i] == null)
                                faces[i] = new Face();
                            if (reader.GetInt32(2 + i) == 1)
                                faces[i].faceOptions.Add(reader.GetString(1));
                        }
                    }
                }

                string[] files = Directory.GetFiles(v1, "*.lsf", SearchOption.AllDirectories);
                LsfManager lm = new();
                foreach (string file in files)
                {
                    if (lm.LoadLsf(file, true))
                        Console.WriteLine($"Load {file} Success");
                    else
                    {
                        Console.WriteLine($"Load {file} Failed");
                    }
                }
                connection.Close();
                string outputDir = Path.Combine(Path.GetDirectoryName(v1), "Output");
                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 6 // 设置最大并行线程数
                };

                Parallel.ForEach(stts, parallelOptions, stt =>
                //foreach (StTable stt in stts)
                {
                    if (stt.order == 0) //仅提取鉴赏中有的ST
                        return;
                    //continue;
                    string targetFilename = Path.Combine(outputDir, stt.name); //最后保存可用的文件名
                    LsfData? lsfData = lm.FindLsfDataByName(stt.file) ?? throw new Exception($"错误，未找到与{stt.file}对应的lsf数据");
                    List<int> pendingList = [];
                    List<string> pendingListFn = [];
                    foreach (string o in stt.option)
                    {
                        List<int> t = TableManagercs.ParseOptions(lsfData, o);
                        if (t.Count == 0)
                            continue;
                        pendingList.AddRange(t);
                        foreach (int i in t)
                        {
                            pendingListFn.Add(lsfData.lli[i].nameStr);
                        }
                    }
                    pendingList = TableManagercs.OrderLayer(pendingList, pendingListFn);
                    int n = 0;
                    foreach (string o in faces[(int)stt.face].faceOptions)
                    {
                        List<int> pendingListCopy = new(pendingList);
                        List<int> t = TableManagercs.ParseOptions(lsfData, o);
                        if (t.Count == 0)
                            continue;
                        pendingListCopy.AddRange(t);
                        if (!ImageManager.Process(lsfData, [.. pendingListCopy], targetFilename + $"_{n++}.png"))
                            throw new Exception("Process Fail");
                        else
                            Console.WriteLine($"Export {stt.name}_{n - 1} Success");
                    }
                });
                //}

            }
        }

        private static void EvProcess(string v1, string v2)
        {
            if (Directory.Exists(v1) && File.Exists(v2))
            {
                string graphicsDBPath = v1;
                using SqliteConnection connection = new($"Data Source={graphicsDBPath};");
                connection.Open();
                List<string> tableNames = [];
                string[] foundTN = new string[1];
                List<int> tableIds = [];
                bool found1 = false;
                using (var command = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection))
                using (var reader = command.ExecuteReader())
                {
                    int id = 0;
                    while (reader.Read())
                    {
                        string tableName = reader.GetString(0);
                        if (tableName.StartsWith("イベント"))
                        {
                            foundTN[0] = tableName;
                            found1 = true;
                        }
                        tableNames.Add(tableName);
                        tableIds.Add(id++);
                    }
                }
                if (!found1) //这里的代码未经测试
                {
                    for (int i = 0; i < tableNames.Count; i++)
                        Console.WriteLine($"{tableIds[i]}: {tableNames[i]}");
                    if (!found1)
                    {
                        Console.WriteLine("自动识别失败，请选择存放CG信息的数据表ID: ");
                        string? input = Console.ReadLine();
                        if (int.TryParse(input, out int userInputId))
                        {
                            if (userInputId >= 0 && userInputId < tableIds.Count)
                            {
                                foundTN[0] = tableNames[userInputId];
                            }
                            else
                            {
                                Console.WriteLine("Invalid ID.");
                                return;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid input. Please enter a valid number.");
                            return;
                        }
                    }
                }
                List<EvTable> evts = [];
                using (var command = new SqliteCommand($"SELECT * FROM {foundTN[0]};", connection))
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0) || string.IsNullOrEmpty(reader.GetString(0)))
                            continue;
                        evts.Add(new EvTable
                        {
                            name = reader.GetString(0),
                            file = reader.GetString(1),
                            option = reader.GetString(2).Split(' '),
                            coverd = (uint)reader.GetInt32(3),
                            filter = (uint)reader.GetInt32(4),
                            color = (uint)reader.GetInt32(5),
                            id = (uint)reader.GetInt32(6),
                            loc = (uint)reader.GetInt32(7),
                            order = reader.GetInt32(8),
                            link = (uint)reader.GetInt32(9)
                        });
                    }
                }
                string[] files = Directory.GetFiles(v1, "*.lsf", SearchOption.AllDirectories);
                LsfManager lm = new();
                foreach (string file in files)
                {
                    if (lm.LoadLsf(file, true))
                        Console.WriteLine($"Load {file} Success");
                    else
                    {
                        Console.WriteLine($"Load {file} Failed");
                    }
                }
                connection.Close();
                string outputDir = Path.Combine(Path.GetDirectoryName(v1), "Output");
                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 6 // 设置最大并行线程数
                };

                Parallel.ForEach(evts, parallelOptions, evt =>
                //foreach (EvTable evt in evts)
            {
                if (evt.order == 0) //仅提取鉴赏中有的CG
                    return;
                //continue;
                string targetFilename = Path.Combine(outputDir, evt.name + ".png"); //最后保存可用的文件名
                LsfData lsfData = lm.FindLsfDataByName(evt.file) ?? throw new Exception("Something Wrong");
                List<int> pendingList = [];
                List<string> pendingListFn = [];
                foreach (string o in evt.option)
                {
                    List<int> t = TableManagercs.ParseOptions(lsfData, o);
                    if (t.Count == 0)
                        continue;
                    pendingList.AddRange(t);
                    foreach (int i in t)
                    {
                        pendingListFn.Add(lsfData.lli[i].nameStr);
                    }
                }
                pendingList = TableManagercs.OrderLayer(pendingList, pendingListFn);
                if (pendingList[0] != 0)
                    pendingList.Insert(0, 0);
                if (!ImageManager.Process(lsfData, [.. pendingList], targetFilename))
                    throw new Exception("Process Fail");
                else
                    Console.WriteLine($"Export {evt.name} Success");
            });
                //}
            }
        }


        private static void RepackScript2(string v)
        {
            //TEXT
            if (File.Exists(v))
                ScriptManager.Repackv3(v);
            else
                Console.WriteLine("Invalid Path");
        }

        private static void RepackScript1(string v)
        {
            RepackScript1(v, true);
        }

        private static void RepackScript0(string v)
        {
            RepackScript0(v, true);
        }

        private static void RepackScript1(string v1, bool v2)
        {
            //MESS
            if (File.Exists(v1))
                ScriptManager.Repackv2(v1, v2);
            else
                Console.WriteLine("Invalid Path");
        }

        private static void RepackScript0(string v1, bool v2)
        {
            //FULL
            if (File.Exists(v1))
                ScriptManager.Repackv1(v1, v2);
            else
                Console.WriteLine("Invalid Path");
        }

        private static void UnpackScript(string v1, string v2)
        {
            //Batch Unpack Script(Full, Text, Mess)
            if (Directory.Exists(v1))
            {
                string[] files = Directory.GetFiles(v1, "*.bin");
                foreach (string file in files)
                {
                    ScriptManager smr = new();
                    if (smr.LoadScriptFile(file))
                    {
                        Console.WriteLine($"Load {file} Success");
                        switch (v2)
                        {
                            case "0":
                                if (smr.ExportDatabase(Path.GetDirectoryName(v1)))
                                    Console.WriteLine("Export Script Success");
                                else
                                    Console.WriteLine("Export Script Failed");
                                break;
                            case "1":
                                if (smr.ExportMessDatabase(Path.GetDirectoryName(v1)))
                                    Console.WriteLine("Export Mess Success");
                                else
                                    Console.WriteLine("Export Mess Failed");
                                break;
                            case "2":
                                if (smr.ExportTextDatabase(Path.GetDirectoryName(v1)))
                                    Console.WriteLine("Export Text Success");
                                else
                                    Console.WriteLine("Export Text Failed");
                                break;
                            default:
                                InvalidArgument();
                                return;
                        }
                    }
                    else
                        Console.WriteLine($"Load {file} Failed");
                }
            }
        }

        private static void RepackMdb(string v)
        {
            //repack sqlite to bin
            if (Directory.Exists(v))
            {
                string[] files = Directory.GetFiles(v, "*.db");
                foreach (string file in files)
                {
                    DatabaseManager.ExportMDB(file);
                }
            }
            else
                Console.WriteLine("Invalid Path");
        }

        private static void UnpackMdb(string v)
        {
            //导出db_*.bin
            if (Directory.Exists(v))
            {
                string[] files = Directory.GetFiles(v, "db_*.bin");
                DatabaseManager dm = new();
                foreach (string file in files)
                {
                    if (dm.LoadDatabase(file))
                    {
                        Console.WriteLine($"Load {file} Success");
                        if (dm.ExportDatabase(Path.GetDirectoryName(v)))
                            Console.WriteLine("Export Database Success");
                        else
                            Console.WriteLine("Export Database Failed");
                    }
                    else
                        Console.WriteLine($"Load {file} Failed");
                }
            }
            else
                Console.WriteLine("Invalid Path");
        }

        private static void RepackEscArc(string v, string v1)
        {
            //Batch Repack ESC-ARC Package
            if (Directory.Exists(v) && (v1 == "" || File.Exists(v1)))
            {
                string[] directories = Directory.GetDirectories(v);
                foreach (string directory in directories)
                {
                    PackManager pm = new();
                    //string providerFilePath = Path.Combine(args[1], Path.GetFileName(directory) + ".bin");
                    if (pm.Repack(directory, 2, v1))
                        Console.WriteLine("Repack Package Success");
                    else
                        Console.WriteLine("Repack Package Failed");
                }
            }
            else
                Console.WriteLine("Invalid Path");
        }

        private static void RepackEscArc(string v)
        {
            RepackEscArc(v, "");
        }

        private static void UnpackEscArc(string v)
        {
            //Batch Unpack ESC-ARC Package
            if (Directory.Exists(v))
            {
                string[] files = Directory.GetFiles(v, "*.bin");
                PackManager pm = new();
                foreach (string file in files)
                {
                    if (pm.Load(file))
                    {
                        Console.WriteLine($"Load {file} Success");
                        if (pm.Extract())
                            Console.WriteLine("Extract Package Success");
                        else
                            Console.WriteLine("Extract Package Failed");
                    }
                    else
                        Console.WriteLine($"Load {file} Failed");
                }
            }
            else
                Console.WriteLine("Invalid Path");
        }

        private static void InvalidArgument()
        {
            Console.WriteLine("Invalid arguments. Use -h for help.");
        }

        public static void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("EscudeTools.exe -u <path>");
            Console.WriteLine("-u: unpack ESC-ARC bin");
            Console.WriteLine("Unpacks all ESC-ARC bin files in the specified directory path.");
            Console.WriteLine("Unpacked contents will be saved in the 'output' directory.");
            Console.WriteLine("*.json files include package information; do not delete them (use for repack).");
            Console.WriteLine("lzwManifest.json file contains LZW file information; delete it if you want to repack the package without LZW compression.");
            Console.WriteLine();
            Console.WriteLine("EscudeTools.exe -r (-c) <path>");
            Console.WriteLine("-r: repack ESC-ARC bin");
            Console.WriteLine("-r -c: repack ESC-ARC bin & use a custom key from an existing ESC-ARC bin.");
            Console.WriteLine("Repacks all directories in the specified directory path to ESC-ARC bin.");
            Console.WriteLine("Accepts an optional -c flag to use a custom key from an existing ESC-ARC bin.");
            Console.WriteLine("Default key is ... (check it in the source code).");
            Console.WriteLine("For notes about JSON files in the output directory, please refer to unpack usage.");
            Console.WriteLine();
            Console.WriteLine("EscudeTools.exe -v <path> -t <type>");
            Console.WriteLine("-v -t: unpack script bin, type");
            Console.WriteLine("Unpacks all script bin files in the specified directory path to SQLite database.");
            Console.WriteLine("Ignore the 001 files; the program will read them if needed.");
            Console.WriteLine("Must specify the type of script bin file to unpack.");
            Console.WriteLine("Accepts the following types: 0, 1, 2.");
            Console.WriteLine("Type 0: Full; this creates script.db containing all .bin and .001 information.");
            Console.WriteLine("Type 2: Exports only the text from bin; this creates script_text.db and many .dat files (non-text data).");
            Console.WriteLine("Type 1: Exports only the text from 001; this creates script_sm.db containing all .001 information.");
            Console.WriteLine();
            Console.WriteLine("EscudeTools.exe -e <path> -t <type>");
            Console.WriteLine("-e -t: repack script bin, type");
            Console.WriteLine("Repacks all SQLite database files in the specified directory path to script bin.");
            Console.WriteLine("Must specify the type of script bin file to repack.");
            Console.WriteLine("Accepts the following types: 0, 1, 2.");
            Console.WriteLine("Type 0: Full; this generates .bin and .001 files.");
            Console.WriteLine("Type 2: This generates .bin files.");
            Console.WriteLine("Type 1: This generates .001 files.");
            Console.WriteLine();
            Console.WriteLine("EscudeTools.exe -d <path>");
            Console.WriteLine("-d: unpack db_*.bin to SQLite");
            Console.WriteLine("Exports all db_*.bin files in the path to individual SQLite databases.");
            Console.WriteLine();
            Console.WriteLine("EscudeTools.exe -f <path>");
            Console.WriteLine("-f: repack SQLite to db_*.bin");
            Console.WriteLine("Restores all SQLite databases in the path to db_*.bin files.");
            Console.WriteLine();
            Console.WriteLine("EscudeTools.exe -h");
            Console.WriteLine("-h: print help info");
        }

    }

}
