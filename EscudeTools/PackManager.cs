//这里的提取代码参考(Ctrl+C, Ctrl+V)了Garbro中关于ESCUDE BIN封包的实现
using EscudeTools.Garbro;
using System.Text;
using System.Text.Json;

namespace EscudeTools
{
    public class Entry
    {
        public string Name { get; set; }
        public long Offset { get; set; }
        public uint Size { get; set; }
    }
    public class LzwEntry
    {
        public string Name { get; set; }
        public int UnpackSize { get; set; }
    }
    public class PackManager
    {
        static readonly byte[] fileSignature = [0x45, 0x53, 0x43, 0x2D, 0x41, 0x52, 0x43]; //"ESC-ARC"
        static readonly byte[] LzwSignature = [0x61, 0x63, 0x70, 0x00]; //"acp\0"
        static readonly byte[] supportPackVersion = [0x31, 0x32]; //1, 2
        private bool isLoaded = false;
        private uint LoadedKey;
        private string pFile = "";
        private uint m_seed;
        private uint m_count;
        List<Entry> pItem = [];

        public bool Load(string path)
        {
            if (!File.Exists(path))
                return false;
            using (FileStream fs = new(path, FileMode.Open))
            using (BinaryReader br = new(fs))
            {
                byte[] head = br.ReadBytes(fileSignature.Length);
                if (!head.SequenceEqual(fileSignature))
                    return false;
                byte ver = br.ReadByte();
                List<Entry>? item = [];
                switch (Array.IndexOf(supportPackVersion, ver))
                {
                    case 0:
                        item = ProcessV1(br);
                        break;
                    case 1:
                        item = ProcessV2(br);
                        break;
                    default:
                        return false;
                }
                if (item == null)
                    return false;
                pItem = item;
            }
            isLoaded = true;
            pFile = path;
            return true;
        }

        //测试用
        private bool LoadKey(string path)
        {
            if (!File.Exists(path))
                return false;
            using (FileStream fs = new(path, FileMode.Open))
            using (BinaryReader br = new(fs))
            {
                byte[] head = br.ReadBytes(fileSignature.Length);
                if (!head.SequenceEqual(fileSignature))
                    return false;
                br.ReadByte();
                m_seed = br.ReadUInt32();
            }
            LoadedKey = m_seed;
            isLoaded = true;
            return true;
        }

        private List<Entry>? ProcessV1(BinaryReader br)
        {
            m_seed = br.ReadUInt32();
            LoadedKey = m_seed;
            m_count = br.ReadUInt32() ^ NextKey();
            uint index_size = m_count * 0x88;
            byte[] index = Utils.ReadBytes(br, index_size);
            if (index.Length != index_size)
                return null;
            Decrypt(ref index);
            int index_offset = 0;
            List<Entry> dir = new((int)m_count);
            for (uint i = 0; i < m_count; ++i)
            {
                string name = Utils.ReadStringFromTextData(index, index_offset, 0x80);
                Entry entry = new()
                {
                    Name = name,
                    Offset = Utils.ToUInt32(index, index_offset + 0x80),
                    Size = Utils.ToUInt32(index, index_offset + 0x84)
                };
                index_offset += 0x88;
                dir.Add(entry);
            }
            return dir;
        }

        private List<Entry>? ProcessV2(BinaryReader br)
        {
            m_seed = br.ReadUInt32();
            LoadedKey = m_seed;
            m_count = br.ReadUInt32() ^ NextKey();
            uint names_size = br.ReadUInt32() ^ NextKey();
            uint index_size = m_count * 12;
            byte[] index = Utils.ReadBytes(br, index_size);
            if (index.Length != index_size)
                return null;
            var names = Utils.ReadBytes(br, names_size);
            if (names.Length != names_size)
                return null;
            Decrypt(ref index);
            int index_offset = 0;
            var dir = new List<Entry>((int)m_count);
            for (uint i = 0; i < m_count; ++i)
            {
                int filename_offset = (int)Utils.ToUInt32(index, index_offset);
                if (filename_offset < 0 || filename_offset >= names.Length)
                    return null;
                var name = Utils.ReadStringFromTextData(names, filename_offset, names.Length - filename_offset);
                if (0 == name.Length)
                    return null;
                Entry entry = new()
                {
                    Name = name,
                    Offset = Utils.ToUInt32(index, index_offset + 4),
                    Size = Utils.ToUInt32(index, index_offset + 8)
                };
                index_offset += 12;
                dir.Add(entry);
            }
            return dir;
        }

