namespace EscudeTools
{
    public class ScriptMessage
    {
        public byte[] Data { get; set; }        // MESS領域 (消息区域)
        public uint Size { get; set; }          // MESS領域サイズ (消息区域大小)
        public uint[] Offset { get; set; }      // MESSオフセット (消息偏移)
        public uint Count { get; set; }         // MESS数 (消息数量)
    }

    public class ScriptFile
    {
        public byte[] Code { get; set; }        // CODE領域 (代码区域)
        public uint CodeSize { get; set; }      // CODE領域サイズ (代码区域大小)

        public byte[] Text { get; set; }        // TEXT領域 (文本区域)
        public uint TextSize { get; set; }      // TEXT領域サイズ (文本区域大小)
        public uint[] TextOffset { get; set; }  // TEXTオフセット (文本偏移)
        public uint TextCount { get; set; }     // TEXT数 (文本数量)

        public uint MessCount { get; set; }     // MESS数 (消息数量)
    }

    public class ScriptManager
    {
        static readonly byte[] MessHeader = [0x40, 0x6D, 0x65, 0x73, 0x73, 0x3A, 0x5F, 0x5F]; //@mess:__
        static readonly byte[] FileHeader = [0x40, 0x63, 0x6F, 0x64, 0x65, 0x3A, 0x5F, 0x5F]; //@code:__

        public bool LoadScriptFile(string path)
        {
            throw new NotImplementedException();
        }

        public bool LoadScriptMess(string path)
        {
            throw new NotImplementedException();
        }
    }
}
