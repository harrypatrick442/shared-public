using Core.Strings;
namespace Snippets.UnityCore.Site {
    public class HouseTypeHelper
    {
        public static string GetSlugFromDisplayName(string displayName) { 
            return StringHelper.MultipleReplace(displayName.ToString(), new string[] { "-", "_", " " }, "").ToLower();
        }
    }
}