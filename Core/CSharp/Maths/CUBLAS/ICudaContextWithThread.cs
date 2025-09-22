using System;
using System.Collections.Generic;
using Logging;
using System.Linq;
using InfernoDispatcher.Tasks;
using System.Threading;
using ManagedCuda;
using ManagedCuda.CudaBlas;
using ManagedCuda.CudaSolve;
using static Core.Maths.CUBLAS.CuBlasInterop;
using Core.Threading.ContextAssigned;
using ManagedCuda.BasicTypes;

namespace Core.Maths.CUBLAS
{

    public interface ICudaContextWithThread
    {
        public int ThreadId { get; }
        public InfernoTaskNoResultBase UsingContext(Action<CudaContextHandles> callback);
        public InfernoTaskWithResultBase<TResult> UsingContext<TResult>(
            Func<CudaContextHandles, TResult> callback);
    }
}