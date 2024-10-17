namespace EscudeTools
{
    internal class Program
    {
        static void Main(string[] args)
        {


            //if (Directory.Exists(args[0]))
            //{
            //    string[] files = Directory.GetFiles(args[0], "*.bin");
            //    PackManager pm = new();
            //    foreach (string file in files)
            //    {
            //        //if (pm.Load(file))
            //        //{
            //        //    Console.WriteLine($"Load {file} Success");
            //        //}
            //        //else
            //        //{
            //        //    Console.WriteLine($"Load {file} Failed");
            //        //    return;
            //        //}

            //        //if (pm.Extract())
            //        //    Console.WriteLine("Extract Package Success");
            //        //else
            //        //{
            //        //    Console.WriteLine("Extract Package Failed");
            //        //    return;
            //        //}

            //        if (pm.Repack(args[1], 2))
            //            Console.WriteLine("Repack Package Success");
            //        else
            //        {
            //            Console.WriteLine("Repack Package Failed");
            //            return;
            //        }
            //    }

            //}

            //PackManager pm = new();
            //if (pm.Repack(args[1]))
            //    Console.WriteLine("Export Database Success");
            //else
            //{
            //    Console.WriteLine("Export Database Failed");
            //    return;
            //}

            //if (Directory.Exists(args[0]))
            //{
            //    string[] files = Directory.GetFiles(args[0], "*.bin");
            //    foreach (string file in files)
            //    {
            //        ScriptManager smr = new();
            //        //目前不支持二次加载
            //        //Todo
            //        //修复
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
            //            Console.WriteLine("Export Database Success");
            //        else
            //        {
            //            Console.WriteLine("Export Database Failed");
            //            return;
            //        }

            //        if (smr.ExportMessDatabase(Path.GetDirectoryName(args[0])))
            //            Console.WriteLine("Export Mess Database Success");
            //        else
            //        {
            //            Console.WriteLine("Export Mess Database Failed");
            //            return;
            //        }
            //    }

            //}


            //ScriptManager smr = new();
            //smr.LoadScriptFile(args[0]); //加载.bin文件
            //smr.ExportDatabase(Path.GetDirectoryName(args[0]));
            //smr.ExportMessDatabase(Path.GetDirectoryName(args[0]));
            //return;


            if (Directory.Exists(args[0]))
            {
                string[] files = Directory.GetFiles(args[0], "*.db");

                foreach (string file in files)
                {
                    DatabaseManager.ExportMDB(file);

                }
            }
            //}
            //if (Directory.Exists(args[0]))
            //{
            //    string[] files = Directory.GetFiles(args[0], "*.bin");
            //    DatabaseManager dm = new DatabaseManager();
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
