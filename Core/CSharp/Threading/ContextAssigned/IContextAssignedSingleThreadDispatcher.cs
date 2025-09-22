using InfernoDispatcher.Tasks;
using System;

namespace Core.Threading.ContextAssigned
{
    public interface IContextAssignedSingleThreadDispatcher<THandle>
    {
        int ThreadId { get; }

        void Dispose();
        InfernoTaskNoResultBase UsingContext(Action<THandle> callback);
        InfernoTaskWithResultBase<TResult> UsingContext<TResult>(Func<THandle, TResult> callback);
    }
}