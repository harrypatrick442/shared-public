using Core.Maths.CUBLAS;
using Core.Maths.Tensors;
using ManagedCuda;
using System;
using System.ComponentModel;

namespace Core.Maths.Vectors
{
    public static partial class VectorHelper
    {
        public static void SubtractWithGPU(
    nint cublasHandle,
    CudaDeviceVariable<double> vectorA,
    CudaDeviceVariable<double> vectorB,
    CudaDeviceVariable<double> resultVector,
    IntPtr minusOneIntPtr,
    int length)
        {
            if (vectorA.Size != vectorB.Size || vectorA.Size != resultVector.Size)
            {
                throw new ArgumentException("All vectors must have the same length.");
            }
            SubtractWithGPUNoChecks(cublasHandle, vectorA, vectorB, resultVector, minusOneIntPtr, length);
        }
        public static void SubtractWithGPUNoChecks(
    nint cublasHandle,
    CudaDeviceVariable<double> vectorA,
    CudaDeviceVariable<double> vectorB,
    CudaDeviceVariable<double> resultVector,
    IntPtr minusOneIntPtr,
    int length)
        {
            // Step 1: Copy vectorA into resultVector
            int status = CuBlasInterop.cublasDcopy_v2(
                cublasHandle,
                length,                              // Number of elements
                vectorA.DevicePointer.Pointer,       // Pointer to vectorA
                1,                                    // Stride for vectorA
                resultVector.DevicePointer.Pointer,   // Pointer to resultVector
                1                                     // Stride for resultVector
            );
            CudaStatusHelper.CheckCublasStatus(status);

            // Step 2: Perform vector subtraction using cuBLAS AXPY
            status = CuBlasInterop.cublasDaxpy_v2(
                cublasHandle,
                length,                              // Number of elements
                minusOneIntPtr,                     // Scalar multiplier (-1.0)
                vectorB.DevicePointer.Pointer,      // Pointer to vectorB (x)
                1,                                   // Stride for vectorB
                resultVector.DevicePointer.Pointer,  // Pointer to resultVector (y)
                1                                    // Stride for resultVector
            );
            CudaStatusHelper.CheckCublasStatus(status);
        }
        public static void AdditionToFirstVariableWithGPU(
    nint cublasHandle,
    CudaDeviceVariable<double> vectorA,
    CudaDeviceVariable<double> vectorB,
    IntPtr oneIntPtr,
    int length)
        {
            // Perform vector addition directly using cuBLAS AXPY
            // AXPY computes: vectorA = vectorA + scalar * vectorB
            int status = CuBlasInterop.cublasDaxpy_v2(
                cublasHandle,
                length,                              // Number of elements
                oneIntPtr,                          // Scalar multiplier (1.0)
                vectorB.DevicePointer.Pointer,      // Pointer to vectorB (x)
                1,                                   // Stride for vectorB
                vectorA.DevicePointer.Pointer,      // Pointer to vectorA (y) - updated directly
                1                                    // Stride for vectorA
            );

            // Check the cuBLAS operation status
            CudaStatusHelper.CheckCublasStatus(status);
        }

        public static void ScaleVectorWithGPU(
    nint cublasHandle,
    CudaDeviceVariable<double> vector,
    IntPtr alphaPtr,
    int length)
        {
            ScaleVectorWithGPUNoChecks(cublasHandle, vector, alphaPtr, length);
        }
        public static void ScaleVectorWithGPUNoChecks(
nint cublasHandle,
CudaDeviceVariable<double> vector,
IntPtr alphaPtr,
int length)
        {
            // Ensure the vector's size matches the expected length
            if (vector.Size != length * sizeof(double))
            {
                throw new ArgumentException("Vector length does not match the expected size.");
            }

            // Scale the vector using cuBLAS DSCAL
            int status = CuBlasInterop.cublasDscal_v2(
                cublasHandle,
                length,                         // Number of elements
                alphaPtr,                       // Pointer to the scalar multiplier
                vector.DevicePointer.Pointer,   // Pointer to vector x
                1                               // Stride for x
            );

            // Check cuBLAS status for errors
            CudaStatusHelper.CheckCublasStatus(status);
        }
        public static void ScaleVectorToAnotherWithGPU(
    nint cublasHandle,
    CudaDeviceVariable<double> sourceVector,
    CudaDeviceVariable<double> destinationVector,
    IntPtr alphaPtr,
    int length)
        {
            ScaleVectorToAnotherWithGPUNoChecks(cublasHandle, sourceVector, destinationVector, alphaPtr, length);
        }
        public static void ScaleVectorToAnotherWithGPUNoChecks(
    nint cublasHandle,
    CudaDeviceVariable<double> sourceVector,
    CudaDeviceVariable<double> destinationVector,
    IntPtr alphaPtr,
    int length)
        {
            // Ensure both vectors have the same size
            if (sourceVector.Size != length)
            {
                throw new ArgumentException("Source and destination vectors must have the same length.");
            }

            // Step 1: Copy the source vector to the destination vector
            int status = CuBlasInterop.cublasDcopy_v2(
                cublasHandle,
                length,                              // Number of elements
                sourceVector.DevicePointer.Pointer,  // Pointer to source vector
                1,                                   // Stride for source vector
                destinationVector.DevicePointer.Pointer, // Pointer to destination vector
                1                                    // Stride for destination vector
            );
            CudaStatusHelper.CheckCublasStatus(status);

            // Step 2: Scale the destination vector
            status = CuBlasInterop.cublasDscal_v2(
                cublasHandle,
                length,                              // Number of elements
                alphaPtr,                            // Pointer to the scalar multiplier
                destinationVector.DevicePointer.Pointer, // Pointer to destination vector
                1                                    // Stride for destination vector
            );
            CudaStatusHelper.CheckCublasStatus(status);
        }

    }
}