using System.IO;

namespace Logging.Writers
{
    public class TempFileWriter : FileWriter
    {
        public TempFileWriter(string fileName) : base(GetFilePath(fileName))
        {

        }
        private static string GetFilePath(string fileName)
        {
            return Path.Combine(Path.GetTempPath(), fileName);
        }
    }
}
