using Core.Delegates;
using Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Core.FileSystem {
    public static class DirectoryHelper
    {

        public static void DeleteEmptyDirectoriesRecursively(DirectoryInfo directoryInfoRoot, string directoryPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            while (
                (directoryInfo != null)
                && (directoryInfo.FullName != directoryInfoRoot.FullName)
                && !directoryInfo.EnumerateFiles().Any()
                && !directoryInfo.EnumerateDirectories().Any()
            )
            {
                DirectoryInfo parent = directoryInfo.Parent;
                directoryInfo.Delete();
                directoryInfo = parent;
            }
        }
        public static DelegateNextEntry<string> GetFilesRecursivelyAsynchronously(DirectoryInfo directoryInfo)
        {
            IEnumerator<DirectoryInfo> childDirectoryInfos = 
                directoryInfo.EnumerateDirectories().GetEnumerator();
            DelegateNextEntry<string> _iterateRecursively = null;
            DelegateNextEntry<string> nextInternalDirectory = (out string path) =>
            {
                if (_iterateRecursively != null)
                {
                    if (_iterateRecursively(out path))
                    {
                        return true;
                    }
                }
                bool has = false;
                do
                {
                    if (!childDirectoryInfos.MoveNext())
                    {
                        path = null;
                        return false;
                    }
                    _iterateRecursively = GetFilesRecursivelyAsynchronously(childDirectoryInfos.Current);
                }
                while (!(has=_iterateRecursively(out path)));
                return has;
            };
            IEnumerator<FileInfo> files = directoryInfo.EnumerateFiles().GetEnumerator();
            DelegateNextEntry<string> nextInternal = (out string path) => {
                if (files.MoveNext())
                {
                    path =  files.Current.FullName;
                    return true;
                }
                nextInternal = nextInternalDirectory;
                return nextInternal(out path);
            };
            return (out string path) => { return nextInternal(out path); };
        }
        public static string[] GetDirectoriesRecursively(string parentDirectory) {
            List<string> directories = new List<string>();
            GetDirectoriesRecursively(parentDirectory, directories);
            return directories.ToArray();
        }
        private static void GetDirectoriesRecursively(string parentDirectory, List<string> directories) {
            foreach (string childDirectory in Directory.GetDirectories(parentDirectory))
            {
                directories.Add(childDirectory);
                GetDirectoriesRecursively(childDirectory, directories);
            }
        }
        public static void DeleteRecursively(string path, bool throwOnError = true, Func<DirectoryInfo, bool> checkCanDeleteDirectory = null, Func<FileInfo, bool> checkCanDeleteFile = null) {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
                return;
            DeleteRecursively(directoryInfo, throwOnError, checkCanDeleteDirectory, checkCanDeleteFile);
        }
        public static void DeleteRecursively(DirectoryInfo directoryInfo, bool throwOnError = true, Func<DirectoryInfo, bool> checkCanDeleteDirectory = null, Func<FileInfo, bool> checkCanDeleteFile = null)
        {
            _DeleteRecursively(directoryInfo, throwOnError, checkCanDeleteDirectory, checkCanDeleteFile);
        }

        public static bool _DeleteRecursively( DirectoryInfo directoryInfo, bool throwOnError = true,
            Func<DirectoryInfo, bool> checkCanDeleteDirectory = null, Func<FileInfo, bool> checkCanDeleteFile = null)
        {
            bool cantDelete = false;
            foreach (var childDirectoryInfo in directoryInfo.EnumerateDirectories())
            {
                cantDelete|=_DeleteRecursively(childDirectoryInfo, throwOnError, checkCanDeleteDirectory, checkCanDeleteFile);
            }
            var files = directoryInfo.GetFiles();
            foreach (var file in files) 
            {
                if (checkCanDeleteFile != null && !checkCanDeleteFile.Invoke(file))
                {
                    cantDelete = true;
                    continue;
                }
                file.IsReadOnly = false;
                try
                {
                    file.Delete(); 
                }
                catch (Exception ex) {
                    if (throwOnError) throw new OperationFailedException("Recursive deletion failed", ex);
                    else Console.WriteLine(ex);
                }
            }
            if (cantDelete) return true;
            if (checkCanDeleteDirectory != null && !checkCanDeleteDirectory.Invoke(directoryInfo)) return true;
            try
            {
                directoryInfo.Delete();
            }
            catch (Exception ex) {
                if (throwOnError) throw new OperationFailedException("Recursive deletion failed",  ex);
                else Console.WriteLine(ex);
            }
            return cantDelete;
        }
        public static void CopyRecurively(string directoryFrom, string directoryTo, Func<string, bool> canCopyPath = null, bool allowErrors = false)
        {
            CopyRecurively(new DirectoryInfo(directoryFrom), new DirectoryInfo(directoryTo), canCopyPath, allowErrors);
        }
        public static void CopyRecurively(DirectoryInfo directoryFrom, DirectoryInfo directoryTo, Func<string, bool> canCopyPath = null, bool allowErrors = false)
        {
            if (canCopyPath != null && !canCopyPath(PathHelper.NormalizePath(directoryFrom.FullName)))
                return;
            Directory.CreateDirectory(directoryTo.FullName);
            var childDirectories = directoryFrom.GetDirectories();
            foreach (DirectoryInfo dir in childDirectories)
            {
                CopyRecurively(dir, new DirectoryInfo(Path.Combine(directoryTo.FullName, dir.Name)), canCopyPath, allowErrors);
            }
            foreach (FileInfo file in directoryFrom.GetFiles())
            {
                if (canCopyPath != null && !canCopyPath(PathHelper.NormalizePath(file.FullName))) continue;
                string filePath = Path.Combine(directoryTo.FullName, file.Name);
                try
                {
                    file.CopyTo(filePath, true);
                }
                catch(Exception ex)
                {
                    if (!allowErrors)
                        throw;
                }
            }
        }
        public static FileInfo[] GetFilesRecursively(DirectoryInfo directory)
        {
            List<FileInfo> files = new List<FileInfo>();
            GetFilesRecursively(directory, files);
            return files.ToArray();
        }
        public static string[] GetFilesRecursively(string directory)
        {
            List<FileInfo> files = new List<FileInfo>();
            GetFilesRecursively(new DirectoryInfo(directory), files);
            return files.Select(file=>file.FullName).ToArray();
        }
        public static void GetFilesRecursively(DirectoryInfo directory, List<FileInfo> files)
        {
            if (!Directory.Exists(directory.FullName)) return;
            foreach (FileInfo file in directory.GetFiles())
                files.Add(file);
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                GetFilesRecursively(dir, files);
            }
        }
        public static string FindFileInDirectoriesRecursiveByFileNameWithExtension(string fileNameWithExtension, string directory) {
            return _FindFileInDirectoriesRecursiveByFileNameWithExtension(new DirectoryInfo(directory), fileNameWithExtension);
        }
        private static string _FindFileInDirectoriesRecursiveByFileNameWithExtension(DirectoryInfo directory, string fileNameWithExtension) {

            try
            {
                foreach (FileInfo file in directory.GetFiles())
                    if (file.Name == fileNameWithExtension)
                        return file.FullName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            try
            {
                foreach (DirectoryInfo dir in directory.GetDirectories())
                {
                    string fileAbsolutePath = _FindFileInDirectoriesRecursiveByFileNameWithExtension(dir, fileNameWithExtension);
                    if (fileAbsolutePath!=null) return fileAbsolutePath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }
        public static string GetProjectDirectory()
        {
            string path = Assembly.GetEntryAssembly()!.Location!;
            string[] segments = path.Split(Path.DirectorySeparatorChar);
            int i = segments.Length - 1;
            while (i > 0)
            {
                if (segments[i] == "bin")
                {
                    return Path.Combine(segments.Take(i).ToArray());
                }
                i--;
            }
            throw new Exception("Could not find project directory");
        }
    }
}
