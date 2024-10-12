namespace EscudeTools
{
    public class BGMT : Database
    {
        public string name; // 登録名
        public string file; // ファイル名
        public byte volume; // 再生ボリューム
        public byte loop; // 途中ループ
        public string title; // 曲名
        public int order; // 曲順
    }

    public class AMBT : Database
    {
        public string name; // 登録名
        public string file; // ファイル名
        public byte volume; // 再生ボリューム
    }

    public class BGVT : Database
    {
        public string name; // 登録名
        public string file; // ファイル名
        public byte volume; // 再生ボリューム
    }

    public class SET : Database
    {
        public string name; // 登録名
        public string file; // ファイル名
        public byte volume; // 再生ボリューム
        public byte type; // Ｈ効果音フラグ
        public byte sample; // サンプル音声フラグ
    }

    public class SFXT : Database
    {
        public string name; // 登録名
        public string file; // ファイル名
    }


    internal class DatabaseSounds
    {
    }
}
