namespace EscudeTools
{
    internal class Program
    {
        static void Main(string[] args)
        {
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
            //        }
            //        else
            //        {
            //            Console.WriteLine($"Load {file} Failed");
            //        }

            //        if (pm.Extract())
            //            Console.WriteLine("Extract Package Success");
            //        else
            //        {
            //            Console.WriteLine("Extract Package Failed");
            //        }
            //    }
            //}

            ////Batch Repack ESC-ARC Package
            //if (Directory.Exists(args[0]) && Directory.Exists(args[1]))
            //{
            //    string[] directories = Directory.GetDirectories(args[0]);
            //    foreach (string directory in directories)
            //    {
            //        PackManager pm = new();
            //        string providerFilePath = Path.Combine(args[1], Path.GetFileName(directory) + ".bin");
            //        if (pm.Repack(directory, 2,true, providerFilePath))
            //            Console.WriteLine("Repack Package Success");
            //        else
            //        {
            //            Console.WriteLine("Repack Package Failed");
            //            return;
            //        }
            //    }
            //}


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

            //Export Full Script
            if (File.Exists(args[0])) //fail //lost 1 //something diff
            {
                ScriptManager.Repackv1(args[0], true);
            }

            //Export ScriptMessage
            if (File.Exists(args[1])) //pass
            {
                ScriptManager.Repackv2(args[1], true);
            }

            ////Export ScriptFile
            if (File.Exists(args[2])) //pass
            {
                ScriptManager.Repackv3(args[2]);
            }



            //ScriptManager smr = new();
            //smr.LoadScriptFile(args[0]); //加载.bin文件
            //smr.ExportDatabase(Path.GetDirectoryName(args[0]));
            //smr.ExportMessDatabase(Path.GetDirectoryName(args[0]));
            //return;


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


            //if (Directory.Exists(args[0]))
            //{
            //    string[] files = Directory.GetFiles(args[0], "db_*.bin");
            //    DatabaseManager dm = new();
            //    foreach (string file in files)
            //    {
            //        if (dm.LoadDatabase(file))
            //        {
            //            Console.WriteLine($"Load {file} Success");
            //        }
            //        else
            //        {
            //            Console.WriteLine($"Load {file} Failed");
            //            return;
            //        }

            //        if (dm.ExportDatabase(Path.GetDirectoryName(args[0])))
            //            Console.WriteLine("Export Database Success");
            //        else
            //        {
            //            Console.WriteLine("Export Database Failed");
            //            return;
            //        }

            //    }

            //}


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
