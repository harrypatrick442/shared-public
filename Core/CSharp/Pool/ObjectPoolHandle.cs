using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Core.Pool
{
	public class ObjectPoolHandle<T> :IDisposable where T : class
    {
        public T Object;
        private Action<T> _ReturnObject;
        private readonly object _LockObjectDisposed = new object();
        public ObjectPoolHandle(T obj, Action<T> returnObject)
        {
            Object = obj;
            _ReturnObject = returnObject;
        }
        ~ObjectPoolHandle() {
            Dispose();
        }
        public void Dispose(){
            lock (_LockObjectDisposed) {
                if (_ReturnObject == null)
                    return;
                _ReturnObject(Object);
                _ReturnObject = null;
            }
        }
    }
}
