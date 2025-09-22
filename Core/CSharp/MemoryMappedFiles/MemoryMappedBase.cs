using Logging;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Core.MemoryMappedFiles
{
    public abstract class MemoryMappedBase
    {
        protected static object _LockObjectDispose = new object();
        protected volatile bool _Disposed = false;
        protected MemoryMappedFile _MemoryMappedFile;
        protected MemoryMappedViewAccessor _MemoryMappedViewAccessor;
        protected int _Size;
        private string _Path;
        public MemoryMappedBase(int size, string path) {
            _Size = size;
            _Path = path;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (!File.Exists(path)) {
                File.Create(path).Close();
            }
            FileStream fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            _MemoryMappedFile = MemoryMappedFile.CreateFromFile(fileStream,
                GetNameFromPath(path), _Size, MemoryMappedFileAccess.ReadWrite, 
                //new MemoryMappedFileSecurity { }, 
                HandleInheritability.Inheritable, false);
            try
            {
                _MemoryMappedViewAccessor = _MemoryMappedFile.CreateViewAccessor(0, _Size);
            }
            finally
            {
                _MemoryMappedFile.Dispose();
            }
        }
        protected static string GetNameFromPath(string path)
        {
            return path.Replace("\\", "_").Replace(":", "_");
        }
        public void Dispose() {
            Dispose(false);
        }
        public void Dispose(bool attemptToDeleteFile)
        {
            lock (_LockObjectDispose)
            {
                if (_Disposed) return;
                _MemoryMappedViewAccessor.Dispose();
                _MemoryMappedFile.Dispose();
                if(attemptToDeleteFile)
                    AttemptToDeleteFile();
                _Disposed = true;
            }
        }
        private void AttemptToDeleteFile() {
            try
            {
                File.Delete(_Path);
            }
            catch (Exception ex)
            {
            }
        }
        ~MemoryMappedBase()
        {
            Dispose();
        }
    }
}
