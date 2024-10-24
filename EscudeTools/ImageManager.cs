using ImageMagick;

namespace EscudeTools
{
    public class ImageManager
    {
        public static bool EvProcess(LsfData ld, int[] n, string target)
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
                baseImage.Composite(overlayImage, offsetX, offsetY, (mode == 3) ? CompositeOperator.Multiply : ((mode == 10) ? CompositeOperator.Plus : CompositeOperator.Over));
            }
            baseImage.Write(target);
            return true;
        }

        public static bool STProcess()
        {
            throw new NotImplementedException();
        }
    }
}
