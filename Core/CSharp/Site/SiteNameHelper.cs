using Core.Strings;
namespace Snippets.UnityCore.Site {
    public class SiteNameHelper
    {
        public static string NormalizeSiteName(string displayName) {
            if (displayName == null) return null;
            return StringHelper.MultipleReplace(displayName.ToString(), new string[] { "-", "_", " ", "'", "`", "’" }, "").ToLower();
        }
    }
}