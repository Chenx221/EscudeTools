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
        public uint face;
        public uint id;
        public uint loc;
        public int order;
        public uint link;
    }
    public class Face
    {
        public List<string> faceOptions = []; //face options str ↓需要解析
    }
    public class TableManagercs
    {
        public static List<int> ParseOptions(LsfData ld, string input)
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
            List<int> tmp = [];
            List<string> tmpS = [];
            for (int i = 0; i < ld.lli.Length; i++)
            {
                if (ld.lli[i].index == 0 && ld.lli[i].state == 0)
                {
                    if (ld.lli[i].index == results[0] && ld.lli[i].state + 1 == results[1])
                    {
                        tmpS.Add(ld.lli[i].nameStr);
                        tmp.Add(i);
                    }

                }
                else if (ld.lli[i].index == results[0] && ld.lli[i].state == results[1])
                {
                    tmpS.Add(ld.lli[i].nameStr);
                    tmp.Add(i);
                }

            }
            if (tmp.Count == 0)
                Console.WriteLine($"[WARN] Found invalid index:state data {results[0]}:{results[1]} in {ld.lsfName}, may be a bug?"); //一般可以忽略这个警告
            return tmp;

        }

        //根据文件名序号重新为组合顺序进行排序，解决某些长发角色合成的问题
        public static List<int> OrderLayer(List<int> layer, List<string> layer_fn)
        {
            List<int> order = [];
            foreach (string item in layer_fn)
            {
                string[] parts = item.Split("_");
                if (parts.Length == 3)
                {
                    if (int.TryParse(parts[2], out int number))
                    {
                        order.Add(number);
                    }
                }
            }
            List<int> sortedTmp = layer.Select((value, index) => new { Value = value, Index = order[index] })
                     .OrderBy(x => x.Index)
                     .Select(x => x.Value)
                     .ToList();
            return sortedTmp;
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
