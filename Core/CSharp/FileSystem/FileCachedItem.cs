using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using Core.Serialization;
using Logging;
namespace Core.Pool
{
	public class FileCachedItem<TObject>
    {
        private string _FilePath;
        private readonly object _LockObject = new object();
        public bool TryGet(out TObject? obj)
        {
            lock (_LockObject)
            {
                try
                {
                    obj = BinaryReflectionDeserializer.Deserialize<TObject>(_FilePath);
                    return true;
                }
                catch (IOException)
                {
                }
                catch (Exception ex) {
                    Logs.Default.Error(ex);
                }
                obj = default(TObject);
                return false;
            }
        }
        public void Set(TObject obj)
        {
            lock (_LockObject)
            {
                BinaryReflectionSerializer.Serialize<TObject>(obj, _FilePath);
            }
        }
        public FileCachedItem(int identifier, string directoryPath) {
            _FilePath = Path.Combine(directoryPath, $"{identifier}.bin");
        }
    }
}