        private void Decrypt(ref byte[] data)
        {
            int length = data.Length / 4;
            uint[] buffer = new uint[length];
            Buffer.BlockCopy(data, 0, buffer, 0, data.Length);
            for (int i = 0; i < length; i++)
            {
                buffer[i] ^= NextKey();
            }
            Buffer.BlockCopy(buffer, 0, data, 0, data.Length);
        }

        private uint NextKey()
        {
            m_seed ^= 0x65AC9365;
            m_seed ^= (((m_seed >> 1) ^ m_seed) >> 3) ^ (((m_seed << 1) ^ m_seed) << 3);
            return m_seed;
        }

        public bool Extract()
        {
            if (!isLoaded)
                throw new InvalidOperationException("Pack not loaded");

            string output = Path.Combine(Path.GetDirectoryName(pFile), "output", Path.GetFileNameWithoutExtension(pFile));
            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);
            var lzwManifest = new List<LzwEntry>();
            //string jsonPath = Path.Combine(output, "lzwManifest.json");
            using FileStream inputStream = new(pFile, FileMode.Open, FileAccess.Read);
            using BinaryReader br = new(inputStream);
            foreach (Entry entry in pItem)
            {
                string entryPath = Path.Combine(output, entry.Name);
                string entryDirectory = Path.GetDirectoryName(entryPath);

                if (!Directory.Exists(entryDirectory))
                    Directory.CreateDirectory(entryDirectory);
                byte[] test;
                br.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                test = br.ReadBytes(LzwSignature.Length);

                if (entry.Size > 8 && test.SequenceEqual(LzwSignature))
                {
                    entryPath += ".lzw";
                    int upackSize = Utils.ToBigEndian(br.ReadInt32());
                    lzwManifest.Add(new LzwEntry { Name = entry.Name, UnpackSize = upackSize });
                    using FileStream outputStream = new(entryPath, FileMode.Create);
                    inputStream.Seek(entry.Offset + 8, SeekOrigin.Begin);
                    byte[] buffer = new byte[entry.Size - 8];
                    inputStream.Read(buffer, 0, buffer.Length);
                    outputStream.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    using FileStream outputStream = new(entryPath, FileMode.Create);
                    inputStream.Seek(entry.Offset, SeekOrigin.Begin);
                    byte[] buffer = new byte[entry.Size];
                    inputStream.Read(buffer, 0, buffer.Length);
                    outputStream.Write(buffer, 0, buffer.Length);
                }
            }

            if (lzwManifest.Count > 0)
            {
                //using (FileStream fs = File.Create(jsonPath))
                //{
                //    byte[] jsonBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(lzwManifest));
                //    fs.Write(jsonBytes, 0, jsonBytes.Length);
                //}
                LzwDecode(lzwManifest, output);
            };

            return true;
        }

        private void LzwDecode(List<LzwEntry> lzwManifest, string output)
        {
            foreach (var i in lzwManifest)
            {
                string targetPath = Path.Combine(output, i.Name);
                int unpacked_size = i.UnpackSize;
                using FileStream inputStream = new(targetPath + ".lzw", FileMode.Open, FileAccess.Read);
                using var decoder = new LzwDecoder(inputStream, unpacked_size);
                decoder.Unpack();
                using FileStream outputStream = new(targetPath, FileMode.Create);
                outputStream.Write(decoder.Output, 0, decoder.Output.Length);
                outputStream.Flush();
                outputStream.Close();
                inputStream.Close();
                File.Delete(targetPath + ".lzw");
            }
        }

        internal sealed class LzwDecoder : IDisposable
        {
            private MsbBitStream m_input;
            private byte[] m_output;

            public byte[] Output { get { return m_output; } }

            public LzwDecoder(Stream input, int unpacked_size)
            {
                m_input = new MsbBitStream(input, true);
                m_output = new byte[unpacked_size];
            }

