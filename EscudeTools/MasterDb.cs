using System.Text;

namespace EscudeTools
{
    public class Sheet
    {
        public string Name { get; set; }
        public uint Cols { get; set; }
        public Column[] Columns { get; set; }

        public class Column
        {
            public ushort Type { get; set; }
            public ushort Size { get; set; }
            public string Name { get; set; }
        }
    }

    public class DatabaseLoader
    {
        public static byte[] LoadFile(string file)
        {
            return File.ReadAllBytes(file);
        }

        public static byte[] DbLoad(string file)
        {
            byte[] db = LoadFile(file);
            int p = 0;

            if (BitConverter.ToUInt32(db, p) != 0x0062646D) // check file magic number == "mdb"
            {
                // Handle error (e.g., log or throw an exception)
                throw new InvalidOperationException($"db_load: {file}");
            }
            p += 4;

            while (BitConverter.ToUInt32(db, p) != 0)
            {
                uint size = BitConverter.ToUInt32(db, p);
                var sheet = new Sheet
                {
                    Columns = new Sheet.Column[1] // Initialize with 1 column (adjust as needed)
                };
                p += 4 + (int)size;

                uint dataSize = BitConverter.ToUInt32(db, p);
                byte[] data = new byte[dataSize];
                Array.Copy(db, p + 4, data, 0, dataSize);
                p += 4 + (int)dataSize;

                uint textSize = BitConverter.ToUInt32(db, p);
                byte[] text = new byte[textSize];
                Array.Copy(db, p + 4, text, 0, textSize);
                p += 4 + (int)textSize;

                sheet.Name = Encoding.UTF8.GetString(text); // Assuming UTF-8 encoding
                uint totalSize = 0;
                for (int i = 0; i < sheet.Cols; i++)
                {
                    sheet.Columns[i].Name = Encoding.UTF8.GetString(text); // Adjust accordingly
                    totalSize += sheet.Columns[i].Size;
                }

                uint index = 0;
                for (int i = 0; i < sheet.Columns.Length; i++)
                {
                    if (sheet.Columns[i].Type == 4) // 假设4表示可变长字符串类型
                    {
                        for (uint j = index; j < dataSize; j += totalSize)
                        {
                            // 假设 data 是 byte[]，我们需要将 byte[] 转换为 uint 数组
                            uint currentValue = BitConverter.ToUInt32(data, (int)j); // 从字节数组中读取当前的 uint 值
                            currentValue += (uint)text; // 加上 text 的值
                            Array.Copy(BitConverter.GetBytes(currentValue), 0, data, (int)j, sizeof(uint)); // 将更新后的值写回 data
                        }
                    }
                    index += sheet.Columns[i].Size; // 更新索引
                }

            }

            return db; // Return the database as a byte array
        }
        public static bool DbSet(byte[] db, string name, uint elemSize, out byte[] data, out int count)
        {
            data = null;
            count = 0;

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid argument: name cannot be null or empty.", nameof(name));
            }

            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            int p = 0;
            if (BitConverter.ToUInt32(db, p) != 0x0062646D)
            {
                throw new InvalidOperationException($"db_set: {name}");
            }
            p += 4;

            while (BitConverter.ToUInt32(db, p) != 0)
            {
                var sheet = new Sheet(); // Create a new instance for each sheet
                                         // Assume sheet.Name is set somewhere during loading...

                if (sheet.Name == name)
                {
                    uint size = 0;
                    for (int i = 0; i < sheet.Cols; i++)
                    {
                        size += sheet.Columns[i].Size;
                    }

                    if (size != elemSize)
                    {
                        throw new InvalidOperationException($"db_set: {name} - Data size mismatch.");
                    }

                    p += 4 + (int)(BitConverter.ToUInt32(db, p));
                    size = BitConverter.ToUInt32(db, p);
                    data = new byte[size];
                    Array.Copy(db, p + 4, data, 0, size);
                    count = (int)(size / elemSize);
                    return true;
                }

                p += 4 + (int)(BitConverter.ToUInt32(db, p)); // Move past the data
                p += 4 + (int)(BitConverter.ToUInt32(db, p)); // Move past text size
            }

            throw new InvalidOperationException($"db_set: {name} not found.");
        }
    }
}
