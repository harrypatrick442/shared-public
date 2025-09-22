using System.Runtime.InteropServices;

namespace Core.Enums
{
    public enum Platform
    {
        Unknown=0,
        Browser=1,
        Android=2,
        Windows=3,
        IOS=4,
        Mac=5,
        Linux,
    }
    public static partial class PlatformHelper
    {
        private static Platform _SetPlatform;
        public static void SetPlatform(Platform platform) {
            _SetPlatform = platform;
        }
        public static Platform GetPlatform()
        {
            if (_SetPlatform != null) return _SetPlatform;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Platform.Windows;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Platform.Linux;
            return Platform.Unknown;
        }
    }
}