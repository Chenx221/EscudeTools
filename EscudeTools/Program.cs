using Microsoft.Data.Sqlite;

namespace EscudeTools
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ////批量处理EV/ST
            //if (Directory.Exists(args[0]) && File.Exists(args[1]))
            ////if (File.Exists(args[0]))
            //{
            //    string graphicsDBPath = args[1];
            //    using SqliteConnection connection = new($"Data Source={graphicsDBPath};");
            //    connection.Open();
            //    List<string> tableNames = [];
            //    string[] foundTN = new string[3];
            //    List<int> tableIds = [];
            //    bool found1 = false, found2 = false, found3 = false;
            //    using (var command = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection))
            //    using (var reader = command.ExecuteReader())
            //    {
            //        int id = 0;
            //        while (reader.Read())
            //        {
            //            string tableName = reader.GetString(0);
            //            if (tableName.StartsWith("イベント"))
            //            {
            //                foundTN[0] = tableName;
            //                found1 = true;
            //            }
            //            else if (tableName.StartsWith("立ち"))
            //            {
            //                foundTN[1] = tableName;
            //                found2 = true;
            //            }
            //            else if (tableName.StartsWith("表情"))
            //            {
            //                foundTN[2] = tableName;
            //                found3 = true;
            //            }
            //            tableNames.Add(tableName);
            //            tableIds.Add(id++);
            //        }
            //    }
            //    if (!(found1 && found2 && found3)) //这里的代码未经测试
            //    {
            //        for (int i = 0; i < tableNames.Count; i++)
            //            Console.WriteLine($"{tableIds[i]}: {tableNames[i]}");
            //        if (!found1)
            //        {
            //            Console.WriteLine("自动识别失败，请选择存放CG信息的数据表ID: ");
            //            string? input = Console.ReadLine();
            //            if (int.TryParse(input, out int userInputId))
            //            {
            //                if (userInputId >= 0 && userInputId < tableIds.Count)
            //                {
            //                    foundTN[0] = tableNames[userInputId];
            //                }
            //                else
            //                {
            //                    Console.WriteLine("Invalid ID.");
            //                    return;
            //                }
            //            }
            //            else
            //            {
            //                Console.WriteLine("Invalid input. Please enter a valid number.");
            //                return;
            //            }
            //        }
            //        if (!found2)
            //        {
            //            Console.WriteLine("自动识别失败，请选择存放立绘信息的数据表ID: ");
            //            string? input = Console.ReadLine();
            //            if (int.TryParse(input, out int userInputId))
            //            {
            //                if (userInputId >= 0 && userInputId < tableIds.Count)
            //                {
            //                    foundTN[1] = tableNames[userInputId];
            //                }
            //                else
            //                {
            //                    Console.WriteLine("Invalid ID.");
            //                    return;
            //                }
            //            }
            //            else
            //            {
            //                Console.WriteLine("Invalid input. Please enter a valid number.");
            //                return;
            //            }
            //        }
            //        if (!found3)
            //        {
            //            Console.WriteLine("自动识别失败，请选择存放表情信息的数据表ID: ");
            //            string? input = Console.ReadLine();
            //            if (int.TryParse(input, out int userInputId))
            //            {
            //                if (userInputId >= 0 && userInputId < tableIds.Count)
            //                {
            //                    foundTN[2] = tableNames[userInputId];
            //                }
            //                else
            //                {
            //                    Console.WriteLine("Invalid ID.");
            //                    return;
            //                }
            //            }
            //            else
            //            {
            //                Console.WriteLine("Invalid input. Please enter a valid number.");
            //                return;
            //            }
            //        }

            //    }
            //    List<EvTable> evts = [];
            //    List<StTable> stts = [];
            //    Face[] faces = new Face[32];
            //    using (var command = new SqliteCommand($"SELECT * FROM {foundTN[0]};", connection))
            //    {
            //        using var reader = command.ExecuteReader();
            //        while (reader.Read())
            //        {
            //            if (reader.IsDBNull(0) || string.IsNullOrEmpty(reader.GetString(0)))
            //                continue;
            //            evts.Add(new EvTable
            //            {
            //                name = reader.GetString(0),
            //                file = reader.GetString(1),
            //                option = reader.GetString(2).Split(' '),
            //                coverd = (uint)reader.GetInt32(3),
            //                filter = (uint)reader.GetInt32(4),
            //                color = (uint)reader.GetInt32(5),
            //                id = (uint)reader.GetInt32(6),
            //                loc = (uint)reader.GetInt32(7),
            //                order = reader.GetInt32(8),
            //                link = (uint)reader.GetInt32(9)
            //            });
            //        }
            //    }
            //    using (var command = new SqliteCommand($"SELECT * FROM {foundTN[1]};", connection))
            //    {
            //        using var reader = command.ExecuteReader();
            //        while (reader.Read())
            //        {
            //            if (reader.IsDBNull(0) || string.IsNullOrEmpty(reader.GetString(0)))
            //                continue;
            //            stts.Add(new StTable
            //            {
            //                name = reader.GetString(0),
            //                file = reader.GetString(1),
            //                option = reader.GetString(2).Split(' '),
            //                coverd = (uint)reader.GetInt32(3),
            //                filter = (uint)reader.GetInt32(4),
            //                face = (uint)reader.GetInt32(5),
            //                id = (uint)reader.GetInt32(6),
            //                loc = (uint)reader.GetInt32(7),
            //                order = reader.GetInt32(8),
            //                link = (uint)reader.GetInt32(9)
            //            });
            //        }
            //    }
            //    using (var command = new SqliteCommand($"SELECT * FROM {foundTN[2]};", connection))
            //    {
            //        using var reader = command.ExecuteReader();
            //        while (reader.Read())
            //        {
            //            if (reader.IsDBNull(0) || string.IsNullOrEmpty(reader.GetString(0)))
            //                continue;
            //            for (int i = 0; i < faces.Length; i++)
            //            {
            //                if (faces[i] == null)
            //                    faces[i] = new Face();
            //                if (reader.GetInt32(2 + i) == 1)
            //                    faces[i].faceOptions.Add(reader.GetString(1));
            //            }
            //        }
            //    }

            //    string[] files = Directory.GetFiles(args[0], "*.lsf", SearchOption.AllDirectories);
            //    LsfManager lm = new();
            //    foreach (string file in files)
            //    {
            //        if (lm.LoadLsf(file, true))
            //            Console.WriteLine($"Load {file} Success");
            //        else
            //        {
            //            Console.WriteLine($"Load {file} Failed");
            //        }
            //    }
            //    connection.Close();
            //    string outputDir = Path.Combine(Path.GetDirectoryName(args[0]), "Output");
            //    if (!Directory.Exists(outputDir))
            //        Directory.CreateDirectory(outputDir);
            //    var parallelOptions = new ParallelOptions
            //    {
            //        MaxDegreeOfParallelism = 6 // 设置最大并行线程数
            //    };

            //    //    //ST //表情还要另取？
            //    //    Parallel.ForEach(stts, parallelOptions, stt =>
            //    //    //foreach (StTable stt in stts)
            //    //    {
            //    //        if (stt.order == 0) //仅提取鉴赏中有的ST
            //    //                            return;
            //    //        //continue;
            //    //        string targetFilename = Path.Combine(outputDir, stt.name); //最后保存可用的文件名
            //    //        LsfData? lsfData = lm.FindLsfDataByName(stt.file) ?? throw new Exception($"错误，未找到与{stt.file}对应的lsf数据");
            //    //        List<int> pendingList = [];
            //    //        List<string> pendingListFn = [];
            //    //        foreach (string o in stt.option)
            //    //        {
            //    //            List<int> t = TableManagercs.ParseOptions(lsfData, o);
            //    //            if (t.Count == 0)
            //    //                continue;
            //    //            pendingList.AddRange(t);
            //    //            foreach (int i in t)
            //    //            {
            //    //                pendingListFn.Add(lsfData.lli[i].nameStr);
            //    //            }
            //    //        }
            //    //        pendingList = TableManagercs.OrderLayer(pendingList, pendingListFn);
            //    //        int n = 0;
            //    //        foreach (string o in faces[(int)stt.face].faceOptions)
            //    //        {
            //    //            List<int> pendingListCopy = new(pendingList);
            //    //            List<int> t = TableManagercs.ParseOptions(lsfData, o);
            //    //            if (t.Count == 0)
            //    //                continue;
            //    //            pendingListCopy.AddRange(t);
            //    //            if (!ImageManager.Process(lsfData, [.. pendingListCopy], targetFilename + $"_{n++}.png"))
            //    //                throw new Exception("Process Fail");
            //    //            else
            //    //                Console.WriteLine($"Export {stt.name}_{n - 1} Success");
            //    //        }
            //    //        });
            //    ////}

            //    //EV
            //    Parallel.ForEach(evts, parallelOptions, evt =>
            //    //foreach (EvTable evt in evts)
            //{
            //    if (evt.order == 0) //仅提取鉴赏中有的CG
            //        return;
            //    //continue;
            //    string targetFilename = Path.Combine(outputDir, evt.name + ".png"); //最后保存可用的文件名
            //    LsfData lsfData = lm.FindLsfDataByName(evt.file) ?? throw new Exception("Something Wrong");
            //    List<int> pendingList = [];
            //    List<string> pendingListFn = [];
            //    foreach (string o in evt.option)
            //    {
            //        List<int> t = TableManagercs.ParseOptions(lsfData, o);
            //        if (t.Count == 0)
            //            continue;
            //        pendingList.AddRange(t);
            //        foreach (int i in t)
            //        {
            //            pendingListFn.Add(lsfData.lli[i].nameStr);
            //        }
            //    }
            //    pendingList = TableManagercs.OrderLayer(pendingList, pendingListFn);
            //    if (pendingList[0] != 0)
            //        pendingList.Insert(0, 0);
            //    if (!ImageManager.Process(lsfData, [.. pendingList], targetFilename))
            //        throw new Exception("Process Fail");
            //    else
            //        Console.WriteLine($"Export {evt.name} Success");
            //});
            //    //}
            //}








            //// 批量读取lsf文件
            //if (Directory.Exists(args[0]))
            //{
            //    string[] files = Directory.GetFiles(args[0], "*.lsf", SearchOption.AllDirectories);
            //    LsfManager lm = new();
            //    foreach (string file in files)
            //    {
            //        if (lm.LoadLsf(file, true))
            //            Console.WriteLine($"Load {file} Success");
            //        else
            //        {
            //            Console.WriteLine($"Load {file} Failed");
            //        }
            //    }
            //    Console.WriteLine("OK");

            //}




            // NOTE
            // 推荐使用DB Browser for SQLite (https://sqlitebrowser.org/) 查看、编辑导出的数据库文件
            // 这不是广告，这只是我在开发期间使用的工具

            ////Batch Unpack ESC-ARC Package
            //if (Directory.Exists(args[0]))
            //{
            //    string[] files = Directory.GetFiles(args[0], "*.bin");
            //    PackManager pm = new();
            //    foreach (string file in files)
            //    {
            //        if (pm.Load(file))
            //        {
            //            Console.WriteLine($"Load {file} Success");
            //            if (pm.Extract())
            //                Console.WriteLine("Extract Package Success");
            //            else
            //                Console.WriteLine("Extract Package Failed");
            //        }
            //        else
            //            Console.WriteLine($"Load {file} Failed");
            //    }
            //}

            //Batch Repack ESC-ARC Package
            if (Directory.Exists(args[0]))// && Directory.Exists(args[1])
            {
                string[] directories = Directory.GetDirectories(args[0]);
                foreach (string directory in directories)
                {
                    PackManager pm = new();
                    //string providerFilePath = Path.Combine(args[1], Path.GetFileName(directory) + ".bin");
                    if (pm.Repack(directory, 2, true))
                        Console.WriteLine("Repack Package Success");
                    else
                    {
                        Console.WriteLine("Repack Package Failed");
                    }
                }
            }


            ////Batch Unpack Script(Full, Text, Mess)
            //if (Directory.Exists(args[0]))
            //{
            //    string[] files = Directory.GetFiles(args[0], "*.bin");
            //    foreach (string file in files)
            //    {
            //        ScriptManager smr = new();
            //        if (smr.LoadScriptFile(file))
            //        {
            //            Console.WriteLine($"Load {file} Success");
            //        }
            //        else
            //        {
            //            Console.WriteLine($"Load {file} Failed");
            //            return;
            //        }
            //        if (smr.ExportDatabase(Path.GetDirectoryName(args[0])))
            //            Console.WriteLine("Export Script Success");
            //        else
            //        {
            //            Console.WriteLine("Export Script Failed");
            //            return;
            //        }
            //        if (smr.ExportTextDatabase(Path.GetDirectoryName(args[0])))
            //            Console.WriteLine("Export Text Success");
            //        else
            //        {
            //            Console.WriteLine("Export Text Failed");
            //            return;
            //        }
            //        if (smr.ExportMessDatabase(Path.GetDirectoryName(args[0])))
            //            Console.WriteLine("Export Mess Success");
            //        else
            //        {
            //            Console.WriteLine("Export Mess Failed");
            //            return;
            //        }
            //    }
            //}

            ////Export Full Script
            //if (File.Exists(args[0])) //fail //lost 1 //something diff
            //{
            //    ScriptManager.Repackv1(args[0], true);
            //}

            ////Export ScriptMessage
            //if (File.Exists(args[1])) //pass
            //{
            //    ScriptManager.Repackv2(args[1], true);
            //}

            //////Export ScriptFile
            //if (File.Exists(args[2])) //pass
            //{
            //    ScriptManager.Repackv3(args[2]);
            //}



            //ScriptManager smr = new();
            //smr.LoadScriptFile(args[0]); //加载.bin文件
            //smr.ExportDatabase(Path.GetDirectoryName(args[0]));
            //smr.ExportMessDatabase(Path.GetDirectoryName(args[0]));
            //return;

            ////repack sqlite to bin
            //if (Directory.Exists(args[0]))
            //{
            //    string[] files = Directory.GetFiles(args[0], "*.db");

            //    foreach (string file in files)
            //    {
            //        DatabaseManager.ExportMDB(file);

            //    }
            //}


            //if (Directory.Exists(args[0]))
            //{
            //    string[] files = Directory.GetFiles(args[0], "*.bin");
            //    DatabaseManager dm = new();
            //    foreach (string file in files)
            //    {
            //        dm.LoadDatabase(file);
            //        if (dm.ExportDatabase(Path.GetDirectoryName(args[0])))
            //            Console.WriteLine("Export Database Success");
            //    }

            //}

            //导出db_*.bin
            if (Directory.Exists(args[0]))
            {
                string[] files = Directory.GetFiles(args[0], "db_*.bin");
                DatabaseManager dm = new();
                foreach (string file in files)
                {
                    if (dm.LoadDatabase(file))
                    {
                        Console.WriteLine($"Load {file} Success");
                    }
                    else
                    {
                        Console.WriteLine($"Load {file} Failed");
                        return;
                    }

                    if (dm.ExportDatabase(Path.GetDirectoryName(args[0])))
                        Console.WriteLine("Export Database Success");
                    else
                    {
                        Console.WriteLine("Export Database Failed");
                        return;
                    }

                }

            }


            //    if (args.Length == 0 || args.Length > 2)
            //    {
            //        Console.WriteLine("Invalid arguments. Use -h for help.");
            //        return;
            //    }

            //    switch (args[0])
            //    {
            //        case "-h":
            //        case "-r":
            //        case "-d":
            //        case "-s":
            //        default:
            //            break;
            //    }
            //}
            //static void DisplayHelp()
            //{
            //    Console.WriteLine("Usage: EscudeTools.exe [-r <filepath>] [-d <directory>] [-s <filepath>] [-h]");
            //    Console.WriteLine("Options:");
            //    Console.WriteLine("  <filepath>     Single lsf process");
            //    Console.WriteLine("  -r <filepath>  Read single lsf file");
            //    Console.WriteLine("  -d <directory> Process all lsf files in directory");
            //    Console.WriteLine("  -s <filepath>  Same as <filepath>");
            //    Console.WriteLine("  -h             Display help info");
            //}
        }

    }
}
