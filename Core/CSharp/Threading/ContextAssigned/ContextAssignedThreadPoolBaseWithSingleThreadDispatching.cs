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

    public abstract class ContextAssignedThreadPoolBaseWithSingleThreadDispatching<TSingleThreadDispatcher, TContext, THandle>
        : INonThreadSpecificTaskSource<TContext>, IDisposable
        where TSingleThreadDispatcher : ContextAssignedSingleThreadDispatcher<TContext>
        where THandle : ContextAssignedSingleThreadDispatcherHandle<TContext>
    {
        private TSingleThreadDispatcher[] _SingleThreadDispatchers;
        private Dictionary<TSingleThreadDispatcher, int> _MapSingleThreadDispatcherToNTimesInUse = new Dictionary<TSingleThreadDispatcher, int>(0);
        private LinkedList<InfernoTask> _Waitings = new LinkedList<InfernoTask>();
        private HashSet<ContextAssignedSingleThreadDispatcher<TContext>> _Awakens 
            = new HashSet<ContextAssignedSingleThreadDispatcher<TContext>>();
        private readonly object _LockObject = new object();
        private bool _Disposed = false;
#if DEBUG
        private Timer _TimerDebug;
#endif
        public ContextAssignedThreadPoolBaseWithSingleThreadDispatching(
            int nContext)
        {
            _SingleThreadDispatchers = new TSingleThreadDispatcher[nContext];
            for (int i = 0; i < nContext; i++)
            {
                var singleThreadDispatcher = CreateSingleThreadDispatcher();
                _SingleThreadDispatchers[i] = singleThreadDispatcher;
                _MapSingleThreadDispatcherToNTimesInUse[singleThreadDispatcher] = 0;
            }
#if DEBUG
            _TimerDebug = new Timer(30000);
            _TimerDebug.Elapsed += Debug;
            _TimerDebug.AutoReset = true;
            _TimerDebug.Enabled = true;
            _TimerDebug.Start();
#endif
        }
        protected abstract TSingleThreadDispatcher CreateSingleThreadDispatcher();
        protected abstract THandle CreateSafeCleanupHandle(TSingleThreadDispatcher dispatcher, Action dispose);
        public THandle TakeHandle() {
            TSingleThreadDispatcher handle;
            lock (_LockObject) {
                handle = _MapSingleThreadDispatcherToNTimesInUse.OrderBy(p => p.Value).First().Key;
                _MapSingleThreadDispatcherToNTimesInUse[handle] += 1;
            }
            try
            {
                return CreateSafeCleanupHandle(handle, () =>
                {
                    lock (_LockObject)
                    {
                        _MapSingleThreadDispatcherToNTimesInUse[handle] -= 1;
                    }
                });
            }
            catch
            {
                lock (_LockObject)
                {
                    _MapSingleThreadDispatcherToNTimesInUse[handle] -= 1;
                }
                throw;
            }
        }
        public InfernoTask? TakeTaskElseListAwaken(ContextAssignedSingleThreadDispatcher<TContext> singleThreadDispatcher)
        {
            lock (_LockObject)
            {
                InfernoTask? next = _Waitings.FirstOrDefault();
                if (next != null)
                {
                    _Waitings.RemoveFirst();
                    return next;
                }
                _Awakens.Add(singleThreadDispatcher);
                return null;
            }
        }
        public InfernoTaskNoResultBase UsingContext(Action<TContext> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            InfernoTaskNoResultBase task =
                new InactiveInfernoTaskNoResultArgument<TContext>(callback);
            ContextAssignedSingleThreadDispatcher<TContext>? awaken;
            lock (_LockObject)
            {
                _Waitings.AddLast(task);
                awaken = _Awakens.First();
                if (awaken != null)
                {
                    _Awakens.Remove(awaken);
                }
            }
            awaken?.Awaken();
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
            ContextAssignedSingleThreadDispatcher<TContext>? awaken;
            lock (_LockObject)
            {
                if (_Disposed)
                {
                    throw new ObjectDisposedException(nameof(CudaContextAssignedThreadPool));
                }
                _Waitings.AddLast(task);
                awaken = _Awakens.First();
                if (awaken != null)
                {
                    _Awakens.Remove(awaken);
                }
            }
            awaken?.Awaken();
            return task;
        }
        public void Dispose()
        {
            foreach (var singleThreadDispatcher in _SingleThreadDispatchers)
            {
                singleThreadDispatcher.Dispose();
            }
            InfernoTask[] waitings;
            lock (_LockObject)
            {
                _Disposed = true;
                waitings = _Waitings.ToArray();
                _Waitings.Clear();
            }
#if DEBUG
            _TimerDebug.Dispose();
            _TimerDebug = null;
#endif
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
            ContextAssignedSingleThreadDispatcher<TContext>[] awakens;
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