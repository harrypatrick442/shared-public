using Core.Maths.CUBLAS;
using InfernoDispatcher.Locking;
using ManagedCuda;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Core.Maths
{
    public class GPUMathsParameters : IDisposable
    {
        private const long MIN_N_OPERATIONS_TO_USE_GPU_FOR_MULTIPLICATION_DEFUALT = 48000000,
            MIN_N_VALUES_IN_MATRIX_TO_USE_GPU_FOR_INVERSION = 10000;
        public double MaxProportionMemoryCanUse { get; }
        public InfernoFiniteResourceSemaphore? MemoryAllocationLock { get; }
        public long MinNOperationsToUseGpuForMultiplication { get; }
        public long MinNValuesInMatrixToUseGpuForInversion { get; }

        public CudaContextAssignedThreadPool CudaContextAssignedThreadPool { get; }
        public GPUMathsParameters(
            CudaContextAssignedThreadPool cudaContextAssignedThreadPool,
            double maxProportionMemoryCanUse, 
            InfernoFiniteResourceSemaphore? memoryAllocationLock,
            long minNOperationsToUseGpuForMultiplication = MIN_N_OPERATIONS_TO_USE_GPU_FOR_MULTIPLICATION_DEFUALT,
            long minNValuesInMatrixToUseGpuForInversion = MIN_N_VALUES_IN_MATRIX_TO_USE_GPU_FOR_INVERSION,
            bool disposeChildren = true)
        {
            CudaContextAssignedThreadPool = cudaContextAssignedThreadPool;
            MaxProportionMemoryCanUse = maxProportionMemoryCanUse;
            MemoryAllocationLock = memoryAllocationLock;
            MinNOperationsToUseGpuForMultiplication = minNOperationsToUseGpuForMultiplication;
            MinNValuesInMatrixToUseGpuForInversion = minNValuesInMatrixToUseGpuForInversion;
        }
        public void Dispose()
        {
            CudaContextAssignedThreadPool?.Dispose();
        }
    }
}
