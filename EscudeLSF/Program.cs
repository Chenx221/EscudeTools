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


            switch (args[0])
            {
                case "-h":
                    return;

                case "-r":
                case "-d":
                case "-s":
                    break;

                default:
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

    }
}
