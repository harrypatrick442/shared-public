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

    public class CudaContextWithThread 
        : ContextAssignedSingleThreadDispatcher<CudaContextHandles>,
        ICudaContextWithThread
    {
        public CudaContextWithThread(INonThreadSpecificTaskSource<CudaContextHandles> pool) : base(pool)
        {
        }

        protected override void OnTheThread(CountdownLatch countdownLatchInitialized)
        {

            using (CudaContext cudaContext = new CudaContext())
            {
                nint cublasHandle = nint.Zero;
                nint cusolverHandle = nint.Zero;
                try
                {
                    int cublasStatus = cublasCreate_v2(ref cublasHandle);
                    CudaStatusHelper.CheckCublasStatus(cublasStatus);
                    int cusolverStatus = CuSolverInterop.cusolverDnCreate(ref cusolverHandle);
                    CudaStatusHelper.CheckCusolverStatus(cusolverStatus);
                    countdownLatchInitialized.Signal();
                    Loop(new CudaContextHandles(cudaContext, cublasHandle, cusolverHandle));
                }
                finally
                {

                    if (cublasHandle != nint.Zero)
                    {
                        cublasDestroy_v2(cublasHandle);
                    }
                    if (cusolverHandle != nint.Zero)
                    {
                        CuSolverInterop.cusolverDnDestroy(cusolverHandle);
                    }
                }
            }
        }
    }
}