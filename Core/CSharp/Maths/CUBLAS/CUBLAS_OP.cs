using ManagedCuda;
using ManagedCuda.BasicTypes;
using System;
using System.Runtime.InteropServices;
namespace Core.Maths.CUBLAS
{
    public enum CUBLAS_OP
    {
        N = 0,  // No transpose
        T = 1,  // Transpose
        C = 2   // Conjugate transpose
    }
}