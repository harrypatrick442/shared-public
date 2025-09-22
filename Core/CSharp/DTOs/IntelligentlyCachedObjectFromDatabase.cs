using System.IO;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Snippets.Enums;
using System;
namespace Snippets.FileWrappers
{

    [DataContract]
    public class IntelligentlyCachedObjectFromDatabase<TObject>
    {
        private object _LockObject = new object();
        private TObject _Object;
        private DateTime? _CachedAt;
        Func<TObject> _GetObject;
        public IntelligentlyCachedObjectFromDatabase(Func<TObject> getObject) {
            _GetObject = getObject;
        }
        public TObject Get()
        {
            lock (_LockObject)
            {
                if (_Object == null||CachedTooLongAgo()) {
                    _CachedAt = DateTime.UtcNow;
                    _Object = _GetObject();
                    if (_Object == null) throw new NullReferenceException($"Failed to get the {typeof(TObject).Name} from the database");
                }
                return _Object;
            }
        }
        private bool CachedTooLongAgo() {
            if (_CachedAt == null) return false;
            return ((DateTime)_CachedAt).AddSeconds(30) < DateTime.UtcNow;
        }
        protected IntelligentlyCachedObjectFromDatabase() { }
    }
}
