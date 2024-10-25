using ImageMagick;
using System.Text;

namespace EscudeTools
{
    public class Image
    {
        //public byte[] file = new byte[64]; // Image file name
        //public int page; // Image memory
        //public int back_page; // Back image memory
        public uint width; // Width
        public uint height; // Height
        public uint depth; // Color depth
        //public int id; // ID
        public int reff; // Reference counter
        //public bool cache; // Cache flag
        public bool isFile; // Is it an image file
        //public uint[] extra = new uint[8]; // Reserved

        public string fileStr; // 自己加的，用于保存文件名
    }
    public class GInfo
    {
        public int width; // Width
        public int height; // Height
        public int depth; // Color depth
        public string pixel; // address of the pixel data
        public uint pitch;
        public string palette;
    }
    public class LsfImage
    {
        //public bool cache; // Cache flag
        public Image img; // Layer image
    }
    public class LsfData
    {
        //public byte[] path = new byte[64]; // LSF folder
        public LsfFileHeader lfh; // LSF file header
        public LsfLayerInfo[] lli; // LSF layer information
        public LsfImage[] layer; // LSF layer image

        public string pathStr;
        public string lsfName;
    }
    public class LsfFileHeader
    {
        //public uint signature; // Header signature (LSF) 0x46534C
        public ushort revision; // Revision number
        public ushort bg; // Background flag
        public ushort id; // ID
        public ushort layer_count; // Number of layers
        public int width; // Width in pixels
        public int height; // Height in pixels
        public int bx; // Base coordinates
        public int by; // Base coordinates
    }
    public class LsfLayerInfo
    {
        public byte[] name = new byte[64]; // File name
        public byte[] text = new byte[64]; // Generic string
        public Rect rect; // Layer position
        public int cx; // Center coordinates
        public int cy; // Center coordinates
        public byte index; // Position
        public byte state; // State
        public byte mode; // Drawing mode
        public byte opacity; // Opacity
        public uint fill; // Fill color
        public uint value; // Generic value

        public string nameStr;  // 自己加的，用于保存文件名
        public string textStr; // 自己加的，用于保存通用名
        public string indexStr; // Position str
        public string stateStr; // State str
        public string modeStr; // Drawing mode str
        public string opacityStr; // Opacity str

        public bool skip = false; // 是否跳过

    }
    public class Rect
    {
        public int left; // Top-left corner X coordinate of the rectangle
        public int top; // Top-left corner Y coordinate of the rectangle
        public int right; // Bottom-right corner X coordinate of the rectangle
        public int bottom; // Bottom-right corner Y coordinate of the rectangle
    }
    public class CgInfo
    {
        public int kind; // Image category
        public int index; // Image index
        public int x; // Coordinates
        public int y; // Coordinates
        public int scale; // Scale factor
        public bool loop; // Loop flag
        public byte[] name = new byte[64]; // Registered name
        public byte[] file = new byte[64]; // File name
        public byte[] option = new byte[128]; // Options
        public uint coverd; // White-out ID
        public uint filter; // Filter
        public uint color; // Color
        public uint id; // Image identification ID
        public uint loc; // Coordinate list
        public uint spot; // Coordinate index
        public int order; // CG viewing display order
        public uint link; // Related CG

        public string nameStr;
        public string fileStr;
        public string optionStr;
    }
    public class LsfManager
    {
        static readonly byte[] lsfFileSignature = [0x4C, 0x53, 0x46, 0x00];
        static readonly byte[] lsfLayerSkipSignature = [0x00, 0x75, 0x6C, 0x00]; //flowchat部分的lsf块
        private static string WorkPath = string.Empty;
        private LsfData lsfData = new();
        private Dictionary<string, LsfData> lsfDataLookup = new();

        private bool preFetchInfo;

