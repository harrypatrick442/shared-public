using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Core.NativeExtensions
{

    public static class ZipArchiveExtensions
    {
        public static void ExtractToDirectory(this ZipArchive source, string destinationDirectoryName, Action<double> setProgressProportion = null, CancellationToken? cancellationToken = null)
        {
            ExtractToDirectory(source, destinationDirectoryName, false, setProgressProportion, cancellationToken);
        }

        public static void ExtractToDirectory(this ZipArchive source, string destinationDirectoryName, 
            bool overwrite = true, Action<double> setProgressProportion = null, CancellationToken? cancellationToken=null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (destinationDirectoryName == null)
                throw new ArgumentNullException(nameof(destinationDirectoryName));
            DirectoryInfo directoryInfo = Directory.CreateDirectory(destinationDirectoryName);
            string destinationDirectoryFullPath = directoryInfo.FullName;
            double nEntries = source.Entries.Count, count = 0;
            foreach (ZipArchiveEntry zipArchiveEntry in source.Entries)
            {
                if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
                    return;
                count++;
                string fileDestinationPath = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, zipArchiveEntry.FullName));
                if (!fileDestinationPath.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("File is extracting to outside of the folder specified.");

                if (Path.GetFileName(fileDestinationPath).Length == 0)
                {
                    if (zipArchiveEntry.Length != 0) throw new IOException("Directory entry with data.");
                    Directory.CreateDirectory(fileDestinationPath);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileDestinationPath));
                    zipArchiveEntry.ExtractToFile(fileDestinationPath, overwrite: overwrite);
                }
                setProgressProportion?.Invoke(count / nEntries);
            }
            setProgressProportion?.Invoke(1);
        }
    }
}
