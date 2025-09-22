using ManagedCuda;
using ManagedCuda.BasicTypes;
using System;
using System.Runtime.InteropServices;
namespace Core.Maths.CUBLAS
{
    using System;
    using System.Runtime.InteropServices;

    public static class CudartInterop
    {
        private const string Library = "cudart64_12.dll"; // Ensure that the correct cuBLAS library version is used.


        [DllImport(Library, EntryPoint = "cudaStreamQuery",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int cudaStreamQuery(IntPtr stream); 
        [DllImport(Library, EntryPoint = "cudaGetLastError")]
        public static extern int cudaGetLastError();

        [DllImport(Library, EntryPoint = "cudaGetErrorString")]
        public static extern IntPtr cudaGetErrorString(int error);
    }
}