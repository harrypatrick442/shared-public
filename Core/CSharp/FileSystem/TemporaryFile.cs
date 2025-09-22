using System;
using System.IO;
namespace Core.FileSystem {
    public class TemporaryFile:IDisposable
    {
        private object _LockObjectDispose = new object();
        private bool _Disposed = false;
        private string _FilePath;
        public string FilePath { get { return  _FilePath; } }
        public TemporaryFile(string extension, bool useLocalApplicationData = false) {
            if (extension == null) throw new ArgumentNullException(nameof(extension));
            if (extension.Length < 1) extension = ".tmp";
            else if(extension[0]!='.')extension = $".{ extension}";
            if (useLocalApplicationData)
            {
                _FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Guid.NewGuid().ToString()+extension);
            }
            _FilePath = $"{System.IO.Path.GetTempPath()}{Guid.NewGuid().ToString() }{extension}";
        }
        ~TemporaryFile() {
            Dispose();
        }
        public void Dispose() {
            lock (_LockObjectDispose) {
                if (_Disposed) return;
                    try { File.Delete(_FilePath); } catch { }
                _Disposed = true;
            }
        }
    }
}
