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

namespace Core.Maths.CUBLAS
{

    public class CudaContextWithThreadSafeCleanupHandle 
        : ContextAssignedSingleThreadDispatcherHandle<CudaContextHandles>,
        ICudaContextWithThread
    {
        public CudaContextWithThreadSafeCleanupHandle(
            ContextAssignedSingleThreadDispatcher<CudaContextHandles> 
            wrapped, Action callbackDisposed) : base(wrapped, callbackDisposed)
        {

        }
    }
}