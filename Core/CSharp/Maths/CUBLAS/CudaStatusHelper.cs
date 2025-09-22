using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.CudaBlas;
using ManagedCuda.CudaSolve;
using System;
using System.Runtime.InteropServices;
using static Core.Maths.CUBLAS.CuBlasInterop;
namespace Core.Maths.CUBLAS
{

    public static class CudaStatusHelper
    {
        public static void CheckCublasStatus(int status)
        {

            if (Enum.IsDefined(typeof(CublasStatus), status))
            {
                CublasStatus cublasStatus = (CublasStatus)status;
                if (cublasStatus == CublasStatus.Success)
                {
                    return;
                }
                throw new Exception($"Failed to create handle with {nameof(CublasStatus)} {Enum.GetName(typeof(CublasStatus), cublasStatus)}.");
            }
            throw new Exception($"Failed to create handle with with unknown {nameof(CublasStatus)} {status}.");
        }
        public static void CheckCusolverStatus(int status)
        {

            if (Enum.IsDefined(typeof(cusolverStatus), status))
            {
                cusolverStatus cusolverStatus = (cusolverStatus)status;
                if (cusolverStatus == cusolverStatus.Success)
                {
                    return;
                }
                throw new Exception($"Failed to create handle with {nameof(cusolverStatus)} {Enum.GetName(typeof(cusolverStatus), cusolverStatus)}.");
            }
            throw new Exception($"Failed to create handle with with unknown {nameof(cusolverStatus)} {status}.");
        }
    }
}