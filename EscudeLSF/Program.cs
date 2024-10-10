using ImageMagick;
using System.Runtime.InteropServices;

namespace EscudeLSF
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LSFHDR
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] signature; // 4 bytes
        public uint unknown1;     // 4 bytes
        public uint unknown2;     // 4 bytes
        public uint width;        // 4 bytes
        public uint height;       // 4 bytes
        public uint unknown_width; // 4 bytes
        public uint unknown_height; // 4 bytes
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LSFENTRY
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public char[] name;           // 128 bytes
        public uint x;                // 4 bytes
        public uint y;                // 4 bytes
        public uint baseWidth;        // 4 bytes
        public uint baseHeight;       // 4 bytes
        public uint unknown3;         // 4 bytes
        public uint unknown4;         // 4 bytes
        public byte type;             // 1 byte
        public byte id;               // 1 byte
        public byte unknown5;         // 1 byte
        public byte unknown6;         // 1 byte
        public uint unknown7;         // 4 bytes
        public uint unknown8;         // 4 bytes
    }

    internal class Program
    {
        private static readonly byte[] Signature = [0x4C, 0x53, 0x46, 0x00];

        static void Main(string[] args)
        {
            if (args.Length == 0 || args.Length > 2)
            {
                Console.WriteLine("Invalid arguments. Use -h for help.");
                return;
            }

            string? workDirectory;
            string? outputDirectory;

            switch (args[0])
            {
                case "-h":
                    DisplayHelp();
                    return;

                case "-r":
                case "-d":
                case "-s":
                    workDirectory = GetWorkDirectory(args);
                    outputDirectory = GetOutputDirectory(workDirectory);
                    if (args[0] == "-d")
                    {
                        ProcessBatch(args[1], workDirectory, outputDirectory);
                    }
                    else
                    {
                        CombineIMG(ReadLSF(args[1]), workDirectory, outputDirectory);
                    }
                    break;

                default:
                    if (!IsValidFileArgument(args))
                    {
                        Console.WriteLine("Invalid arguments. Use -h for help.");
                        return;
                    }
                    workDirectory = Path.GetDirectoryName(args[0]);
                    outputDirectory = GetOutputDirectory(workDirectory);
                    CombineIMG(ReadLSF(args[0]), workDirectory, outputDirectory);
                    break;
            }
        }
        static void DisplayHelp()
        {
            Console.WriteLine("Usage: EscudeLSF.exe [-r <filepath>] [-d <directory>] [-s <filepath>] [-h]");
            Console.WriteLine("Options:");
            Console.WriteLine("  <filepath>     Single lsf process");
            Console.WriteLine("  -r <filepath>  Read single lsf file");
            Console.WriteLine("  -d <directory> Process all lsf files in directory");
            Console.WriteLine("  -s <filepath>  Same as <filepath>");
            Console.WriteLine("  -h             Display help info");
        }

        static string GetOutputDirectory(string workDirectory)
        {
            string outputDirectory = Path.Combine(Path.GetDirectoryName(workDirectory), "output");
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            return outputDirectory;
        }

        static void ProcessBatch(string directory, string workDirectory, string outputDirectory)
        {
            foreach (string file in Directory.GetFiles(directory, "*.lsf"))
            {
                CombineIMG(ReadLSF(file), workDirectory, outputDirectory);
            }
        }

        static string GetWorkDirectory(string[] args)
        {
            return args[0] == "-d" ? args[1] : Path.GetDirectoryName(args[1]);
        }

        static bool IsValidFileArgument(string[] args)
        {
            return args.Length == 2 || (args.Length == 1 && !Path.Exists(args[0]));
        }
        static List<LSFENTRY> ReadLSF(string filePath)
        {
            LSFHDR header;
            List<LSFENTRY> entries = [];

            using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new(fs))
            {
                header = ReadLSFHDR(reader);
                if (!header.signature.SequenceEqual(Signature))
                {
                    Console.WriteLine("Invalid LSF file: Signature does not match.");
                    return [];
                }
                while (fs.Position < fs.Length)
                {
                    LSFENTRY entry = ReadLSFENTRY(reader);

                    // 过滤某些类型(主要是表情，还有审查
                    // 没找到合适的处理方法，有主意的老哥欢迎Pull Request
                    if (entry.type == 0x0B || entry.type == 0x15 || entry.type == 0xFF || (entry.type == 0x00 && entry.unknown5 == 0x03))
                    {
                        continue;
                    }
                    // 过滤重复项(没看出重复的意义，也许是前面过滤掉的某个类型需要
                    if (entries.Any(e => e.name.SequenceEqual(entry.name)))
                    {
                        continue;
                    }

                    entries.Add(entry);
                }
            }
            return entries;
        }

        static LSFHDR ReadLSFHDR(BinaryReader reader)
        {
            byte[] headerData = reader.ReadBytes(Marshal.SizeOf(typeof(LSFHDR)));
            GCHandle handle = GCHandle.Alloc(headerData, GCHandleType.Pinned);
            try
            {
                return (LSFHDR)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(LSFHDR));
            }
            finally
            {
                handle.Free();
            }
        }

        static LSFENTRY ReadLSFENTRY(BinaryReader reader)
        {
            byte[] entryData = reader.ReadBytes(Marshal.SizeOf(typeof(LSFENTRY)));
            GCHandle handle = GCHandle.Alloc(entryData, GCHandleType.Pinned);
            try
            {
                return (LSFENTRY)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(LSFENTRY));
            }
            finally
            {
                handle.Free();
            }
        }

        static void CombineIMG(List<LSFENTRY> entrys, string workDirectory, string storeDir)
        {
            // 检查是否有多个角色
            bool isMultiCharacter = entrys.Any(e => e.type == 0x14);

            foreach (var entry in entrys.Where(e => e.type == 0x00))
            {
                string baseName = new string(entry.name).Trim('\0');
                string targetPath = Path.Combine(storeDir, baseName);
                string baseImagePath = Path.Combine(workDirectory, baseName + ".png");

                if (!File.Exists(baseImagePath))
                {
                    Console.WriteLine($"Error, File not found: {baseName}");
                    continue;
                }

                

                foreach (var eyeEntry in entrys.Where(e => e.type == 0x0A))
                {
                    using var baseImage = new MagickImage(baseImagePath);
                    ProcessEyeImage(entrys, workDirectory, baseImage, eyeEntry, targetPath, isMultiCharacter);
                }
            }
        }

        private static void ProcessEyeImage(List<LSFENTRY> entrys, string workDirectory, MagickImage baseImage, LSFENTRY eyeEntry, string targetPath, bool isMultiCharacter)
        {
            string eyeName = new string(eyeEntry.name).Trim('\0');
            string eyeImagePath = Path.Combine(workDirectory, eyeName + ".png");

            if (!File.Exists(eyeImagePath))
            {
                Console.WriteLine($"Error, File not found: {eyeName}");
                return;
            }

            using var eyeImage = new MagickImage(eyeImagePath);
            var tmpImage = RealProcessIMG(baseImage, eyeImage, (int)eyeEntry.x, (int)eyeEntry.y);
            string target2Path = targetPath + "_" + eyeName;

            if (isMultiCharacter)
            {
                foreach (var eye2Entry in entrys.Where(e => e.type == 0x14))
                {
                    using var tmpImageClone = (MagickImage)tmpImage.Clone();
                    ProcessOverlayImage(eye2Entry, target2Path, workDirectory, tmpImageClone);
                }
            }
            else
            {
                tmpImage.Write(target2Path + ".png");
            }
        }

        private static void ProcessOverlayImage(LSFENTRY eye2Entry, string target2Path, string workDirectory, MagickImage tmpImage)
        {
            string eye2Name = new string(eye2Entry.name).Trim('\0');
            string eye2ImagePath = Path.Combine(workDirectory, eye2Name + ".png");

            if (!File.Exists(eye2ImagePath))
            {
                Console.WriteLine($"Error, File not found: {eye2Name}");
                return;
            }

            using var overlayImage = new MagickImage(eye2ImagePath);
            RealProcessIMG(tmpImage, overlayImage, (int)eye2Entry.x, (int)eye2Entry.y).Write($"{target2Path}_{eye2Name}.png");
        }


        static MagickImage RealProcessIMG(MagickImage b, MagickImage o, int x, int y)
        {
            o.Alpha(AlphaOption.Set);

            // Not working
            // 原先是设计给处理那些不透明的图片，但好像有问题
            //if (args[4] == "y")
            //{
            //    //overlayImage.ColorFuzz = new Percentage(6); // For Face
            //    //overlayImage.Transparent(MagickColors.White); // For Face
            //    //overlayImage.Blur(0, 4); // For Face
            //}

            b.Composite(o, x, y, CompositeOperator.Over);
            return b;
        }
    }
}
