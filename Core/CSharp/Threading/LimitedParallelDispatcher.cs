using Core.Exceptions;
using InfernoDispatcher;
using System;
using System.Threading;

namespace Core.Threading {
    public class LimitedParallelDispatcher:IDisposable
    {
        private object _LockObjectDispose = new object();
        private object _LockObjectOnAllDone = new object();
        private bool _Disposed = false;
        public event EventHandler OnAllDone;
        private int _MaxNAtOnce;
        private Semaphore _Semaphore;
        private volatile int _NScheduled = 0;
        public int NScheduled { get {
                return _NScheduled;
            } }
        public LimitedParallelDispatcher(int maxNAtOnce) {
            _MaxNAtOnce = maxNAtOnce;
            _Semaphore = new Semaphore(maxNAtOnce);
            _Semaphore.OnCountCurrentIncrement += SemaphoreCountIncrement;
        }
        private void SemaphoreCountIncrement(object o, EventArgs e) {
            if (_Semaphore.Count >= _MaxNAtOnce) {
                lock (_LockObjectOnAllDone)
                {
                    OnAllDone?.Invoke(this, new EventArgs());
                }
            }
        }
        public void Run(Action action)
        {
            lock(_LockObjectDispose)
            {
                if (_Disposed)
                    throw new ObjectDisposedException(nameof(LimitedParallelDispatcher));
            }
            _NScheduled++;
            SemaphoreHandle semaphoreHandle = _Semaphore.WaitOne();
            try
            {
                Dispatcher.Instance.Run(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    finally
                    {
                        _NScheduled--;
                        semaphoreHandle.Dispose();
                    }
                });
            }
            catch(Exception ex)
            {
                _NScheduled--;
                semaphoreHandle.Dispose();
                throw new OperationFailedException("Dispatching task failed", ex);
            }
        }
        public void WaitForAllDone(CancellationToken? cancellationTokenExternal= null, int? timeoutMilliseconds = null)
        {
            CountdownLatch countdownLatch = new CountdownLatch();
            Exception exception = null;
            if (timeoutMilliseconds != null)
                Dispatcher.Instance.Run(() =>
                {
                    Thread.Sleep((int)timeoutMilliseconds);
                    exception = new TimeoutException();
                    countdownLatch.Signal();
                });
            EventHandler onAllDoneEventHandler = (o, e) =>
            {
                countdownLatch.Signal();
            };
            lock (_LockObjectOnAllDone)
            {
                OnAllDone += onAllDoneEventHandler;
            }
            try
            {
                if (_Semaphore.Count >= _MaxNAtOnce)
                    return;
                if (cancellationTokenExternal != null)
                {
                    using (CancellationTokenRegistration cancellationTokenRegistratioon = ((CancellationToken)cancellationTokenExternal).Register(() =>
                    {
                        exception =new OperationCanceledException();
                        countdownLatch.Signal();
                    }))
                    {
                        countdownLatch.Wait();
                    }
                }
                else 
                    countdownLatch.Wait();
                RethrowIfHasException(exception);
            }
            finally
            {
                lock (_LockObjectOnAllDone)
                {
                    OnAllDone -= onAllDoneEventHandler;
                }
            }
        }
        private void RethrowIfHasException(Exception exception)
        {
            if (exception != null) throw new OperationFailedException($"Failed to wait for {nameof(LimitedParallelDispatcher)} to all complete", exception);
        }
        ~LimitedParallelDispatcher() {
            Dispose();
        }
        public void Dispose() {
            lock (_LockObjectDispose) {
                if (_Disposed) return;
                _Disposed = true;
                _Semaphore.OnCountCurrentIncrement-=SemaphoreCountIncrement;
            }
        }
    }
}
