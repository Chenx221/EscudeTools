using ImageMagick;

namespace EscudeTools
{
    public class ImageManager
    {
        public static void ImgPreProcessNew(List<StTable> stts, Face[] faces, LsfManager lm, string outputDir)
        {
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 6 // 设置最大并行线程数
            };

            Parallel.ForEach(stts, parallelOptions, stt =>
            //foreach (StTable stt in stts)
            {
                if (stt.order == 0) //仅提取鉴赏中有的ST
                    return;
                //continue;
                string targetFilename = Path.Combine(outputDir, stt.name); //最后保存可用的文件名
                LsfData? lsfData = lm.FindLsfDataByName(stt.file) ?? throw new Exception($"错误，未找到与{stt.file}对应的lsf数据");
                List<int> pendingList = [];
                List<string> pendingListFn = [];
                foreach (string o in stt.option)
                {
                    List<int> t = TableManagercs.ParseOptions(lsfData, o);
                    if (t.Count == 0)
                        continue;
                    pendingList.AddRange(t);
                    foreach (int i in t)
                    {
                        pendingListFn.Add(lsfData.lli[i].nameStr);
                    }
                }
                //pendingList = TableManagercs.OrderLayer(pendingList, pendingListFn);
                int n = 0;
                foreach (string o in faces[(int)stt.face].faceOptions)
                {
                    List<int> pendingListCopy = new(pendingList);
                    List<string> pendingListFnCopy = new(pendingListFn);
                    List<int> t = TableManagercs.ParseOptions(lsfData, o);
                    if (t.Count == 0)
                        continue;
                    foreach (int i in t)
                    {
                        pendingListFnCopy.Add(lsfData.lli[i].nameStr);
                    }
                    pendingListCopy.AddRange(t);
                    pendingListCopy = TableManagercs.OrderLayer(pendingListCopy, pendingListFnCopy);
                    if (!ImageManager.Process(lsfData, [.. pendingListCopy], targetFilename + $"_{n}.png"))
                        throw new Exception("Process Fail");
                    else
                        Console.WriteLine($"Export {stt.name}_{n} Success");
                    n++;
                }
            });
        }

        public static void ImgPreProcessOld(List<StTable> stts, LsfManager lm, string outputDir)
        {
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 6 // 设置最大并行线程数
            };

            Parallel.ForEach(stts, parallelOptions, stt =>
            //foreach (var stt in stts)
            {
                if (stt.order == 0) //仅提取鉴赏中有的ST
                    return;
                string targetFilename = Path.Combine(outputDir, stt.name);
                LsfData? lsfData = lm.FindLsfDataByName(stt.file) ?? throw new Exception($"错误，未找到与{stt.file}对应的lsf数据");
                List<int> faceAvailList = [];
                List<string> faceAvailNameList = [];
                for (int i = 0; i < lsfData.lli.Length; i++)
                {
                    if (lsfData.lli[i].index == 1)
                    {
                        faceAvailList.Add(i);
                        faceAvailNameList.Add(lsfData.lli[i].nameStr);
                    }

                }
                List<int> pendingList = [];
                List<string> pendingListFn = [];
                foreach (string o in stt.option)
                {
                    List<int> t = TableManagercs.ParseOptions(lsfData, o);
                    if (t.Count == 0)
                        continue;
                    pendingList.AddRange(t);
                    foreach (int i in t)
                    {
                        pendingListFn.Add(lsfData.lli[i].nameStr);
                    }
                }
                //pendingList = TableManagercs.OrderLayer(pendingList, pendingListFn);
                int n = 0;
                for (int k = 0; k < faceAvailList.Count; k++)
                {
                    List<int> que = new(pendingList);
                    List<string> queStr = new(pendingListFn);
                    que.Add(faceAvailList[k]);
                    queStr.Add(faceAvailNameList[k]);
                    que = TableManagercs.OrderLayer(que, queStr);
                    if (!ImageManager.Process(lsfData, [.. que], targetFilename + $"_{n}.png"))
                        throw new Exception("Process Fail");
                    else
                        Console.WriteLine($"Export {stt.name}_{n} Success");
                    n++;
                }
            });
        }

        public static bool Process(LsfData ld, int[] n, string target)
        {
            int height = ld.lfh.height, width = ld.lfh.width;
            using var baseImage = new MagickImage(MagickColors.Transparent, (uint)width, (uint)height);
            for (int i = 0; i < n.Length; i++)
            {
                string imgPath = ld.layer[n[i]].img.fileStr;
                using var overlayImage = new MagickImage(imgPath);
                int offsetX = ld.lli[n[i]].rect.left;
                int offsetY = ld.lli[n[i]].rect.top;
                int mode = ld.lli[n[i]].mode;
                if (mode == 3)
                {
                    overlayImage.Composite(baseImage, -1 * offsetX, -1 * offsetY, CompositeOperator.DstIn);
                    baseImage.Composite(overlayImage, offsetX, offsetY, CompositeOperator.Multiply);//应该能解决透明度问题了
                }
                else if (mode == 10) //目前还没遇到过?
                {
                    baseImage.Composite(overlayImage, offsetX, offsetY, CompositeOperator.Plus);
                }
                else
                {
                    baseImage.Composite(overlayImage, offsetX, offsetY, CompositeOperator.Over);
                }
            }
            baseImage.Write(target);
            return true;
        }


    }
}
