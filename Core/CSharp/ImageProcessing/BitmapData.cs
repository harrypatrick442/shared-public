using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
namespace Snippets.Core.ImageProcessing
{
    public class BitmapData
    {
        private Rectangle _Rectangle;
        public int Stride { get { return 4*Width; } }
        public int Height { get { return _Rectangle.Height; } }
        public int Width { get { return _Rectangle.Width; } }
        public BitmapData(Image<Rgba32> sourceBitmap, Rectangle rectangle) {
                _Rectangle = rectangle;
        }
    }
}
