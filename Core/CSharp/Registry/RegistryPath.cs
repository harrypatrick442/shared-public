using Microsoft.Win32;
using System;
using System.Text.RegularExpressions;

namespace Core.Registry
{
    public class RegistryPath
    {
        private static Regex __RegexSplitAbsolute;
        private static Regex _RegexSplitAbsolute
        {
            get
            {
                if (__RegexSplitAbsolute == null)
                    __RegexSplitAbsolute = new Regex($"([a-zA-Z0-9_-]+)\\\\(.+)$");
                return __RegexSplitAbsolute;
            }
        }
        private static Regex __RegexSplitAbsolutePathAndKey;
        private static Regex _RegexSplitAbsolutePathAndKey { get {
                if (__RegexSplitAbsolutePathAndKey == null)
                    __RegexSplitAbsolutePathAndKey = new Regex($"(.+)\\\\([a-zA-Z0-9_-]+)$");
                return __RegexSplitAbsolutePathAndKey;
            } }
        private string _Path, _Key;
        private RegistryHive _Hive;
        public RegistryHive Hive { get { return _Hive; } }
        public string Path { get { return _Path; } }
        public string Key { get { return _Key; } }
        public RegistryPath(RegistryHive hive, string path, string key) {
            _Hive = hive;
            if (path[0]== '\\') path =path.Substring(1, path.Length-1);
            if (path[path.Length - 1] == '\\') path = path.Substring(1, path.Length - 1);
            _Path = path;
            _Key = key;
        }
		public override string ToString(){
            return Hive.GetPathString()+"\\"+Path+"\\"+Key;
		}
        public static RegistryPath FromAbsolute(string absoluteKey, bool hasKey=false) {
            Match match = _RegexSplitAbsolute.Match(absoluteKey);
            string hive, pathOrPathAndKey;
            if (!match.Success || match.Groups.Count < 2
                || (hive = match.Groups[1].Value).Length < 1
                || (pathOrPathAndKey = match.Groups[2].Value).Length<1)
                throw new FormatException("Could not parse absolute key");
            string path = pathOrPathAndKey, key=null;
            if (hasKey) {
                match = _RegexSplitAbsolutePathAndKey.Match(pathOrPathAndKey);
                if (!match.Success || match.Groups.Count < 2
                    || (path = match.Groups[1].Value).Length < 1
                    || (key = match.Groups[2].Value).Length<1)
                    throw new FormatException("Could not parse absolute key");
            }
            return new RegistryPath(RegistryHiveHelper.Parse(hive), path, key);
        }
    }
}
