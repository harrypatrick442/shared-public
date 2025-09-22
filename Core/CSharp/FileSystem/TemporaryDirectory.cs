using System;
using System.IO;

namespace Core.FileSystem {
    public class TemporaryDirectory : IDisposable
    {
        private readonly object _LockObjectDispose = new object();
        private bool _Disposed = false;
        private string _AbsolutePath;
        public string AbsolutePath { get { return _AbsolutePath; } }
        private bool _DoNotDispose = false;
        public void DoNotDispose() {
            lock (_LockObjectDispose)
            {
                _DoNotDispose = true;
            }
        }
        public TemporaryDirectory() {
            do
            {
                _AbsolutePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("D"));
            } while (Directory.Exists(_AbsolutePath));
            Directory.CreateDirectory(_AbsolutePath);
        }
        protected TemporaryDirectory(string absolutePath) { 
            _AbsolutePath = absolutePath;
        }
        ~TemporaryDirectory() {
            Dispose();
        }
        public void Dispose() {
            lock (_LockObjectDispose) {
                if (_DoNotDispose) {
                    return;
                }
                if (_Disposed) return;
                _Disposed = true;
            }
            DirectoryHelper.DeleteRecursively(_AbsolutePath, throwOnError: false);
        }
        public static TemporaryDirectory InCustomTempDirectory(string customTempPath) {
            string absolutePath;
            do
            {
                absolutePath = Path.Combine(customTempPath, Guid.NewGuid().ToString("D"));
            } while (Directory.Exists(absolutePath));
            Directory.CreateDirectory(absolutePath);
            return new TemporaryDirectory(absolutePath);
        }
    }
}
