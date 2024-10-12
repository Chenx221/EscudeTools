namespace EscudeTools
{
    public class SCRIPTT : Database
    {
        public string name; // 登録名
        public string file; // ファイル名
        public int days; // 経過日数
        public int chart; // チャートID
        public int block; // ブロックID
        public int shape; // ブロック画像
        public string title; // ブロックタイトル
        public int chapter; // チャプターID
        public int boundary; // チャプター境界
        public int diary; // 日記
        public int attrib; // 属性
        public int unlock; // ルート開放条件
    }

    public class CHAPTERT : Database
    {
        public string name; // 登録名
        public string text; // 概要
    }

    public class NAMET : Database
    {
        public string text; // 表示名
        public uint color; // 文字色
        public uint id; // 画像ID
        public uint group; // 音声グループ
        public string face; // 顔画像ファイル名
    }

    public class DIARYT : Database
    {
        public string title; // タイトル
        public string[] text = new string[2]; // 日記本文
    }

    public class VOCT : Database
    {
        public string name; // キャラ識別子
        public string path; // サブフォルダ
        public uint group; // 音声グループ
        public uint sample_voice; // サンプル音声
        public int sample_count; // サンプル音声数
    }

    public class VART : Database
    {
        public string name; // 変数名
        public byte scope; // スコープ
        public uint index; // 変数番号
        public int value; // 初期値
        public bool inherit; // 継承フラグ
        public bool debug; // 表示フラグ
    }

    public class TWEENT : Database
    {
        public string name; // 登録名
        public int time; // 時間
        public int[] param = new int[6]; // パラメータ
        public byte rel_flag; // 相対フラグ
        public byte def_flag; // 定義フラグ
    }

    public class SCENET : Database
    {
        public uint script; // スクリプト番号
        public string title; // シーン名
        public string thumbnail; // サムネイル名
        public int icon; // アイコン表示用
        public int order; // SCENE鑑賞表示順
    }


    internal class DatabaseScripts
    {
    }
}
