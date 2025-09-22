
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Core.NativeExtensions;
using Core.FileSystem;
using Logging;
using Core.Cleanup;

namespace Core
{
    public class ZipResourceUnpackingHelper
    {
        private static readonly string[] PROPERTY_NAMES_TO_IGNORE = new string[] { "ResourceManager", "Culture" };
        public static CleanupHandle Unpack(Type resourceType, string directoryToPath, 
            bool unzipOverwritingIfDirectoryAlreadyExists = true, bool returnCleanupHandle=false)
        {
            PropertyInfo[] resourcePropertyInfos = GetResourcePropertyInfos(resourceType);
            List<string> unzippedDirectoryPaths = new List<string>();
            foreach (PropertyInfo resourcePropertyInfo in resourcePropertyInfos)
            {
                string unzippedDirectoryPath = UnzipZipFromResource(resourcePropertyInfo, directoryToPath, unzipOverwritingIfDirectoryAlreadyExists);
                unzippedDirectoryPaths.Add(unzippedDirectoryPath);
            }
            if (!returnCleanupHandle) return null;
            return new CleanupHandle(() => {
                foreach (string unzippedDirectoryPath in unzippedDirectoryPaths)
                    DirectoryHelper.DeleteRecursively(unzippedDirectoryPath, false);
            });
        }
        private static string UnzipZipFromResource(PropertyInfo resourcePropertyInfo,
            string directoryPathTo, bool unzipOverwritingIfDirectoryAlreadyExists)
        {
            string unzippedDirectoryPath = Path.Combine(directoryPathTo, resourcePropertyInfo.Name);
            if (!unzipOverwritingIfDirectoryAlreadyExists && Directory.Exists(unzippedDirectoryPath))
                return unzippedDirectoryPath;
            string zipFilePath = $"{unzippedDirectoryPath}.zip";
            byte[] bytes = GetBytesForProperty(resourcePropertyInfo);
            DeleteZipIfExists(zipFilePath);
            DeleteUnzippedDirectoryIfExists(unzippedDirectoryPath);
            try
            {
                SaveBytesAsZipFile(bytes, zipFilePath);
                ZipFileHelper.ExtractToDirectory(zipFilePath, unzippedDirectoryPath);
            }
            finally {
                try { File.Delete(zipFilePath); } catch { }
            }
            return unzippedDirectoryPath;
        }
        private static void DeleteZipIfExists(string zipFilePath) { 
            if(File.Exists(zipFilePath)) {
                try
                {
                    File.Delete(zipFilePath);
                }
                catch(Exception ex){ Logs.Default.Debug(ex); }    
            }
        }
        private static void DeleteUnzippedDirectoryIfExists(string unzippedDirectoryPath) {
            if (Directory.Exists(unzippedDirectoryPath)) {
                try
                {
                    DirectoryHelper.DeleteRecursively(unzippedDirectoryPath);
                }
                catch (Exception ex) {
                    Logs.Default.Debug(ex);
                }
            }
        }
        private static void SaveBytesAsZipFile(byte[] bytes, string zipFilePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(zipFilePath));
            using (FileStream fileStream = File.Open(zipFilePath, FileMode.Create))
            {
                    fileStream.Write(bytes, 0, bytes.Length);
            }
        }
        private static byte[] GetBytesForProperty(PropertyInfo resourcePropertyInfo)
        {
            return (byte[])resourcePropertyInfo.GetValue(null);
        }
        private static PropertyInfo[] GetResourcePropertyInfos(Type resourceType)
        {
            return resourceType.GetProperties(BindingFlags.Static | BindingFlags.NonPublic)
          .Where(propertyInfo => !PROPERTY_NAMES_TO_IGNORE.Contains(propertyInfo.Name)).ToArray();
        }
    }
}
