using Core.Maths.CUBLAS;
using Core.Messages.Messages;
using InfernoDispatcher.Tasks;
using Logging;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.CudaBlas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;

//using static Core.Maths.CUBLAS.CuBlasInterop;
using Timer = System.Timers.Timer;
namespace Core.Threading.ContextAssigned
{

    public abstract class ContextAssignedThreadPoolWithoutSingleThreadDispatching<THandle> : IDisposable
    {
        private LinkedList<InfernoTask> _Waitings = new LinkedList<InfernoTask>();
        private LinkedList<Action> _Awakens = new LinkedList<Action>();
        private readonly object _LockObject = new object();
        private bool _Disposed = false;
#if DEBUG
        private Timer _TimerDebug;
#endif
        public ContextAssignedThreadPoolWithoutSingleThreadDispatching(int nContext)
        {
            var exceptions = new List<Exception>();
            CountdownLatch countdownLatchInitialized = new CountdownLatch(nContext);
            for (int i = 0; i < nContext; i++)
            {
                new Thread(() => OnNewThread(countdownLatchInitialized, exceptions)).Start();
            }
            countdownLatchInitialized.Wait();
            lock (exceptions)
            {
                if (exceptions.Any())
                {
                    throw new AggregateException(exceptions);
                }
            }
#if DEBUG
            _TimerDebug = new Timer(30000);
            _TimerDebug.Elapsed += Debug;
            _TimerDebug.AutoReset = true;
            _TimerDebug.Enabled = true;
            _TimerDebug.Start();
#endif
        }
        protected abstract void OnNewThread(CountdownLatch countdownLatchInitialized, List<Exception> initializeExceptions);
        protected void Cycle(THandle cublasHandle)
        {
            InfernoTask next = null;
            CountdownLatch countdownLatch = null;
            while (true)
            {
                lock (_LockObject)
                {
                    if (_Disposed)
                    {
                        break;
                    }
                    next = _Waitings.FirstOrDefault();
                    if (next != null)
                    {
                        _Waitings.RemoveFirst();
                    }
                    else
                    {
                        countdownLatch = new CountdownLatch();
                        _Awakens.AddLast(() => countdownLatch.Signal());
                    }
                }
                if (next != null)
                {
                    try
                    {
                        next.Run(new object[] { cublasHandle });
                    }
                    catch (Exception ex)
                    {
                        Logs.Default.Error(ex);
                        //Just incase
                    }
                    next = null;
                }
                else
                {
                    countdownLatch.Wait();
                }
            }
        }
        public InfernoTaskNoResultBase UsingContext(Action<THandle> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            InfernoTaskNoResultBase task =
                new InactiveInfernoTaskNoResultArgument<THandle>(callback);
            Action? awaken;
            lock (_LockObject)
            {
                _Waitings.AddLast(task);
                awaken = _Awakens.First();
                if (awaken != null)
                    _Awakens.RemoveFirst();
            }
            awaken?.Invoke();
            return task;
        }
        public InfernoTaskWithResultBase<TResult> UsingContext<TResult>(Func<THandle, TResult> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            InfernoTaskWithResultBase<TResult> task =
                new InactiveInfernoTaskWithResultArgument<THandle, TResult>(callback);
            Action? awaken;
            lock (_LockObject)
            {
                if (_Disposed)
                {
                    throw new ObjectDisposedException(nameof(CudaContextAssignedThreadPool));
                }
                _Waitings.AddLast(task);
                awaken = _Awakens.FirstOrDefault();
                if (awaken != null)
                    _Awakens.RemoveFirst();
            }
            awaken?.Invoke();
            return task;
        }
        public void Dispose()
        {
            Action[] awakens;
            InfernoTask[] waitings;
            lock (_LockObject)
            {
                _Disposed = true;
                awakens = _Awakens.ToArray();
                _Awakens.Clear();
                waitings = _Waitings.ToArray();
                _Waitings.Clear();
            }
#if DEBUG
            _TimerDebug.Dispose();
            _TimerDebug = null;
#endif
            foreach (var awaken in awakens)
            {
                awaken();
            }
            foreach (var waiting in waitings)
            {
                waiting.Fail(new ObjectDisposedException(nameof(CudaContextAssignedThreadPool)));
            }
        }
#if DEBUG
        private void Debug(object sender, ElapsedEventArgs e)
        {
            bool disposed;
            InfernoTask[] waitings;
            Action[] awakens;
            lock (_LockObject)
            {
                disposed = _Disposed;
                waitings = _Waitings.ToArray();
                awakens = _Awakens.ToArray();
            }
        }
#endif
    }
}