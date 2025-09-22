using System;
using System.Threading;
namespace Core.Threading {
    public class SemaphoreHandle:IDisposable
    {
        private object _LockObjectRelease = new object();
        private volatile bool _Released = false;
        private System.Threading.Semaphore _Semaphore;
        private Action _CallbackRelease;
        public SemaphoreHandle(System.Threading.Semaphore semaphore, Action callbackRelease) {
            _Semaphore = semaphore;
            _CallbackRelease = callbackRelease;
        }
        public void Release() {
            lock (_LockObjectRelease){
                if (_Released) return;
                _Released = true;
                _CallbackRelease();
            }
            _Semaphore.Release();
        }
        public void Dispose()
        {
            Release();
        }
        ~SemaphoreHandle() {
            Dispose();
        }
    }
}
