namespace Core.NativeExtensions
{
    public static class PathExtensions {
        public static string SetEndSlashes(this string path, bool startSlash, bool endSlash){
            if (path == null) return null;
            bool containsBackSlash = path.Contains("\\");
            bool containsForwardSlash = path.Contains("/");
            char slash;
            if (containsForwardSlash && !containsBackSlash) slash = '/';
            else slash = '\\';
            char firstChar = path[0];
            bool hasStartSlash = firstChar == '\\'||firstChar=='/';
            char lastChar = path[path.Length - 1];
            bool hasEndSlash = lastChar == '\\'||lastChar=='/';
            if (hasStartSlash) {
                if (!startSlash)
                {
                    path = path.Substring(1, path.Length - 1);
                }
            }
            else {
                if (startSlash) {
                    path = slash + path;
                }
            }
            if (hasEndSlash)
            {
                if (!endSlash)
                {
                    path = path.Substring(0, path.Length - 1);
                }
            }
            else {
                if (endSlash) {
                    path = path + slash;
                }
            }
            return path;
        }
    }
}
