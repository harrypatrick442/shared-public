using ManagedCuda;
using ManagedCuda.BasicTypes;
using System;
using System.Runtime.InteropServices;
namespace Core.Maths.CUBLAS
{
    using System;
    using System.Runtime.InteropServices;

    public static class CuBlasInterop
    {
        private const string CuBlasLibrary = "cublas64_12.dll"; // Ensure that the correct cuBLAS library version is used.

        [DllImport(CuBlasLibrary, EntryPoint = "cublasSetStream_v2", CallingConvention = CallingConvention.Cdecl)]
        public static extern int cublasSetStream_v2(nint handle, nint stream);
        // Create cuBLAS handle
        [DllImport(CuBlasLibrary, EntryPoint = "cublasCreate_v2")]
        public static extern int cublasCreate_v2(ref nint handle);

        // Destroy cuBLAS handle
        [DllImport(CuBlasLibrary, EntryPoint = "cublasDestroy_v2")]
        public static extern int cublasDestroy_v2(nint handle);

        // Perform double-precision matrix multiplication (DGEMM)
        [DllImport(CuBlasLibrary, EntryPoint = "cublasDgemm_v2")]
        public static extern int cublasDgemm_v2(
            IntPtr handle,
            CUBLAS_OP transa,
            CUBLAS_OP transb,
            int m,
            int n,
            int k,
            IntPtr alpha,        // Pointer to alpha
            IntPtr d_A,         // Pointer to matrix A on the device
            int lda,
            IntPtr d_B,         // Pointer to matrix B on the device
            int ldb,
            IntPtr beta,        // Pointer to beta
            IntPtr d_C,         // Pointer to matrix C on the device
            int ldc
        ); 
        [DllImport(CuBlasLibrary, EntryPoint = "cublasDgemv_v2")]
        public static extern int cublasDgemv_v2(
            IntPtr handle,
            CUBLAS_OP trans,
            int m,                    // Number of rows in matrix A
            int n,                    // Number of columns in matrix A
            IntPtr alpha,             // Pointer to alpha
            IntPtr d_A,               // Pointer to matrix A on the device
            int lda,                  // Leading dimension of matrix A
            IntPtr d_x,               // Pointer to vector x on the device
            int incx,                 // Stride between elements of x
            IntPtr beta,              // Pointer to beta
            IntPtr d_y,               // Pointer to vector y on the device
            int incy                  // Stride between elements of y
        ); 
        [DllImport(CuBlasLibrary, EntryPoint = "cublasDaxpy_v2")]
        public static extern int cublasDaxpy_v2(
            nint handle,            // Handle to cuBLAS context
            int n,                  // Number of elements in the vectors
            IntPtr alpha,           // Pointer to the scalar multiplier
            IntPtr x,               // Pointer to the input vector x
            int incx,               // Increment between elements of x
            IntPtr y,               // Pointer to the output vector y
            int incy                // Increment between elements of y
        ); 
        [DllImport(CuBlasLibrary, EntryPoint = "cublasDcopy_v2")]
        public static extern int cublasDcopy_v2(
            nint handle,            // Handle to cuBLAS context
            int n,                  // Number of elements in the vectors
            IntPtr x,               // Pointer to the input vector x
            int incx,               // Increment between elements of x
            IntPtr y,               // Pointer to the output vector y
            int incy                // Increment between elements of y
        ); [DllImport(CuBlasLibrary, EntryPoint = "cublasDscal_v2")]
        public static extern int cublasDscal_v2(
            nint handle,            // Handle to cuBLAS context
            int n,                  // Number of elements in the vector
            IntPtr alpha,           // Pointer to the scalar multiplier
            IntPtr x,               // Pointer to the vector x
            int incx                // Increment between elements of x
        );
    }
}