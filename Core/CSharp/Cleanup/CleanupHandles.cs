using Logging;
using System;
using System.Collections.Generic;

namespace Core.Cleanup
{
    public class CleanupHandles : IDisposable
    {
        private readonly List<Action> _Callbacks = new List<Action>();
        private readonly object _LockObject = new object();
        private bool _Disposed = false;

        public CleanupHandles Add(Action callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            lock (_LockObject)
            {
                _Callbacks.Add(callback);
            }
            return this;
        }

        public CleanupHandles Add(IDisposable disposable)
        {
            if (disposable == null) throw new ArgumentNullException(nameof(disposable));
            lock (_LockObject)
            {
                _Callbacks.Add(disposable.Dispose);
            }
            return this;
        }

        public void Dispose()
        {
            List<Action> callbacks;
            lock (_LockObject)
            {
                if (_Disposed) return;
                _Disposed = true;
                callbacks = new List<Action>(_Callbacks);
            }

            foreach (var callback in callbacks)
            {
                try
                {
                    callback();
                }
                catch (Exception ex)
                {
                    Logs.Default.Error(ex);
                }
            }
            GC.SuppressFinalize(this);
        }
    }
}
