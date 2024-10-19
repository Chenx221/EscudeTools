namespace EscudeTools
{
    public class Image
    {
        public byte[] filename = new byte[64]; // Image file name
        public int page; // Image memory
        public int back_page; // Back image memory
        public int width; // Width
        public int height; // Height
        public int depth; // Color depth
        public int id; // ID
        public int reff; // Reference counter
        public bool cache; // Cache flag
        public bool isFile; // Is it an image file
        public uint[] extra = new uint[8]; // Reserved

        public string filenameStr; // 自己加的，用于保存文件名
    }
    public class LsfImage
    {
        public bool cache; // Cache flag
        public Image img; // Layer image
    }
    public class LsfData
    {
        public byte[] path = new byte[64]; // LSF folder
        public LsfFileHeader lfh; // LSF file header
        public LsfLayerInfo lli; // LSF layer information
        public LsfImage layer; // LSF layer image

        public string pathStr;
    }
    public class LsfFileHeader
    {
        public uint signature = 0x46534C; // Header signature (LSF)
        public ushort revision; // Revision number (0x0001)
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
        public string index; // Position
        public string state; // State
        public string mode; // Drawing mode
        public string opacity; // Opacity
        public uint fill; // Fill color
        public uint value; // Generic value

        public string nameStr;  // 自己加的，用于保存文件名
        public string textStr; // 自己加的，用于保存通用名
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
    public class ImageManager
    {
        private static Image image;
        //Todo: Implement ImageManager
        //Lots of methods and properties to implement
    }
}
