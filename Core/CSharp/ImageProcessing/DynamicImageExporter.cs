
using SnippetsCore.ImageProcessing;
using System;
using System.Collections.Generic;
using SnippetsCore.Geometry;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SnippetsCore.Json;

namespace CloudRenderFileNameConverter
{
    public static class DynamicImageExporter
    {
        private static readonly IJsonParser _JsonParser = new NativeJsonParser();
        public static DynamicImage ExportUsingCache(DynamicImageExportProfile dynamicImageExportProfile, 
            string sourceImageAbsolutePath, string rootDirectoryPath, string relativeDirectoryPath, string fileNamePrefix,
            CachedFilesDirectory cachedFilesDirectory) {
            string cachedDirectoryPath = cachedFilesDirectory.GetIfCachedDirectoryIsValid(identifier: sourceImageAbsolutePath, DateTime.MinValue);
            if (cachedDirectoryPath != null)
            {
                string dynamicImageAbsolutePath = Path.Combine(cachedDirectoryPath, "dynamicImage.json");
                DynamicImage dynamicImageFromCache = _JsonParser.Deserialize<DynamicImage>(File.ReadAllText(dynamicImageAbsolutePath));
                return dynamicImageFromCache.CopyToDirectory(rootDirectoryPath);
            }
            DynamicImage dynamicImage = DynamicImageExporter.Export(dynamicImageExportProfile, sourceImageAbsolutePath,
                rootDirectoryPath,
                relativeDirectoryPath, fileNamePrefix);
            cachedDirectoryPath = cachedFilesDirectory.CreateDirectory(sourceImageAbsolutePath);
            DynamicImage dynamicImageCached = dynamicImage.CopyToDirectory(cachedDirectoryPath);
            File.WriteAllText(Path.Combine(cachedDirectoryPath, "dynamicImage.json"), _JsonParser.Serialize(dynamicImageCached));
            return dynamicImage;
        }
        public static DynamicImage Export(DynamicImageExportProfile dynamicImageExportProfile, string sourceImageAbsolutePath, string rootDirectoryPath, string relativeDirectoryPath, string fileNamePrefix)
        {
            Action<Vector2<int>> addSeenSize;
            Func<Vector2<int>, bool> seenSize = Get_SeenSizeAndAddSeenSizeLexicalClosures(out addSeenSize);
            Func<Vector2<int>, string> getFileRelativePathForSize = Get_GetRelativeFilePathForSize(relativeDirectoryPath, fileNamePrefix, Path.GetExtension(sourceImageAbsolutePath));
            List<DynamicImageEntry> dynamicImageEntries = new List<DynamicImageEntry>();
            SafeImage safeImage = new SafeImage(sourceImageAbsolutePath);
            return safeImage.UsingImageSharp((image, save) =>
            {
                Vector2<int> imgeSize = new Vector2<int>(image.Width, image.Height);
                foreach (DynamicImageExportProfileSize dynamicImageExportProfileSize in dynamicImageExportProfile.DesiredSizes)
                {
                    Vector2<int> desiredSize = ValidateAndParseSize(dynamicImageExportProfile, dynamicImageExportProfileSize, imgeSize);
                    if (desiredSize == null || seenSize(desiredSize)) continue;
                    dynamicImageEntries.Add(ExportSize(desiredSize, dynamicImageExportProfile, image, rootDirectoryPath, getFileRelativePathForSize(desiredSize)));
                }
                if (dynamicImageExportProfile.IncludeOriginalSize)
                {
                    if (!seenSize(imgeSize))
                    {
                        dynamicImageEntries.Add(ExportSize(imgeSize, dynamicImageExportProfile, image, rootDirectoryPath, getFileRelativePathForSize(imgeSize)));
                    }
                }

                return new DynamicImage(dynamicImageEntries.ToArray(), rootDirectoryPath);
            });
        }
        private static DynamicImageEntry ExportSize(Vector2<int> size, DynamicImageExportProfile dynamicImageExportProfile, Image<Rgba32> image, string rootDirectoryPath, string fileRelativePathToSaveTo)
        {
                bool isOriginal = image.Width == size.X && image.Height == size.Y;
                string absolutePathToSaveTo = Path.Combine(rootDirectoryPath, fileRelativePathToSaveTo);
                Directory.CreateDirectory(Path.GetDirectoryName(absolutePathToSaveTo));
                if (isOriginal)
                {
                    Console.WriteLine(absolutePathToSaveTo);
                    image.Save(absolutePathToSaveTo);
                }
                else
                {
                    using (TempSafeImage resizedImage = ImageProcessing.ResizeImage(image, size.X, size.Y))
                    {
                        Console.WriteLine(absolutePathToSaveTo);
                        resizedImage.SaveAs(absolutePathToSaveTo);
                    }
                }
                return new DynamicImageEntry(fileRelativePathToSaveTo, size.X, size.Y);
        }
        private static Vector2<int> ValidateAndParseSize(DynamicImageExportProfile dynamicImageExportProfile, DynamicImageExportProfileSize dynamicImageExportProfileSize, Vector2<int> imageSize){
            Vector2<int> size = ParseSize(dynamicImageExportProfileSize, imageSize);
            return ValidateSizeAllowed(imageSize, size, dynamicImageExportProfile, dynamicImageExportProfileSize);
        }
        private static Vector2<int> ParseSize(DynamicImageExportProfileSize dynamicImageExportProfileSize, Vector2<int> imageSize) {
            bool hasWidth = dynamicImageExportProfileSize.Width != null;
            bool hasHeight = dynamicImageExportProfileSize.Height != null;
            Func<float> getAspectRatio = () => { return (float)imageSize.X / (float)imageSize.Y; };
            if (!hasWidth)
            {
                if (!hasHeight)
                {
                    throw new ArgumentException($"The specified {nameof(DynamicImageExportProfileSize)} {dynamicImageExportProfileSize} had neither a width or height");
                }
                int height = (int)dynamicImageExportProfileSize.Height;
                return new Vector2<int>((int)Math.Round(height * getAspectRatio()), height);
                
            }
            int width = (int)dynamicImageExportProfileSize.Width;
            if (!hasHeight)
            {
                return new Vector2<int>(width, (int)Math.Round(width / getAspectRatio()));
            }
            {
                int height = (int)dynamicImageExportProfileSize.Height;
                if (width == imageSize.X && height == imageSize.Y) 
                    return imageSize;
                int calculatedHeightFromWidth = (int)Math.Round(width / getAspectRatio());
                if (calculatedHeightFromWidth == height) 
                    return new Vector2<int>(width, height);
                throw new ArgumentException($"The specified {nameof(DynamicImageExportProfileSize)} {dynamicImageExportProfileSize} did not match the aspect ratio of the actual image. Try specifying only a width or height.");
            }
    }
        private static Vector2<int> ValidateSizeAllowed(Vector2<int> imageSize, Vector2<int> size, DynamicImageExportProfile dynamicImageExportProfile, DynamicImageExportProfileSize dynamicImageExportProfileSize) {
            bool widthGreater = size.X>imageSize.X;
            bool heightGreater = size.Y>imageSize.Y;
            if (widthGreater || heightGreater)
            {
                if (dynamicImageExportProfile.SkipLargerSizesThanSource)
                    return null;
                if (!dynamicImageExportProfile.AllowIncreaseSize)
                {
                    string dimensionName = widthGreater ? (heightGreater ? "width and height" : "width") : "height";
                    throw new ArgumentException($"The {nameof(DynamicImageExportProfileSize)} {dynamicImageExportProfileSize} had a {dimensionName} greater than the source image {imageSize.X}x{imageSize.Y} and {nameof(dynamicImageExportProfile.AllowIncreaseSize)} was false");
                }
            }
            return size;
        }
        private static Func<Vector2<int>, string> Get_GetRelativeFilePathForSize(string relativeDirectoryPath, string fileNamePrefix, string extension) { 
            return (size) => {
                return Path.Combine(relativeDirectoryPath, $"{fileNamePrefix}_{size.X}x{size.Y}{extension}");
            }; 
        }
        private static Func<Vector2<int>, bool> Get_SeenSizeAndAddSeenSizeLexicalClosures(out Action<Vector2<int>> addSeenSize) {
            Dictionary<int, List<int>> mapSeenWidthToSeenHeights = new Dictionary<int, List<int>>();
            addSeenSize = (Vector2<int> size) => {
                if (!mapSeenWidthToSeenHeights.ContainsKey(size.X))
                {
                    mapSeenWidthToSeenHeights[size.X] = new List<int>(size.Y);
                    return;
                }
                List<int> heights = mapSeenWidthToSeenHeights[size.X];
                if (heights.Contains(size.Y)) return;
                heights.Add(size.Y);
            };
            return (size) => {
                if (!mapSeenWidthToSeenHeights.ContainsKey(size.X)) return false;
                return mapSeenWidthToSeenHeights[size.X].Contains(size.Y);
            };
        }
    }
}