        public bool LoadLsf(string path, bool preFI = false)
        {
            if (!File.Exists(path))
                return false;
            preFetchInfo = preFI;
            lsfData.pathStr = Path.GetDirectoryName(path);
            lsfData.lsfName = Path.GetFileNameWithoutExtension(path).ToLower();
            lsfData.lfh = LoadLsfHeader(path);
            lsfData.lli = LoadLsfLayerInfo(path);
            lsfData.layer = new LsfImage[lsfData.lfh.layer_count];
            for (int i = 0; i < lsfData.lfh.layer_count; i++)
            {
                string imgPath = Path.Combine(lsfData.pathStr, lsfData.lli[i].nameStr + ".png");
                LsfImage li = new();
                if (!lsfData.lli[i].skip)
                {
                    li.img = LoadLsfImage(imgPath);
                    lsfData.layer[i] = li;
                }
            }
            lsfDataLookup[lsfData.lsfName] = lsfData;
            lsfData = new();
            return true;
        }

        public LsfData? FindLsfDataByName(string name)
        {
            lsfDataLookup.TryGetValue(name.ToLower(), out var lsfData);


            //c**,为什么会有错字？
            if (name == "08_Syuichi" && lsfData == null)
                lsfDataLookup.TryGetValue("08_syuuichi", out lsfData);


            return lsfData; // 如果未找到，则返回 null
        }

        private LsfFileHeader LoadLsfHeader(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            if (fs.Length < 0x1C)
                throw new Exception("Invalid LSF Header");
            using var br = new BinaryReader(fs);
            byte[] head = br.ReadBytes(4);
            if (!head.SequenceEqual(lsfFileSignature))
                throw new Exception("Invalid LSF file");
            LsfFileHeader lfh = new()
            {
                //lfh.signature = br.ReadUInt32(); //无用
                revision = br.ReadUInt16(),
                bg = br.ReadUInt16(),
                id = br.ReadUInt16(),
                layer_count = br.ReadUInt16(),
                width = br.ReadInt32(),
                height = br.ReadInt32(),
                bx = br.ReadInt32(),
                by = br.ReadInt32()
            };
            return lfh;
        }

        private LsfLayerInfo[] LoadLsfLayerInfo(string path)
        {
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding? shiftJis = provider.GetEncoding("shift-jis");
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);
            br.ReadBytes(0x1C); // Skip the header
            long remainingBytes = br.BaseStream.Length - br.BaseStream.Position;
            if (remainingBytes != lsfData.lfh.layer_count * 0xA4)
                throw new Exception("Invalid LSF Layer Info");
            LsfLayerInfo[] llis = new LsfLayerInfo[lsfData.lfh.layer_count];
            for (int i = 0; i < lsfData.lfh.layer_count; i++)
            {
                LsfLayerInfo l = new()
                {
                    name = br.ReadBytes(64),
                    text = br.ReadBytes(64),
                    rect = new Rect
                    {
                        left = br.ReadInt32(),
                        top = br.ReadInt32(),
                        right = br.ReadInt32(),
                        bottom = br.ReadInt32()
                    },
                    cx = br.ReadInt32(),
                    cy = br.ReadInt32(),
                    index = br.ReadByte(),
                    state = br.ReadByte(),
                    mode = br.ReadByte(),
                    opacity = br.ReadByte(),
                    fill = br.ReadUInt32(),
                    value = br.ReadUInt32()
                };
                if (l.name.Take(4).SequenceEqual(lsfLayerSkipSignature))//临时处理
                    l.skip = true;
                l.nameStr = shiftJis.GetString(l.name).TrimEnd('\0');
                l.textStr = shiftJis.GetString(l.text).TrimEnd('\0');
                l.indexStr = l.index.ToString().TrimEnd('\0');
                l.stateStr = l.state.ToString().TrimEnd('\0');
                l.modeStr = l.mode.ToString().TrimEnd('\0');
                l.opacityStr = l.opacity.ToString().TrimEnd('\0');
                llis[i] = l;
            }
            return llis;
        }

        private Image LoadLsfImage(string imgPath)
        {
            if (!File.Exists(imgPath))
                throw new Exception("Image file not found");//一般文件都是存在的，不存在是因为这是特殊lsf
            Image i = new()
            {
                fileStr = imgPath,
                isFile = true,
                reff = 1
            };
            if (preFetchInfo)
            {
                using var image = new MagickImage(imgPath);
                i.width = image.Width;
                i.height = image.Height;
                i.depth = image.Depth;
            }
            return i;
        }
    }
}
