namespace EscudeTools
{
    public class BGT : Database
    {
        public string name; // 登録名
        public string file; // ファイル名
        public string option; // オプション
        public uint coverd; // 白消しID
        public uint filter; // フィルター
        public uint color; // 色
        public uint id; // 画像識別ID
        public uint loc; // 関連座標
        public int order; // CG表示順
        public uint link; // 関連CG
    }

    public class EVT : Database
    {
        public string name; // 登録名
        public string file; // ファイル名
        public string option; // オプション
        public uint coverd; // 白消しID
        public uint filter; // フィルター
        public uint color; // 色
        public uint id; // 画像識別ID
        public uint loc; // 関連座標
        public int order; // CG表示順
        public uint link; // 関連CG
    }

    public class STT : Database
    {
        public string name; // 登録名
        public string file; // ファイル名
        public string option; // オプション
        public uint coverd; // 白消しID
        public uint filter; // 背景フィルタ
        public uint face; // 表情
        public uint id; // 画像識別ID
        public uint loc; // 関連座標
        public int order; // CG表示順
        public uint link; // 関連CG
    }

    public class FACET : Database
    {
        public string name; // 登録名
        public string option; // オプション
        public byte[] exists = new byte[32]; // 
    }

    public class EFXT : Database
    {
        public string name; // 登録名
        public string file; // ファイル名
        public int spot; // 座標インデックス
        public int dx, dy; // 相対座標
        public int scale; // 倍率
        public bool loop; // ループフラグ
    }

    public class PT
    {
        public short x;
        public short y;
    }
    public class LOCT : Database
    {
        public PT[] pt = new PT[8]; // 座標
    }


    internal class DatabaseGraphics
    {
    }
}