            public void Unpack()
            {
                int dst = 0;
                var lzw_dict = new int[0x8900];
                int token_width = 9;
                int dict_pos = 0;
                while (dst < m_output.Length)
                {
                    int token = m_input.GetBits(token_width);
                    if (-1 == token)
                        throw new EndOfStreamException("Invalid compressed stream");
                    else if (0x100 == token) // end of input
                        break;
                    else if (0x101 == token) // increase token width
                    {
                        ++token_width;
                        if (token_width > 24)
                            throw new Exception("Invalid comressed stream");
                    }
                    else if (0x102 == token) // reset dictionary
                    {
                        token_width = 9;
                        dict_pos = 0;
                    }
                    else
                    {
                        if (dict_pos >= lzw_dict.Length)
                            throw new Exception("Invalid comressed stream");
                        lzw_dict[dict_pos++] = dst;
                        if (token < 0x100)
                        {
                            m_output[dst++] = (byte)token;
                        }
                        else
                        {
                            token -= 0x103;
                            if (token >= dict_pos)
                                throw new Exception("Invalid comressed stream");
                            int src = lzw_dict[token];
                            int count = Math.Min(m_output.Length - dst, lzw_dict[token + 1] - src + 1);
                            if (count < 0)
                                throw new Exception("Invalid comressed stream");
                            Utils.CopyOverlapped(m_output, src, dst, count);
                            dst += count;
                        }
                    }
                }
            }

            #region IDisposable Members
            bool _disposed = false;
            public void Dispose()
            {
                if (!_disposed)
                {
                    m_input.Dispose();
                    _disposed = true;
                }
                GC.SuppressFinalize(this);
            }
            #endregion
        }

        public bool Repack(string path, int version, bool useCustomKey = false, string customKeyProviderPath = "") //目前支持v2v1
        {
            if (useCustomKey)
                LoadKey(customKeyProviderPath);
            GeneratePItem(path);
            //if(File.Exists(Path.Combine(path, "lzwManifest.json")))
            //    return false; 
            //Q:为什么不支持LZW打包 //A:因为我实在不想研究lzw算法，欢迎PR
            m_seed = isLoaded ? LoadedKey : 2210579460;
            string outputPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path) + ".bin");
            using (FileStream fs = new(outputPath, FileMode.Create))
            using (BinaryWriter bw = new(fs))
            {
                bw.Write(fileSignature);
                bw.Write(supportPackVersion[version - 1]);
                bw.Write(m_seed);
                m_count = (uint)pItem.Count;
                bw.Write(m_count ^ NextKey());
                EncodingProvider provider = CodePagesEncodingProvider.Instance;
                Encoding? shiftJis = provider.GetEncoding("shift-jis");
                if (version == 1) //未经测试
                {
                    long storeOffset = 0x10 + m_count * 0x88;
                    for (int i = 0; i < m_count; i++)
                    {
                        byte[] strbytes = shiftJis.GetBytes(pItem[i].Name);
                        byte[] result = new byte[80];
                        int lengthToCopy = Math.Min(strbytes.Length, 78);
                        Array.Copy(strbytes, result, lengthToCopy);
                        bw.Write(result);
                        bw.Write(storeOffset);
                        bw.Write(pItem[i].Size);
                        storeOffset += pItem[i].Size;
                    }
                }
                else
                {
                    uint namesSize = (uint)pItem.Sum(e => e.Name.Length + 1);
                    bw.Write(namesSize ^ NextKey());
                    uint filenameOffset = 0;
                    long storeOffset = 0x14 + m_count * 12 + namesSize;
                    byte[] index = new byte[m_count * 12];
                    int indexOffset = 0;
                    for (int i = 0; i < m_count; i++)
                    {
                        BitConverter.GetBytes(filenameOffset).CopyTo(index, indexOffset);
                        indexOffset += 4;
                        BitConverter.GetBytes(storeOffset).CopyTo(index, indexOffset);
                        indexOffset += 4;
                        BitConverter.GetBytes(pItem[i].Size).CopyTo(index, indexOffset);
                        indexOffset += 4;
                        filenameOffset += (uint)pItem[i].Name.Length + 1;
                        storeOffset += pItem[i].Size;
                    }
                    Decrypt(ref index);
                    bw.Write(index);

                    foreach (Entry entry in pItem)
                    {
                        byte[] nameBytes = shiftJis.GetBytes(entry.Name);
                        bw.Write(nameBytes);
                        bw.Write((byte)0);
                    }
                }
                foreach (Entry entry in pItem)
                {
                            byte[] data = File.ReadAllBytes(Path.Combine(path, entry.Name));
                            bw.Write(data);
                        }
                    }
            return true;
        }

        private void GeneratePItem(string path)
        {
            pItem.Clear();
            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                             .Where(file => !file.EndsWith("lzwManifest.json", StringComparison.OrdinalIgnoreCase))
                             .ToArray();
            foreach (var file in files)
                {
                    var relativePath = Path.GetRelativePath(path, file);
                    var fileInfo = new FileInfo(file);
                    pItem.Add(new Entry
                    {
                        Name = relativePath,
                        Size = (uint)fileInfo.Length
                    });
                }
            m_count = (uint)pItem.Count;
        }
    }
}
