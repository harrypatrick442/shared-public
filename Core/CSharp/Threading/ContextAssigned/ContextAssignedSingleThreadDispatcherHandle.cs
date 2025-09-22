using InfernoDispatcher.Tasks;
using System;

namespace Core.Threading.ContextAssigned
{
    public abstract class ContextAssignedSingleThreadDispatcherHandle<THandle> :
        IContextAssignedSingleThreadDispatcher<THandle>
    {
        private ContextAssignedSingleThreadDispatcher<THandle> _Wrapped;
        private Action _CallbackDisposed;

        public int ThreadId => _Wrapped.ThreadId;
        public ContextAssignedSingleThreadDispatcherHandle(
            ContextAssignedSingleThreadDispatcher<THandle> wrapped,
            Action callbackDisposed
        )
        {
            _Wrapped = wrapped;
            _CallbackDisposed = callbackDisposed;
        }
        ~ContextAssignedSingleThreadDispatcherHandle() {
            Dispose();
        }
        public void Dispose()
        {
            _CallbackDisposed();
        }

        public InfernoTaskNoResultBase UsingContext(Action<THandle> callback)
        {
            return _Wrapped.UsingContext(callback);
        }

        public InfernoTaskWithResultBase<TResult> UsingContext<TResult>(Func<THandle, TResult> callback)
        {
            return _Wrapped.UsingContext(callback);
        }
    }
}