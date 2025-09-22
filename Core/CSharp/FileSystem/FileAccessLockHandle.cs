using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Core.FileSystem {
    public class FileAccessLockHandle:IDisposable
    {
        private object _LockObjectDispose = new object();
        private bool _Disposed = false;
        private string _FilePath;
        public string FilePath { get { return _FilePath; } }
        private CountdownLatch _CountdownLatch;
        private Action<FileAccessLockHandle> _Remove;
        public FileAccessLockHandle(string filePath, CountdownLatch countdownLatch, Action<FileAccessLockHandle> remove) {
            _FilePath = filePath;
            _CountdownLatch = countdownLatch;
            _Remove = remove;
        }
        ~FileAccessLockHandle()
        {
            Dispose();
        }
        public void Dispose() {
            lock (_LockObjectDispose) {
                if (_Disposed) return;
                _Remove(this);
                _Disposed = true;
            }
        }
    }
}
