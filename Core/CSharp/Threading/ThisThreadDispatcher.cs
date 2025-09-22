using Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
namespace Core.Threading {
    public class ThisThreadDispatcher:IDisposable
    {
        private Thread _Thread;
        private object _LockObjectDispose = new object();
        private CancellationTokenSource _CancellationTokenSourceDisposed = new CancellationTokenSource();
        private List<Action> _ActionsToRun = new List<Action>();
        private CountdownLatch _CountdownLatchHasActionsToRun;
        public ThisThreadDispatcher() {

        }
        public void Run(Action action)
        {
            CheckNotDisposed();
            if (CheckNotSameThread()) { 
                action(); 
                return;
            }
            CountdownLatch countdownLatch = new CountdownLatch();
            Exception exception = null;
            Action toRun = () => {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    countdownLatch.Signal();
                }
            };
            lock (_ActionsToRun) {
                _ActionsToRun.Add(toRun);
                _CountdownLatchHasActionsToRun?.Signal();
            }
            countdownLatch.Wait();
            if (exception != null) throw new OperationFailedException("Exception occured while running on main thread", exception);
        }

        private bool CheckNotSameThread() {

            //if
            return(Thread.CurrentThread.ManagedThreadId == _Thread.ManagedThreadId) ;
               // throw new Exception("Already being called from the dispatch thread");
        }
        public T Run<T>(Func<T>func)
        {
            CheckNotDisposed();
            if (CheckNotSameThread()) { return func();  }
            CountdownLatch countdownLatch = new CountdownLatch();
            Exception exception = null;
            T result = default(T);
            Action toRun = () => {
                try
                {
                    result = func();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    countdownLatch.Signal();
                }
            };
            lock (_ActionsToRun)
            {
                _ActionsToRun.Add(toRun);
                _CountdownLatchHasActionsToRun?.Signal();
            }
            countdownLatch.Wait();
            if (exception != null) throw new OperationFailedException("Exception occured while running on main thread", exception);
            return result;
        }
        private void CheckNotDisposed()
        {
            if (_CancellationTokenSourceDisposed.IsCancellationRequested) throw new ObjectDisposedException(nameof(ThisThreadDispatcher));
        }

        public void LoopOnThisThread()
        {
            _Thread = Thread.CurrentThread;
            while (!_CancellationTokenSourceDisposed.IsCancellationRequested) {
                Action[] actionsToRun;
                lock (_ActionsToRun)
                {
                    actionsToRun = _ActionsToRun.ToArray();
                    _ActionsToRun.Clear();
                    _CountdownLatchHasActionsToRun?.Signal();//being safe
                    _CountdownLatchHasActionsToRun = new CountdownLatch();
                }
                foreach (Action actionToRun in actionsToRun)
                {
                    actionToRun();
                }
                _CountdownLatchHasActionsToRun.Wait(_CancellationTokenSourceDisposed.Token);
            }
        }
        ~ThisThreadDispatcher() {
            Dispose();
        }
        public void Dispose() {
            lock (_LockObjectDispose) {
                if (_CancellationTokenSourceDisposed.IsCancellationRequested) return;
                _CancellationTokenSourceDisposed.Cancel();
            }
        }
    }
}
