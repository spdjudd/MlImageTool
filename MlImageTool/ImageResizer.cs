using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace MlImageTool
{
    class ImageResizer
    {
        public static void CropImage(string sourcePath)
        {
            Console.Write("Cropping " + sourcePath);
            var destFolder = Path.GetDirectoryName(sourcePath);
            using (var sourceImage = Image.FromFile(sourcePath))
            {
                if (sourceImage.Width == sourceImage.Height)
                {
                    Console.WriteLine(" - already square");
                    return;
                }
                if (sourceImage.Width > sourceImage.Height)
                {
                    // landscape
                    var crop = CropImage(sourceImage, new Rectangle(0, 0, sourceImage.Height, sourceImage.Height));
                    SaveImage(crop, sourcePath, destFolder, 1);
                    Console.Write(".");
                    crop = CropImage(sourceImage,
                        new Rectangle((sourceImage.Width - sourceImage.Height)/2, 0, sourceImage.Height,
                            sourceImage.Height));
                    SaveImage(crop, sourcePath, destFolder, 2);
                    Console.Write(".");
                    crop = CropImage(sourceImage,
                        new Rectangle(sourceImage.Width - sourceImage.Height, 0, sourceImage.Height, sourceImage.Height));
                    SaveImage(crop, sourcePath, destFolder, 3);
                    Console.Write(".");
                }
                else
                {
                    // portrait
                    var crop = CropImage(sourceImage, new Rectangle(0, 0, sourceImage.Width, sourceImage.Width));
                    SaveImage(crop, sourcePath, destFolder, 1);
                    Console.Write(".");
                    crop = CropImage(sourceImage,
                        new Rectangle(0, (sourceImage.Height - sourceImage.Width)/2, sourceImage.Width,
                            sourceImage.Width));
                    SaveImage(crop, sourcePath, destFolder, 2);
                    Console.Write(".");
                    crop = CropImage(sourceImage,
                        new Rectangle(0, sourceImage.Height - sourceImage.Width, sourceImage.Width, sourceImage.Width));
                    SaveImage(crop, sourcePath, destFolder, 3);
                    Console.Write(".");
                }
            }
            // delete the original only if we cropped it
            File.Delete(sourcePath);
            Console.WriteLine(" - deleted original");
        }

        public static void FlipImage(string sourcePath)
        {
            Console.WriteLine("Flipping " + sourcePath);
            var destFolder = Path.GetDirectoryName(sourcePath);
            using (var sourceImage = Image.FromFile(sourcePath))
            {
                FlipImage(sourceImage);
                SaveImage(sourceImage, sourcePath, destFolder, 1);
            }
        }

        public static void ResizeImage(string sourcePath, int size)
        {
            Console.WriteLine("Resizing " + sourcePath);
            var destFolder = Path.GetDirectoryName(sourcePath);
            using (var sourceImage = Image.FromFile(sourcePath))
            {
                var resized = ResizeImage(sourceImage, size, size);
                SaveImage(resized, sourcePath, destFolder, size);
            }
            File.Delete(sourcePath);
        }

        private static void SaveImage(Image image, string sourcePath, string destFolder, int index = 0)
        {
            // always saves png for now
            var destFileName = string.Format("{0}{1}{2}{3}", 
                Path.GetFileNameWithoutExtension(sourcePath), 
                index > 0 ? ".":"", index > 0 ? index.ToString() : "", 
                ".png");
            var path = Path.Combine(destFolder, destFileName);
            image.Save(path);
        }

        private static void FlipImage(Image sourceImage)
        {
            sourceImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
        }

        private static Image CropImage(Image sourceImage, Rectangle cropRect)
        {
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using(Graphics g = Graphics.FromImage(target))
            {
               g.DrawImage(sourceImage, new Rectangle(0, 0, target.Width, target.Height), 
                                cropRect,                        
                                GraphicsUnit.Pixel);
            }
            return target;
        }

        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }
    }
}
