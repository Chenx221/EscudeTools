using System.Text.RegularExpressions;

namespace EscudeTools
{
    public class EvTable
    {
        public string name;
        public string file;
        public string[] option;
        public uint coverd;
        public uint filter;
        public uint color;
        public uint id;
        public uint loc;
        public int order;
        public uint link;
    }
    public class StTable
    {
        public string name;
        public string file;
        public string[] option;
        public uint coverd;
        public uint filter;
        public uint color;
        public uint id;
        public uint loc;
        public int order;
        public uint link;
    }
    public class TableManagercs
    {
        public static int ParseOptions(LsfData ld, string input)
        {
            // 正则表达式匹配 p*:* 格式
            Regex regex = new(@"p(\d+):(\d+)");
            MatchCollection matches = regex.Matches(input);

            List<int> results = [];

            foreach (Match match in matches)
            {
                if (match.Groups.Count == 3)
                {
                    if (int.TryParse(match.Groups[1].Value, out int firstNumber) &&
                        int.TryParse(match.Groups[2].Value, out int secondNumber))
                    {
                        results.Add(firstNumber);//index 
                        results.Add(secondNumber);//state //可能要+1
                    }
                }
            }
            for (int i = 0; i < ld.lli.Length; i++)
            {
                if (ld.lli[i].index == 0 && ld.lli[i].state == 0)
                {
                    if (ld.lli[i].index == results[0] && ld.lli[i].state + 1 == results[1])
                        return i;
                }
                if (ld.lli[i].index == results[0] && ld.lli[i].state == results[1])
                    return i;
            }
            //一般可以忽略这个警告
            Console.WriteLine($"[WARN] Found invalid index:state data {results[0]}:{results[1]} in {ld.lsfName}, may be a bug?");
            return -1;
        }

        public static int ParseCharactor(string input)
        {
            // 使用正则表达式匹配格式 * _ *
            Regex regex = new(@"^(\d+)_.*$");
            var match = regex.Match(input);

            if (match.Success && match.Groups.Count > 1)
            {
                if (int.TryParse(match.Groups[1].Value, out int result))
                {
                    return result;
                }
            }
            return 0;
        }
    }
}
