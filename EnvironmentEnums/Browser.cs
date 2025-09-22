namespace Core.Enums
{
    public enum Browser
    {
        UnknownOrNone=0,
        Chrome=1,
        Firefox=2,
        Opera=3,
        Yandex=4,
        Safari=5,
        InternetExplorer=6,
        Edge=7,
        Chromium=8,
        Ie=9,
        MobileSafari=10,
        EdgeChromium=11,
        MIUI=12,
        SamsungBrowser=13
    }
    public static class BrowserHelper {
        public static Browser FromString(string str)
        {
            if (str == null) return Browser.UnknownOrNone;
            str = str.ToLower();
            if (str.Contains("edgechromium")) return Browser.EdgeChromium;
            if (str.Contains("chromium")) return Browser.Chromium;
            if (str.Contains("chrome")) return Browser.Chrome;
            if (str.Contains("edge")) return Browser.Edge;

            if (str.Contains("internetexplorer")) return Browser.InternetExplorer;
            if (str.Contains("ie")) return Browser.Ie;

            if (str.Contains("mobilesafari")) return Browser.MobileSafari;
            if (str.Contains("safari")) return Browser.Safari;
            if (str.Contains("opera")) return Browser.Opera;
            if (str.Contains("yandex")) return Browser.Yandex;

            if (str.Contains("firefox")) return Browser.Firefox;

            if (str.Contains("miuibrowser")) return Browser.MIUI;
            if (str.Contains("samsungbrowser")) return Browser.SamsungBrowser;
            return Browser.UnknownOrNone;
        }
    }
}