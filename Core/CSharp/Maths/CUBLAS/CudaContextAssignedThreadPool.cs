
using Core.Threading.ContextAssigned;
using System;

namespace Core.Maths.CUBLAS
{

    public class CudaContextAssignedThreadPool
        : ContextAssignedThreadPoolBaseWithSingleThreadDispatching<CudaContextWithThread, CudaContextHandles, CudaContextWithThreadSafeCleanupHandle>
    {
        public CudaContextAssignedThreadPool(int nContext)
            : base(nContext)
        {
        }

        protected override CudaContextWithThreadSafeCleanupHandle 
            CreateSafeCleanupHandle(CudaContextWithThread dispatcher, 
            Action dispose)
        {
            return new CudaContextWithThreadSafeCleanupHandle(
                dispatcher, dispose);
        }

        protected override CudaContextWithThread CreateSingleThreadDispatcher()
        {
            return new CudaContextWithThread(this);
        }
    }
}