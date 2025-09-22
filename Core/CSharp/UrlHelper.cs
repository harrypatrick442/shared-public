using System;
using System.Linq;

namespace Core
{
    public static class UrlHelper
    {
        public static bool UrlIsValid(string url, params string[] schemes)
        {
            Uri uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (schemes.Contains(uriResult.Scheme));
        }
        public static string SetSlashes(bool startSlash, string url,  bool endSlash) {
            url = SetStartSlash(startSlash, url);
            url = SetEndSlash(url, endSlash);
            return url;
        }
        public static string SetEndSlash(string url, bool endSlash) {
            if (url == null) return null;
            char lastCharacter = url[url.Length - 1];
            if (endSlash)
            {
                if (lastCharacter != '/')
                    url = url + '/';
            }
            else
            {
                if (lastCharacter== '/')
                    url = url.Substring(0, url.Length - 1);
            }
            return url;
        }
        public static string SetStartSlash(bool startSlash, string url)
        {
            if (url == null) return null;
            if (startSlash)
            {
                if (url[0] != '/')
                    url = '/' + url;
            }
            else
            {
                if (url[0] == '/')
                    url = url.Substring(1, url.Length - 1);
            }
            return url;
        }
    }
}