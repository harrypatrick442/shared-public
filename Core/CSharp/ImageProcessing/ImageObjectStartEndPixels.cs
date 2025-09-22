namespace Snippets.Core.ImageProcessing
{
    public class ImageObjectStartEndPixels
    {
        private int? Inclusive;
        public int? FromInclusive { get { return Inclusive; } }
        public bool Has{ get { return Inclusive!= null; } }
        private int? _ToExclusive;
        public int? ToExclusive { get { return _ToExclusive; } }
        public ImageObjectStartEndPixels(int? fromInclusive, int? toExclusive) {
            Inclusive = fromInclusive;
            _ToExclusive = toExclusive;
        }
    }
}
