using ManagedCuda;
using ManagedCuda.BasicTypes;
using System;
using System.Runtime.InteropServices;
using static Core.Maths.CUBLAS.CuBlasInterop;
namespace Core.Maths.CUBLAS
{

    class CuSolverInterop
    {
        [DllImport("cusolver64_11.dll", EntryPoint = "cusolverDnCreate")]
        public static extern int cusolverDnCreate(ref nint handle);

        [DllImport("cusolver64_11.dll", EntryPoint = "cusolverDnDestroy")]
        public static extern int cusolverDnDestroy(nint handle);

        [DllImport("cusolver64_11.dll", EntryPoint = "cusolverDnDgetrf")]
        public static extern int cusolverDnDgetrf(
            nint handle,
            int m,
            int n,
            nint d_A,
            int lda,
            nint d_work,
            nint d_pivotArray,
            nint d_info
        );
        [DllImport("cusolver64_11.dll", EntryPoint = "cusolverDnDgetrs")]
        public static extern int cusolverDnDgetrs(
            nint handle,            // cuSOLVER handle
            CUBLAS_OP trans,          // CUBLAS_OP enum for matrix operation ('N', 'T', 'C')
            int n,                    // Number of rows of matrix A
            int nrhs,                 // Number of right-hand sides (number of columns in B)
            nint A,                 // Pointer to LU-decomposed matrix A
            int lda,                  // Leading dimension of A
            nint devIpiv,           // Pointer to pivot array from getrf
            nint B,                 // Pointer to matrix B (right-hand side)
            int ldb,                  // Leading dimension of B
            nint devInfo            // Pointer to status info
        );


        [DllImport("cusolver64_11.dll", EntryPoint = "cusolverDnDgetrf_bufferSize")]
        public static extern int cusolverDnDgetrf_bufferSize(
            nint handle,
            int m,
            int n,
            nint d_A,
            int lda,
            ref int lwork);
        [DllImport("cusolver64_11.dll", EntryPoint = "cusolverDnDgetri")]
        public static extern int cusolverDnDgetri(
            nint handle,
            int n,
            nint d_A,
            int lda,
            nint d_pivotArray,
            nint d_work,
            int lwork,
            nint d_info
        );
    }
}