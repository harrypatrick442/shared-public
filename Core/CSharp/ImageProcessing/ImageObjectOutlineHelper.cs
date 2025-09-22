using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
namespace Snippets.Core.ImageProcessing
{
    public static class ImageObjectOutlineHelper
    {
        public static void SingleOutlineObjectToFile(string plotImageGlowingFilePath, 
            string plotImageWithoutGlowFilePath, RGBABytes color, int width, string outputFilePath, bool fillIn, bool blur)
        {
                using (TempSafeImage tempOutlineImage = SingleOutlineObjects(plotImageGlowingFilePath, 
                    plotImageWithoutGlowFilePath, new Rgba32(color.R, color.G, color.B, color.A), width, fillIn, blur))
                {
                    tempOutlineImage.SaveAs(outputFilePath);
                }
        }
        public static TempSafeImage SingleOutlineObjects(string plotImageGlowingFilePath, 
            string plotImageWithoutGlowFilePath, Rgba32 color, int width, bool fillIn, bool blur)
        {
            using (TempSafeImage tempSafeImageToSubtract = GetSolidErodedImageToSubtract(plotImageWithoutGlowFilePath, width, color, fillIn))
            {
                TempSafeImage finalTempSafeImage = ImageProcessing.SubtractWithAlpha(new SafeImage(plotImageGlowingFilePath), tempSafeImageToSubtract);
                if(blur)
                    finalTempSafeImage.UsingImageSharp((image, save) =>
                    {
                        image.Mutate(context => SixLabors.ImageSharp.Processing.GaussianBlurExtensions.GaussianBlur(context, 2));
                        save();
                    });
                return finalTempSafeImage;
            }
        }
        private static TempSafeImage GetSolidErodedImageToSubtract(string plotImageWithoutGlowFilePath, int width, Rgba32 color, bool fillIn)
        {
            SafeImage safeImagePlotWithoutGlow = new SafeImage(plotImageWithoutGlowFilePath);
            return safeImagePlotWithoutGlow.UsingImageSharp((image, save) =>
            {
                if (fillIn)
                {
                    Func<Rgba32, bool> pixelIsObject = c => c.A > 0;
                    ImageObjectStartEndPixels[] imageObjectStartEndPixelsAlongX = GetImageObjectStartEndPixelsAlongX(image, pixelIsObject);
                    ImageObjectStartEndPixels[] imageObjectStartEndPixelsAlongY = GetImageObjectStartEndPixelsAlongY(image, pixelIsObject);
                    FillInSpacesBetweenObjectsGoingAlongXSoImageIsSolidColor(image, imageObjectStartEndPixelsAlongX, color);
                    FillInSpacesBetweenObjectsGoingAlongYSoImageIsSolidColor(image, imageObjectStartEndPixelsAlongY, color);
                }
                if (width <= 15)
                {
                    return ImageProcessing.DilateAndErodeFilter(image, width, MorphologyType.Erosion);
                }
                else
                {
                    int nPassesForWidth = width / 5;
                    TempSafeImage tempSafeImage = TempSafeImage.New(image);
                    for (int nPass = 0; nPass < nPassesForWidth; nPass++)
                        tempSafeImage.UsingImageSharp((img, ignore) =>
                        {
                            tempSafeImage = ImageProcessing.DilateAndErodeFilter(img, 5, MorphologyType.Erosion);
                        });
                    return tempSafeImage;
                }
            });
        }
        private static void DeductInitialImageFromDilatedImage(Image<Rgba32> initialImage, Image<Rgba32> dilatedImage, Func<Rgba32, bool> pixelIsObject) {
            Rgba32 transparent = new Rgba32(0, 0, 0, 0);
            for (int xIndex = 0; xIndex < initialImage.Width; xIndex++) {
                for (int yIndex = 0; yIndex < initialImage.Height; yIndex++) {
                    Rgba32 colorInitialImage = initialImage.GetPixelRowSpan(yIndex)[xIndex];
                    if (pixelIsObject(colorInitialImage))
                        dilatedImage.GetPixelRowSpan(yIndex)[xIndex]= transparent;
                }
            }
        }
        private static void FillInSpacesBetweenObjectsGoingAlongYSoImageIsSolidColor(Image<Rgba32> image,
            ImageObjectStartEndPixels[] imageObjectStartEndPixelsAlongY, Rgba32 glowColor)
        {
            for (int yIndex = 0; yIndex < imageObjectStartEndPixelsAlongY.Length; yIndex++)
            {
                ImageObjectStartEndPixels imageObjectStartEndPixels = imageObjectStartEndPixelsAlongY[yIndex];
                if (!imageObjectStartEndPixels.Has) continue;
                int toExclusive = (int)imageObjectStartEndPixels.ToExclusive;
                for (int xIndex = (int)imageObjectStartEndPixels.FromInclusive; xIndex < toExclusive; xIndex++)
                {
                    image.GetPixelRowSpan(yIndex)[xIndex] = glowColor;
                }
            }
        }
        private static void FillInSpacesBetweenObjectsGoingAlongXSoImageIsSolidColor(Image<Rgba32> image, 
            ImageObjectStartEndPixels[] imageObjectStartEndPixelsAlongX, Rgba32 glowColor)
        {
            for (int xIndex = 0; xIndex < imageObjectStartEndPixelsAlongX.Length; xIndex++)
            {
                ImageObjectStartEndPixels imageObjectStartEndPixels = imageObjectStartEndPixelsAlongX[xIndex];
                if (!imageObjectStartEndPixels.Has) continue;
                int toExclusive = (int)imageObjectStartEndPixels.ToExclusive;
                for (int yIndex = (int)imageObjectStartEndPixels.FromInclusive; yIndex < toExclusive; yIndex++)
                {
                    image.GetPixelRowSpan(yIndex)[xIndex]=glowColor;
                }
            }
        }
        private static ImageObjectStartEndPixels[] GetImageObjectStartEndPixelsAlongX(Image<Rgba32 >image, Func<Rgba32, bool> pixelIsObject)
        {
            List<ImageObjectStartEndPixels> imageObjectStartEndPixelss = new List<ImageObjectStartEndPixels>();
            for (int x = 0; x < image.Width; x++)
            {
                int? startY = null;
                int? endY = null;
                for (int y = 0; y < image.Height; y++)
                {
                    if (!pixelIsObject(image.GetPixelRowSpan(y)[x]))
                    {
                        if(endY==null)
                        endY = y;
                        continue;
                    }
                    endY = null;
                    if (startY != null) continue;
                    startY = y;
                    continue;
                }
                if (startY != null && endY == null)
                    endY = image.Height;
                imageObjectStartEndPixelss.Add(new ImageObjectStartEndPixels(startY, endY));
            }
            return imageObjectStartEndPixelss.ToArray();
        }
        private static ImageObjectStartEndPixels[] GetImageObjectStartEndPixelsAlongY(Image<Rgba32> image, Func<Rgba32, bool> pixelIsObject)
        {
            List<ImageObjectStartEndPixels> imageObjectStartEndPixelss = new List<ImageObjectStartEndPixels>();

            for (int y = 0; y < image.Height; y++)
            {
                int? startX = null;
                int? endX = null;
                for (int x = 0; x < image.Width; x++)
                {
                    if (!pixelIsObject(image.GetPixelRowSpan(y)[x]))
                    {
                        if (endX == null)
                            endX = x;
                        continue;
                    }
                    endX = null;
                    if (startX != null) continue;
                    startX = x;
                    continue;
                }
                if (startX != null && endX == null)
                    endX = image.Width;
                imageObjectStartEndPixelss.Add(new ImageObjectStartEndPixels(startX, endX));
            }
            return imageObjectStartEndPixelss.ToArray();
        }
    }
}
