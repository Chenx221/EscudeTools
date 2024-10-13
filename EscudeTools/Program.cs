﻿namespace EscudeTools
{
    internal class Program
    {
        static void Main(string[] args)
        {
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

                    if (dm.ExportDatabase(0, Path.GetDirectoryName(args[0])))
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