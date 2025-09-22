using System.Threading;
using System;
namespace Core.Threading {
    public class Semaphore
    {
        public event EventHandler OnCountCurrentIncrement;
        private volatile int _Count;
        public int Count { get { return _Count; } }
        private System.Threading.Semaphore _Semaphore;
        public Semaphore(int count) {
            _Count = count;
            _Semaphore = new System.Threading.Semaphore(count, count);
        }
        public SemaphoreHandle WaitOne()
        {
            if (_Semaphore.WaitOne())
            {
                SemaphoreHandle semaphoreHandle = new SemaphoreHandle(_Semaphore, OnRelease);
                _Count--;
                return semaphoreHandle;
            }
            return null;
        }
        public SemaphoreHandle WaitOne(int timeoutMilliseconds)
        {
            if (_Semaphore.WaitOne(timeoutMilliseconds))
            {
                SemaphoreHandle semaphoreHandle =  new SemaphoreHandle(_Semaphore, OnRelease);
                _Count--;
                return semaphoreHandle;
            }
            return null;
        }
        private void OnRelease()
        {
            _Count++;
            InvokeCountIncrement();
        }
        private void InvokeCountIncrement() {
            OnCountCurrentIncrement?.Invoke(this, new EventArgs());
        }
    }
}
