using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Core.FileSystem
{

    public class TemporaryWorkingDirectoryManager :WorkingDirectoryManager, IDisposable
    {
        private TemporaryDirectory _TemporaryDirectory;
        public TemporaryWorkingDirectoryManager() : base()
        {
            _TemporaryDirectory = new TemporaryDirectory();
            _DirectoryPath = _TemporaryDirectory.AbsolutePath;
        }
        public TemporaryWorkingDirectoryManager(string customDirectoryAbsolutePath) : base()
        {
            _DirectoryPath = customDirectoryAbsolutePath;
            Directory.CreateDirectory(_DirectoryPath);
        }
        ~TemporaryWorkingDirectoryManager() {
            Dispose();
        }
        public void Dispose() {
            _TemporaryDirectory?.Dispose();
        }
    }
}
