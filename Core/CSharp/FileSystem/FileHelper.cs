using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Core.FileSystem
{
    public static class FileHelper
    {
        public static string NormalizeFileNameLowerCaseUnderscore(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            // Convert camel case to lowercase with underscores
            fileName = Regex.Replace(fileName, "([a-z0-9])([A-Z])", "$1_$2");

            // Convert to lowercase
            fileName = fileName.ToLowerInvariant();

            // Replace any non-alphanumeric character (except underscore) with a space
            fileName = Regex.Replace(fileName, @"[^a-z0-9_]", " ");

            // Replace spaces with underscores
            fileName = Regex.Replace(fileName, @"\s+", "_");

            // Trim any trailing underscores
            fileName = fileName.Trim('_');

            return fileName;
        }
        public static bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
    }
}
