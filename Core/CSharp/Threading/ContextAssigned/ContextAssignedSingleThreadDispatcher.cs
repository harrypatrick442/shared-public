using System;
using System.Collections.Generic;
using Logging;
using System.Linq;
using InfernoDispatcher.Tasks;
using System.Threading;
using Shutdown;
using Core.Maths.CUBLAS;

namespace Core.Threading.ContextAssigned
{

    public abstract class ContextAssignedSingleThreadDispatcher<TContext> : IDisposable, IContextAssignedSingleThreadDispatcher<TContext>
    {
        protected int _ThreadId;
        public int ThreadId
        {
            get
            {

                lock (_LockObject)
                {
                    return _ThreadId;
                }
            }
        }
        protected CancellationTokenSource _DisposedCancellationToken = new CancellationTokenSource();
        protected LinkedList<InfernoTask> _Waitings = new LinkedList<InfernoTask>();
        protected CountdownLatch? _CountdownLatch = null;
        protected readonly object _LockObject = new object();
        private INonThreadSpecificTaskSource<TContext> _Pool;
        public ContextAssignedSingleThreadDispatcher(INonThreadSpecificTaskSource<TContext> pool)
        {
            _Pool = pool;
            CreateThread();
        }
        public void Awaken()
        {
            lock (_LockObject)
            {
                _CountdownLatch?.Signal();
            }
        }

        public InfernoTaskNoResultBase UsingContext(Action<TContext> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            InfernoTaskNoResultBase task =
                new InactiveInfernoTaskNoResultArgument<TContext>(callback);
            lock (_LockObject)
            {
                if (_DisposedCancellationToken.IsCancellationRequested)
                {
                    throw new ObjectDisposedException(nameof(CudaContextAssignedThreadPool));
                }
                _Waitings.AddLast(task);
                _CountdownLatch?.Signal();
            }
            return task;
        }
        public InfernoTaskWithResultBase<TResult> UsingContext<TResult>(Func<TContext, TResult> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            InfernoTaskWithResultBase<TResult> task =
                new InactiveInfernoTaskWithResultArgument<TContext, TResult>(callback);
            lock (_LockObject)
            {
                if (_DisposedCancellationToken.IsCancellationRequested)
                {
                    throw new ObjectDisposedException(nameof(CudaContextAssignedThreadPool));
                }
                _Waitings.AddLast(task);
                _CountdownLatch?.Signal();
            }
            return task;
        }
        private void SetupCancellation(CancellationToken? providedCancellationToken, out CancellationToken cancellationTokenForCall, out Action? disposeRegistration)
        {
            if (providedCancellationToken != null)
            {
                var source = new CancellationTokenSource();
                var registration = _DisposedCancellationToken.Token.Register(source.Cancel);
                var registration2 = providedCancellationToken.Value.Register(source.Cancel);
                disposeRegistration = () =>
                {
                    registration.Dispose();
                    registration2.Dispose();
                };
                cancellationTokenForCall = source.Token;
            }
            else
            {
                cancellationTokenForCall = _DisposedCancellationToken.Token;
                disposeRegistration = null;
            }
        }
        protected abstract void OnTheThread(CountdownLatch countdownLatchInitialized);
        protected void CreateThread()
        {
            CountdownLatch countdownLatchInitialized = new CountdownLatch();
            Exception? initializeException = null;
            int? threadId = null;
            new Thread(() =>
            {
                Console.WriteLine($"Created {this.GetType().Name} thread with unmanaged id: " + ThreadHelper.GetUnmanagedThreadId());
                lock (_LockObject)
                {
                    threadId = Thread.CurrentThread.ManagedThreadId;
                }
                try
                {
                    OnTheThread(countdownLatchInitialized);
                }
                catch (Exception ex)
                {
                    lock (_LockObject)
                    {
                        initializeException = ex;
                    }
                    countdownLatchInitialized.Signal();
                }
            }).Start();
            countdownLatchInitialized.Wait();
            lock (_LockObject)
            {
                _ThreadId = (int)threadId!;
                if (initializeException != null)
                    throw initializeException;
            }
        }

        protected void Loop(TContext handles)
        {
            InfernoTask? next = null;
            CountdownLatch? countdownLatch = null;
            while (true)
            {
                lock (_LockObject)
                {
                    if (_DisposedCancellationToken.IsCancellationRequested)
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
                        next = _Pool.TakeTaskElseListAwaken(this);
                        if (next == null)
                        {
                            _CountdownLatch = (countdownLatch = new CountdownLatch());
                        }
                    }
                }
                if (next != null)
                {
                    try
                    {
                        next.Run(new object[] { handles });
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
                    countdownLatch!.Wait();
                }
            }
        }

        ~ContextAssignedSingleThreadDispatcher()
        {
            Dispose();
        }
        public void Dispose()
        {
            InfernoTask[] waitings;
            lock (_LockObject)
            {
                _DisposedCancellationToken.Cancel();
                waitings = _Waitings.ToArray();
                _Waitings.Clear();
                _CountdownLatch?.Signal();
            }
            foreach (var waiting in waitings)
            {
                waiting.Fail(new ObjectDisposedException(GetType().Name));
            }
            _DisposedCancellationToken.Dispose();
            GC.SuppressFinalize(this); // No need for finalizer once Dispose is called
        }
    }
}
/*
        private void CheckNotRunningThread()
        {
            if (Thread.CurrentThread.ManagedThreadId == _ThreadId)
            {
                throw new InvalidOperationException("Shouldnt be calling from the running thread");
            }
        }
public InfernoTaskNoResultBase RunAsync(Action<THandle, CancellationToken> action, CancellationToken? cancellationToken = null)
{
    CheckNotRunningThread();

    SetupCancellation(cancellationToken, out CancellationToken cancellationTokenForCall, out Action? disposeCancellationTokenRegistration);

    InactiveInfernoTaskNoResultArgument<(THandle, CancellationToken)> returnTask
        = new InactiveInfernoTaskNoResultArgument<(THandle, CancellationToken)>(
            args => action(args.Item1, args.Item2)); 
    try
    {
        var waiting = (THandle handles) =>
        {
            returnTask.Run(new object[] { (handles, cancellationTokenForCall) });
            returnTask.ThenWhatever(() => {
                disposeCancellationTokenRegistration?.Invoke();
            });
        };
        lock (_LockObject)
        {
            _Waitings.AddLast(returnTask);
            _CountdownLatch?.Signal();
        }
    }
    catch
    {
        disposeCancellationTokenRegistration?.Invoke();
        throw;
    }
    return returnTask;
}
public void RunSync(Action<THandle, CancellationToken> action, CancellationToken? cancellationToken)
{
    CheckNotRunningThread();
    CountdownLatch latch = new CountdownLatch();
    SetupCancellation(cancellationToken, out CancellationToken cancellationTokenForCall, out Action? disposeCancellationTokenRegistration);
    try
    {
        lock (_LockObject)
        {
            _Waitings.AddLast((handles) =>
            {
                try
                {
                    action(handles, cancellationTokenForCall);
                }
                finally
                {
                    disposeCancellationTokenRegistration?.Invoke();
                    latch.Signal();
                }
            });
            _CountdownLatch?.Signal();
        }
    }
    catch
    {
        disposeCancellationTokenRegistration?.Invoke();
        latch.Signal();
        throw;
    }
    latch.Wait();
}*/