using System.Text;

namespace EscudeTools
{
    public class Utils
    {
        public static string ReadStringFromTextData(byte[] sheet_text, int offset)
        {
            return ReadStringFromTextData(sheet_text, offset, -1);
        }

        public static string ReadStringFromTextData(byte[] sheet_text, int offset, int length_limit)
        {
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding? shiftJis = provider.GetEncoding("shift-jis") ?? throw new InvalidOperationException("Shift-JIS encoding not supported.");
            return ReadStringFromTextData(sheet_text, offset, length_limit, shiftJis);
        }

        public static string ReadStringFromTextData(byte[] sheet_text, int offset, int length_limit, Encoding enc)
        {
            List<byte> stringBytes = [];
            int end = (length_limit != -1) ? Math.Min(offset + length_limit, sheet_text.Length) : sheet_text.Length;
            for (int i = offset; i < end && sheet_text[i] != 0x00; i++)
            {
                stringBytes.Add(sheet_text[i]);
            }
            return enc.GetString(stringBytes.ToArray());
        }

        public static byte[] ReadBytes(BinaryReader reader, ulong length)
        {
            const int bufferSize = 8192;
            byte[] data = new byte[length];
            ulong bytesRead = 0;
            while (bytesRead < length)
            {
                int toRead = (int)Math.Min(bufferSize, length - bytesRead);
                int read = reader.Read(data, (int)bytesRead, toRead);
                if (read == 0)
                    break;
                bytesRead += (ulong)read;
            }
            return data;
        }

        public static uint ToUInt32<TArray>(TArray value, int index) where TArray : IList<byte>
        {
            return (uint)(value[index] | value[index + 1] << 8 | value[index + 2] << 16 | value[index + 3] << 24);
        }
    }
}
