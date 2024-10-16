﻿//这里的代码参考(Ctrl+C, Ctrl+V)了Garbro中关于ESCUDE BIN封包的实现
using System.Text;

namespace EscudeTools
{
    public class Entry
    {
        public string Name { get; set; }
        public long Offset { get; set; }
        public uint Size { get; set; }
    }
    public class PackManager
    {
        static readonly byte[] fileSignature = [0x45, 0x53, 0x43, 0x2D, 0x41, 0x52, 0x43]; //"ESC-ARC"
        static readonly byte[] supportPackVersion = [0x31, 0x32]; //1, 2
        private bool isLoaded = false;
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

        private List<Entry>? ProcessV1(BinaryReader br)
        {
            m_seed = br.ReadUInt32();
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

            using FileStream inputStream = new(pFile, FileMode.Open);
            foreach (Entry entry in pItem)
            {
                string entryPath = Path.Combine(output, entry.Name);
                string entryDirectory = Path.GetDirectoryName(entryPath);

                if (!Directory.Exists(entryDirectory))
                    Directory.CreateDirectory(entryDirectory);

                using FileStream outputStream = new(entryPath, FileMode.Create);
                inputStream.Seek(entry.Offset, SeekOrigin.Begin);
                byte[] buffer = new byte[entry.Size];
                inputStream.Read(buffer, 0, buffer.Length);
                outputStream.Write(buffer, 0, buffer.Length);
            }

            return true;
        }

        public bool Repack(string path)
        {
            throw new NotSupportedException("Repack not supported");
        }
    }
}