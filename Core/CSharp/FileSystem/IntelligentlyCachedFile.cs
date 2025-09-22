using System.IO;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Snippets.Enums;
using System;
namespace Snippets.FileWrappers
{

    [DataContract]
    public class IntelligentlyCachedFile<TFileWrapper>
    {
        private object _LockObject = new object();
        private string _FilePath;
        private TFileWrapper _FileWrapper;
        private DateTime? _CachedAt;
        Func<string, TFileWrapper> _ReadFile;
        public IntelligentlyCachedFile(string filePath, Func<string, TFileWrapper> readFile) {
            _FilePath = filePath;
            _ReadFile = readFile;
        }
        protected IntelligentlyCachedFile() { }
        public TFileWrapper Get()
        {
            lock (_LockObject)
            {
                DateTime lastModified = File.GetLastWriteTime(_FilePath);
                if (_FileWrapper == null || _CachedAt == null ||
                        (DateTime)_CachedAt < lastModified)
                {
                    DateTime cachedAt = DateTime.UtcNow;
                    _FileWrapper = _ReadFile(_FilePath);
                    _CachedAt = cachedAt;
                }
                return _FileWrapper;
            }
        }
    }
}
