using ManagedCuda.BasicTypes;
using ManagedCuda;
using System;
using System.Runtime.InteropServices;
using Logging;

namespace Core.Maths.CUBLAS
{

    public class CudaContextHandles
    {
        public CudaContext CudaContext { get; }
        public nint CublasHandle { get; }
        public nint CusolverHandle { get; }
        public CudaContextHandles(CudaContext cudaContext, nint cublasHandle, nint cusolverHandle)
        {
            CudaContext = cudaContext;
            CublasHandle = cublasHandle;
            CusolverHandle = cusolverHandle;
        }
        public CUdeviceptr AllocateMemory(long size)
        {
            return CudaContext.AllocateMemory(size);
        }
        public void FreeMemory(CUdeviceptr ptr)
        {
            CudaContext.FreeMemory(ptr);
        }
        public void SetStream(CudaStream stream)
        {
            // Use the native CUDA stream handle provided by ManagedCuda
            nint streamHandle = (nint)stream.Stream.Pointer; // Directly cast Stream to nint
            var status = CuBlasInterop.cublasSetStream_v2(CublasHandle, streamHandle);
            if (status != 0)
            {
                throw new Exception($"cuBLAS SetStream failed with error code {status}");
            }
        }
        public void IfDebugSyncAndThrowLatestError() { 
            
        }
        public void ThrowLastError()
        {
            int error = CudartInterop.cudaGetLastError();

            if (error == (int)CudaError.cudaSuccess)
            {
                return;
            }
            string? errorString = null;
            try
            {
                IntPtr ptrErrorString = CudartInterop.cudaGetErrorString(error);
                if (ptrErrorString != IntPtr.Zero)
                {
                    errorString = Marshal.PtrToStringAnsi(ptrErrorString) ?? errorString;
                }
            }
            catch (Exception ex)
            {
                Logs.Default.Error($"NormalizedError while retrieving CUDA error string: {ex}");
            }
            throw new Exception($"CUDA NormalizedError: \"{((CudaError)error).GetName()}\" with error string: \"{errorString}\"");
        }

    }
}