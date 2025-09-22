using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Core.FileSystem
{

    public class WorkingDirectoryManager
    {
        protected string _DirectoryPath;
        private readonly object _LockObject = new object();
        private int _FileNameCount;
        public WorkingDirectoryManager(string directoryPath) {
            _DirectoryPath = directoryPath;
        }
        protected WorkingDirectoryManager() { 
        
        }
        public string NewBinFile(string? name=null) {
            lock (_LockObject)
            {
                return Path.Combine(_DirectoryPath, $"{name??""}_{_FileNameCount++}.bin");
            }
        }
    }
}
