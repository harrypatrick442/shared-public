using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Core.FileSystem
{
    public static class PathHelper
    {
        public static bool ContainsOrEqualsPath(FileSystemInfo containing, FileSystemInfo beingContained)
        {
            return beingContained.FullName.Contains(containing.FullName);
        }
        public static string GetRelativePath(string fromPath, string toPath)
        {

            // Require trailing backslash for path
            string pathSeperator = "" + Path.DirectorySeparatorChar;
            if (!fromPath.EndsWith(pathSeperator))
                fromPath += pathSeperator;

            Uri baseUri = new Uri(fromPath);
            Uri fullUri = new Uri(toPath);

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);

            // Uri's use forward slashes so convert back to backward slashes
            return relativeUri.ToString().Replace("%20", " ").Replace("/", pathSeperator);
        }
        public static string GetLastFolderName(string folderPath) {
            string[] splits = folderPath.Split(Path.DirectorySeparatorChar);
            return splits[splits.Length-1];
        }
        public static bool EqualPaths(string pathA, string pathB)
        {
            return NormalizePath(pathA) == NormalizePath(pathB);
        }
        public static string NormalizePath(string path)
        {
            if (path == null)
                return null;
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToLowerInvariant();
            //Please do not remove the to lower invariant. If this normalization fails the consequences could be very bad. It's needed
            //If a normalized path is being used somewhere where its printed to the console and looks ugly let me know ill fix it 
            //without breaking the normalization for comparison of paths.
        }
        public static string[] NormalizePaths(IEnumerable<string> paths)
        {
            return paths.Select(path => NormalizePath(path)).ToArray();
        }
        public static string GetPathMe()
        {
            return Assembly.GetEntryAssembly().Location;
        }
        public static string ForwardSlash(string path) {
            return path?.Replace("\\", "/");
        }
    }
}
