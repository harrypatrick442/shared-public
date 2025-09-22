using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Core.NativeExtensions
{

    public static class ZipFileHelper
    {
        public static void ExtractToDirectory(string zipFilePath, string destinationDirectoryPath, Action<double> setProgressProportion = null, CancellationToken? cancellationToken = null)
        {
            using (FileStream fileStream = File.Open(zipFilePath, FileMode.Open))
            {
                new ZipArchive(fileStream).ExtractToDirectory(destinationDirectoryPath, setProgressProportion, cancellationToken);
            }
        }
        public static void ZipDirectory(string sourceDirectoryPath, string zipFilePath)
        {
                ZipFile.CreateFromDirectory(sourceDirectoryPath, zipFilePath);
        }
    }
}
