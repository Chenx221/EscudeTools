namespace EscudeTools
{
    public class LOCTEXTT : Database
    {
        public string key; // 登録名
        public string text; // テキスト
    }

    public class LOCFILET : Database
    {
        public string key; // 登録名
        public string path; // ファイルパス
    }

    public class LOCNUMT : Database
    {
        public string key; // 登録名
        public int[] nums = new int[8]; // 数値リスト
    }
    internal class DatabaseLocalize
    {
    }
}
