using ImageMagick;

namespace EscudeTools
{
    public class ImageManager
    {
        public static bool Process(LsfData ld, int[] n, string target)
        {
            //get base size
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
                    baseImage.Composite(overlayImage, offsetX, offsetY, CompositeOperator.Multiply);//原先就一条这个，发现处理透明时会有问题
                }
                else if (mode == 10)
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

        //上面足够给Ev、St用了
        //public static bool StProcess()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
