using Core.Cleanup;
using Core.FileSystem;
using Core.Maths.CUBLAS;
using Core.Timing;
using InfernoDispatcher.Tasks;
using Logging;
using ManagedCuda;
using ManagedCuda.CudaBlas;
using ManagedCuda.CudaSolve;
using ManagedCuda.VectorTypes;
using System;
using System.Runtime.InteropServices;
using System.Threading;
namespace Core.Maths
{
    public partial class MatrixHelper
    {
        private const string CHECK_INFO_KERNEL_CU =
@"extern ""C"" __global__ void checkInfoKernel(int* info, int* status)
{
    if (*info != 0){
        *status = *info; // Set the status if an error is detected
    }
}";
        public static InfernoTaskWithResultBase<double[]> MatrixMultiplyWithGPUColumnMajor(
    double[] matrixA, double[] matrixB, int rowsA, int colsA, int colsB, 
    CudaContextAssignedThreadPool threadPool)
        {
            // Validate input dimensions
            if (matrixA.Length != rowsA * colsA)
                throw new ArgumentException($"Matrix A dimensions do not match the specified rowsA: {rowsA} and colsA: {colsA}");

            if (matrixB.Length != colsA * colsB)
                throw new ArgumentException($"Matrix B dimensions do not match the specified colsA: {colsA} and colsB: {colsB}");

            // Check matrix dimensions: colsA must match rowsB (since B is already in column-major order)
            int rowsB = colsA;
            int rowsC = rowsA;
            int colsC = colsB;

            // Allocate memory for the result matrix (column-major order)
            double[] resultMatrix = new double[rowsC * colsC];

            Action<int, string> checkStatus = Get_CheckCublasStatus_ForMultiply(rowsA, colsA, colsB);

            return threadPool.UsingContext((cudaHandles) => {
                long startTime = TimeHelper.MillisecondsNow;
                CudaDeviceVariable<double> matrixAVariable = null;
                CudaDeviceVariable<double> matrixBVariable = null;
                CudaDeviceVariable<double> matrixCVariable = null;
                double alpha = 1.0;
                double beta = 0.0;
                IntPtr alphaPtr = Marshal.AllocHGlobal(sizeof(double));
                IntPtr betaPtr = Marshal.AllocHGlobal(sizeof(double));
                try
                {
                    matrixAVariable = new CudaDeviceVariable<double>(rowsA * colsA);
                    matrixBVariable = new CudaDeviceVariable<double>(rowsB * colsB);
                    matrixCVariable = new CudaDeviceVariable<double>(rowsC * colsC);

                    // Step 3: Copy A and B matrices from host (CPU) to device (GPU)
                    matrixAVariable.CopyToDevice(matrixA);
                    matrixBVariable.CopyToDevice(matrixB);
                    matrixA = null;
                    matrixB = null;
                    // Step 4: Perform matrix multiplication using cuBLAS

                    Marshal.Copy(new[] { alpha }, 0, alphaPtr, 1);
                    Marshal.Copy(new[] { beta }, 0, betaPtr, 1);
                    int status = CuBlasInterop.cublasDgemm_v2(
                        cudaHandles.CublasHandle,
                        CUBLAS_OP.N,
                        CUBLAS_OP.N,
                        rowsA,
                        colsB,
                        colsA,
                        alphaPtr,    // Pass pointer to alpha
                        matrixAVariable.DevicePointer.Pointer,
                        rowsA,       // Leading dimension for A
                        matrixBVariable.DevicePointer.Pointer,
                        colsA,       // Leading dimension for B should be colsA
                        betaPtr,     // Pass pointer to beta
                        matrixCVariable.DevicePointer.Pointer,
                        rowsC        // Leading dimension for C
                    );
                    checkStatus(status, "Matrix multiplication failed");
                    // Step 5: Copy the result matrix C back to the host
                    matrixCVariable.CopyToHost(resultMatrix);
                    return resultMatrix;
                }
                catch(Exception ex)
                {
                    throw;
                }
                finally
                {
                    Marshal.FreeHGlobal(alphaPtr);
                    Marshal.FreeHGlobal(betaPtr);
                    // Step 6: Cleanup memory and cuBLAS context
                    matrixAVariable?.Dispose();
                    matrixBVariable?.Dispose();
                    matrixCVariable?.Dispose();
                }
            });
        }
        public static InfernoTaskWithResultBase<double[]> MatrixMultiplyByVectorWithGPUColumnMajor(
    double[] matrix, double[] vector, int rows, int cols,
    CudaContextAssignedThreadPool threadPool)
        {
            // Validate input dimensions
            if (matrix.Length != rows * cols)
                throw new ArgumentException($"Matrix dimensions do not match the specified rows: {rows} and cols: {cols}");

            if (vector.Length != cols)
                throw new ArgumentException($"Vector length does not match the number of columns in the matrix: {cols}");

            // Allocate memory for the result vector (column-major order)
            double[] resultVector = new double[rows];

            // Lambda function for status checking
            Action<int, string> checkStatus = Get_CheckCublasStatus_ForMultiply(rows, cols, 1);

            return threadPool.UsingContext((cudaHandles) =>
            {
                CudaDeviceVariable<double> matrixVariable = null;
                CudaDeviceVariable<double> vectorVariable = null;
                CudaDeviceVariable<double> resultVariable = null;

                // Scalars for cuBLAS
                double alpha = 1.0;
                double beta = 0.0;
                IntPtr alphaPtr = Marshal.AllocHGlobal(sizeof(double));
                IntPtr betaPtr = Marshal.AllocHGlobal(sizeof(double));

                try
                {
                    // Allocate GPU memory
                    matrixVariable = new CudaDeviceVariable<double>(rows * cols);
                    vectorVariable = new CudaDeviceVariable<double>(cols);
                    resultVariable = new CudaDeviceVariable<double>(rows);

                    // Copy data to GPU
                    matrixVariable.CopyToDevice(matrix);
                    vectorVariable.CopyToDevice(vector);

                    // Prepare alpha and beta values
                    Marshal.Copy(new[] { alpha }, 0, alphaPtr, 1);
                    Marshal.Copy(new[] { beta }, 0, betaPtr, 1);

                    // Perform matrix-vector multiplication using cuBLAS
                    int status = CuBlasInterop.cublasDgemv_v2(
                        cudaHandles.CublasHandle,
                        CUBLAS_OP.N,          // Matrix is not transposed
                        rows,                 // Number of rows in matrix
                        cols,                 // Number of columns in matrix
                        alphaPtr,             // Pointer to alpha
                        matrixVariable.DevicePointer.Pointer,
                        rows,                 // Leading dimension of the matrix
                        vectorVariable.DevicePointer.Pointer,
                        1,                    // Stride of the vector
                        betaPtr,              // Pointer to beta
                        resultVariable.DevicePointer.Pointer,
                        1                     // Stride of the result vector
                    );

                    // Check cuBLAS status
                    checkStatus(status, "Matrix-vector multiplication failed");

                    // Copy result back to host
                    resultVariable.CopyToHost(resultVector);

                    return resultVector;
                }
                finally
                {
                    // Free resources
                    Marshal.FreeHGlobal(alphaPtr);
                    Marshal.FreeHGlobal(betaPtr);
                    matrixVariable?.Dispose();
                    vectorVariable?.Dispose();
                    resultVariable?.Dispose();
                }
            });
        }
        public static void MatrixMultiplyByVectorWithGPUColumnMajor(
            nint cublasHandle,
            CudaDeviceVariable<double> matrixVariable,
            CudaDeviceVariable<double> vectorVariable,
            CudaDeviceVariable<double> resultVariable,
            int nRows,
            int nColumns,
            IntPtr alphaPtr,
            IntPtr betaPtr)
        {
            int status = CuBlasInterop.cublasDgemv_v2(
                cublasHandle,
                CUBLAS_OP.N,          // Matrix is not transposed
                nRows,                 // Number of rows in matrix
                nColumns,                 // Number of columns in matrix
                alphaPtr,             // Pointer to alpha
                matrixVariable.DevicePointer.Pointer,
                nRows,                 // Leading dimension of the matrix
                vectorVariable.DevicePointer.Pointer,
                1,                    // Stride of the vector
                betaPtr,              // Pointer to beta
                resultVariable.DevicePointer.Pointer,
                1                     // Stride of the result vector
            );
            CudaStatusHelper.CheckCublasStatus(status);
        }

        public static long EstimateGPUOrCPUMemoryForMatrixMultiplyWithGPUColumnMajor(
            long rowsA, long colsA, long colsB, bool requiresReadWriteReorganisation)
        {

            // Memory for matrix A (Host and GPU)
            long matrixAMemory = rowsA * colsA * sizeof(double);

            // Memory for matrix B (Host and GPU)
            long matrixBMemory = colsA * colsB * sizeof(double);

            // Memory for result matrix C (Host and GPU)
            long matrixCMemory = rowsA * colsB * sizeof(double);

            // Total memory for Host (matrix A, matrix B, matrix C)
            long totalHostMemory = matrixAMemory + matrixBMemory + matrixCMemory;

            // Total memory for GPU (matrix A, matrix B, matrix C)
            long totalGPUMemory = matrixAMemory + matrixBMemory + matrixCMemory;

            // Total memory used on both Host and GPU
            long totalMemoryUsed = totalHostMemory + totalGPUMemory;

            // Estimate for temporary buffers on GPU (10% overhead)
            long temporaryBufferMemory = (long)(totalGPUMemory * 0.1);

            // Final memory estimate
            long totalMemoryWithOverhead = totalMemoryUsed + temporaryBufferMemory;
            if (requiresReadWriteReorganisation)
                totalMemoryWithOverhead += 2L*sizeof(double) * colsA;//used in writing to disk to reorganise from column major

            return totalMemoryWithOverhead;
        }

        public static InfernoTaskWithResultBase<double[]> MatrixInverseWithGPUColumnMajor(
            double[] matrix, int size, CudaContextAssignedThreadPool threadPool,
            CancellationToken? cancellationToken = null)
        {
            if (size * size != matrix.Length)
                throw new Exception("Matrix not square");


            Action<int, string> checkStatus = Get_CheckStatus_ForInversion(size);
            return threadPool.UsingContext(cudaHandles =>
            {
                CudaDeviceVariable<double> matrixVariable = null;
                CudaDeviceVariable<int> pivotArrayVariable = null;
                CudaDeviceVariable<int> infoArrayVariable = null;
                CudaDeviceVariable<double> workspaceVariable = null;
                try
                {

                    // Step 3: Allocate device memory for the matrix, pivot array, and info array
                    matrixVariable = new CudaDeviceVariable<double>(size * size);
                    pivotArrayVariable = new CudaDeviceVariable<int>(size);
                    infoArrayVariable = new CudaDeviceVariable<int>(1);

                    // Copy the transposed matrix data to the GPU
                    matrixVariable.CopyToDevice(matrix);
                    matrix = null;

                    // Step 4: Determine the workspace size needed for LU decomposition
                    int workspaceSizeNeeded = 0;
                    int status = CuSolverInterop.cusolverDnDgetrf_bufferSize(
                        cudaHandles.CusolverHandle,
                        size, size,
                        matrixVariable.DevicePointer.Pointer,
                        size,
                        ref workspaceSizeNeeded
                    );
                    checkStatus(status, "Failed to determine LU decomposition workspace size");

                    // Allocate workspace
                    workspaceVariable = new CudaDeviceVariable<double>(workspaceSizeNeeded);

                    // Step 5: Perform LU decomposition
                    status = CuSolverInterop.cusolverDnDgetrf(
                        cudaHandles.CusolverHandle,
                        size, size,
                        matrixVariable.DevicePointer.Pointer,
                        size,
                        workspaceVariable.DevicePointer.Pointer,
                        pivotArrayVariable.DevicePointer.Pointer,
                        infoArrayVariable.DevicePointer.Pointer
                    );
                    checkStatus(status, "LU decomposition failed");

                    // Check if LU decomposition was successful
                    int infoHost = 0;
                    infoArrayVariable.CopyToHost(ref infoHost);
                    if (infoHost != 0)
                    {
                        throw new Exception($"LU decomposition failed with info = {infoHost}");
                    }

                    // Step 6: Solve the system A * X = I (where I is the identity matrix) for each column
                    double[] identityMatrix = new double[size * size];
                    for (int i = 0; i < size; i++)
                    {
                        identityMatrix[i * size + i] = 1.0;
                    }

                    // Allocate memory for the identity matrix on the GPU
                    CudaDeviceVariable<double> identityMatrixVaraible = new CudaDeviceVariable<double>(identityMatrix.Length);
                    identityMatrixVaraible.CopyToDevice(identityMatrix);

                    // Step 7: Solve A * X = I for each column (X is now the inverse matrix)
                    status = CuSolverInterop.cusolverDnDgetrs(
                        cudaHandles.CusolverHandle,
                        CUBLAS_OP.N,  // 'N' means no transpose
                        size, size,
                        matrixVariable.DevicePointer.Pointer,
                        size,
                        pivotArrayVariable.DevicePointer.Pointer,
                        identityMatrixVaraible.DevicePointer.Pointer,
                        size,
                        infoArrayVariable.DevicePointer.Pointer
                    );
                    checkStatus(status, "Matrix inversion failed during solve");

                    // Check if the solve was successful
                    infoArrayVariable.CopyToHost(ref infoHost);
                    if (infoHost != 0)
                    {
                        throw new Exception($"Matrix inversion failed with info = {infoHost}");
                    }

                    // Step 8: Copy the inverted matrix (which is now in d_identityMatrix) back to the host
                    double[] inverse = new double[size * size];
                    identityMatrixVaraible.CopyToHost(inverse);

                    return inverse;
                }
                finally
                {
                    // Step 9: Cleanup
                    matrixVariable?.Dispose();
                    pivotArrayVariable?.Dispose();
                    infoArrayVariable?.Dispose();
                    workspaceVariable?.Dispose();
                }
            });
        }
        public static InvertWithGPUAsyncHandle Create_InvertMatrixWithGPUColumnMajorAsync(
    int size,
    CudaContextHandles cudaHandles,
    CudaDeviceVariable<double> matrixVariable,
    CudaDeviceVariable<double> resultMatrixVariable,
    CudaStream stream,
    Action? ifDebugSyncAndThrowLatestError)
        {
            CudaKernel kernelCheckInfo;
            Action? unloadModule = null;
            using (TemporaryFile temporaryFilePtx = NvidiaCompiler
                .CompileRawCuStringToPtxUsingNVRTC(CHECK_INFO_KERNEL_CU))
            {
                var module = cudaHandles.CudaContext.LoadModule(temporaryFilePtx.FilePath);
                unloadModule = () => cudaHandles.CudaContext.UnloadModule(module);
                kernelCheckInfo = new CudaKernel("checkInfoKernel", module, cudaHandles.CudaContext);
            }
            kernelCheckInfo.BlockDimensions = new dim3(1, 1, 1);
            kernelCheckInfo.GridDimensions = new dim3(1, 1, 1);
            Action<int, string> checkStatus = Get_CheckStatus_ForInversion(size);


            // Precompute workspace size
            int workspaceSizeNeeded = 0;
            int status = CuSolverInterop.cusolverDnDgetrf_bufferSize(
                cudaHandles.CusolverHandle,
                size, size,
                matrixVariable.DevicePointer.Pointer,
                size,
                ref workspaceSizeNeeded
            ); Console.WriteLine($"");

            var checkWorkspaceStatus = ()=>
                checkStatus(status, $"Failed to determine LU decomposition workspace size. Workspace size required: {workspaceSizeNeeded}");
            checkWorkspaceStatus();
            stream.Synchronize();
            checkWorkspaceStatus();
            // Allocate reusable GPU memory
            CudaDeviceVariable<double>? workspaceVariable = null;
            CudaDeviceVariable<int>? pivotArrayVariable = null;
            CudaDeviceVariable<double>? matrixCopy = null;
            CudaDeviceVariable<int>? infoArrayVariable = null;
            CudaDeviceVariable<int>? decompositionInfoVariable = null;
            CudaDeviceVariable<int>? solveInfoVariable = null;
            CudaDeviceVariable<double>? identityMatrixDevice = null;

            try
            {
                workspaceVariable = new CudaDeviceVariable<double>(workspaceSizeNeeded);
                pivotArrayVariable = new CudaDeviceVariable<int>(size);
                matrixCopy = new CudaDeviceVariable<double>(matrixVariable.Size);
                infoArrayVariable = new CudaDeviceVariable<int>(1);
                decompositionInfoVariable = new CudaDeviceVariable<int>(1);
                solveInfoVariable = new CudaDeviceVariable<int>(1);
                int[] initialInfo = { 0 };
                infoArrayVariable.CopyToDevice(initialInfo);
                decompositionInfoVariable.CopyToDevice(initialInfo);
                solveInfoVariable.CopyToDevice(initialInfo);
                identityMatrixDevice = PrecomputeIdentityMatrix(size);

                return new InvertWithGPUAsyncHandle(
                    invert: () =>
                    {
                        // Reset matrixCopy to original matrix
                        matrixCopy.AsyncCopyToDevice(matrixVariable, stream);
                        ifDebugSyncAndThrowLatestError?.Invoke();
                        // Reset resultMatrixVariable to identity matrix
                        resultMatrixVariable.AsyncCopyToDevice(identityMatrixDevice, stream);

                        ifDebugSyncAndThrowLatestError?.Invoke();

                        // Perform LU decomposition
                        status = CuSolverInterop.cusolverDnDgetrf(
                            cudaHandles.CusolverHandle,
                            size, size,
                            matrixCopy.DevicePointer.Pointer,
                            size,
                            workspaceVariable.DevicePointer.Pointer,
                            pivotArrayVariable.DevicePointer.Pointer,
                            infoArrayVariable.DevicePointer.Pointer
                        );
                        ifDebugSyncAndThrowLatestError?.Invoke();
                        checkStatus(status, "Failed to set up LU decomposition (cusolverDnDgetrf)");

                        ifDebugSyncAndThrowLatestError?.Invoke();
                        // Check decomposition info
                        kernelCheckInfo.RunAsync(stream.Stream, infoArrayVariable.DevicePointer, decompositionInfoVariable.DevicePointer);

                        ifDebugSyncAndThrowLatestError?.Invoke();
                        // Solve A * X = I
                        status = CuSolverInterop.cusolverDnDgetrs(
                            cudaHandles.CusolverHandle,
                            CUBLAS_OP.N,
                            size, size,
                            matrixCopy.DevicePointer.Pointer,
                            size,
                            pivotArrayVariable.DevicePointer.Pointer,
                            resultMatrixVariable.DevicePointer.Pointer,
                            size,
                            infoArrayVariable.DevicePointer.Pointer
                        );

                        checkStatus(status, "Failed to set up matrix inversion solve (cusolverDnDgetrs)");

                        ifDebugSyncAndThrowLatestError?.Invoke();
                        kernelCheckInfo.RunAsync(stream.Stream, infoArrayVariable.DevicePointer, solveInfoVariable.DevicePointer);

                        ifDebugSyncAndThrowLatestError?.Invoke();
                    },
                    synchronize: () =>
                    {
                        ifDebugSyncAndThrowLatestError?.Invoke();
                        stream.Synchronize();
                        int infoHost = 0;
                        decompositionInfoVariable.CopyToHost(ref infoHost);
                        if (infoHost != 0)
                        {
                            throw new Exception($"LU decomposition failed with info = {infoHost}");
                        }
                        solveInfoVariable.CopyToHost(ref infoHost);
                        if (infoHost != 0)
                        {
                            throw new Exception($"Matrix inversion failed with info = {infoHost}");
                        }
                    },
                    dispose: () =>
                    {
                        // Dispose shared resources
                        matrixCopy.Dispose();
                        workspaceVariable.Dispose();
                        pivotArrayVariable.Dispose();
                        infoArrayVariable.Dispose();
                        identityMatrixDevice.Dispose();
                        decompositionInfoVariable.Dispose();
                        solveInfoVariable.Dispose();
                    }
                );
            }
            catch
            {
                matrixCopy?.Dispose();
                workspaceVariable?.Dispose();
                pivotArrayVariable?.Dispose();
                infoArrayVariable?.Dispose();
                identityMatrixDevice?.Dispose();
                decompositionInfoVariable?.Dispose();
                solveInfoVariable?.Dispose();
                try
                {
                    unloadModule?.Invoke();
                }
                catch(Exception exUnloadModule) { Logs.Default.Error(exUnloadModule); }
                throw;
            }
        }
        public static InvertWithGPUHandle Create_InvertMatrixWithGPUColumnMajor(
    int size,
    CudaContextHandles cudaHandles,
    CudaDeviceVariable<double> matrixVariable,
    CudaDeviceVariable<double> resultMatrixVariable)
        {
            Action<int, string> checkStatus = Get_CheckStatus_ForInversion(size);

            // Precompute workspace size
            int workspaceSizeNeeded = 0;
            int status = CuSolverInterop.cusolverDnDgetrf_bufferSize(
                cudaHandles.CusolverHandle,
                size, size,
                matrixVariable.DevicePointer.Pointer,
                size,
                ref workspaceSizeNeeded
            );
            checkStatus(status, "Failed to determine LU decomposition workspace size");

            CudaDeviceVariable<double>? workspaceVariable;
            CudaDeviceVariable<int>? pivotArrayVariable;
            CudaDeviceVariable<double>? matrixCopy;
            CudaDeviceVariable<int>? infoArrayVariable;
            CudaDeviceVariable<double>? identityMatrixDevice;
            // Allocate reusable GPU memory
            workspaceVariable = new CudaDeviceVariable<double>(workspaceSizeNeeded);
            pivotArrayVariable = new CudaDeviceVariable<int>(size);
            matrixCopy = new CudaDeviceVariable<double>(matrixVariable.Size);
            infoArrayVariable = new CudaDeviceVariable<int>(1);
            identityMatrixDevice = PrecomputeIdentityMatrix(size);
            try
            {
                return new InvertWithGPUHandle(
                    invert: () =>
                    {
                        try
                        {
                            // Reset matrixCopy to original matrix
                            matrixCopy.CopyToDevice(matrixVariable);

                            // Reset resultMatrixVariable to identity matrix
                            resultMatrixVariable.CopyToDevice(identityMatrixDevice);

                            // Perform LU decomposition
                            status = CuSolverInterop.cusolverDnDgetrf(
                                cudaHandles.CusolverHandle,
                                size, size,
                                matrixCopy.DevicePointer.Pointer,
                                size,
                                workspaceVariable.DevicePointer.Pointer,
                                pivotArrayVariable.DevicePointer.Pointer,
                                infoArrayVariable.DevicePointer.Pointer
                            );
                            checkStatus(status, "LU decomposition failed");

                            // Check decomposition info
                            int infoHost = 0;
                            infoArrayVariable.CopyToHost(ref infoHost);
                            if (infoHost != 0)
                            {
                                throw new Exception($"LU decomposition failed with info = {infoHost}");
                            }

                            // Solve A * X = I
                            status = CuSolverInterop.cusolverDnDgetrs(
                                cudaHandles.CusolverHandle,
                                CUBLAS_OP.N,
                                size, size,
                                matrixCopy.DevicePointer.Pointer,
                                size,
                                pivotArrayVariable.DevicePointer.Pointer,
                                resultMatrixVariable.DevicePointer.Pointer,
                                size,
                                infoArrayVariable.DevicePointer.Pointer
                            );
                            checkStatus(status, "Matrix inversion failed during solve");

                            // Check solve info
                            infoArrayVariable.CopyToHost(ref infoHost);
                            if (infoHost != 0)
                            {
                                throw new Exception($"Matrix inversion failed with info = {infoHost}");
                            }
                        }
                        finally
                        {
                            // Reusable variables are not disposed here
                        }
                    },
                    dispose: () =>
                    {
                        // Dispose shared resources
                        matrixCopy.Dispose();
                        workspaceVariable.Dispose();
                        pivotArrayVariable.Dispose();
                        infoArrayVariable.Dispose();
                        identityMatrixDevice.Dispose();
                    }
                );
            }
            catch
            {
                matrixCopy?.Dispose();
                workspaceVariable?.Dispose();
                pivotArrayVariable?.Dispose();
                infoArrayVariable?.Dispose();
                identityMatrixDevice?.Dispose();
                throw;
            }
        }

        // Helper function to precompute identity matrix on GPU
        private static CudaDeviceVariable<double> PrecomputeIdentityMatrix(int size)
        {
            var identityMatrixVariable = new CudaDeviceVariable<double>(size * size);
            // Kernel to initialize identity matrix on GPU
            double[] identityMatrix = new double[size * size];
            for (int i = 0; i < size; i++)
            {
                identityMatrix[i * size + i] = 1.0;
            }
            identityMatrixVariable.CopyToDevice(identityMatrix);
            return identityMatrixVariable;
        }


        public static long EstimateCPUOrGPUMemoryForMatrixInverseWithGPUColumnMajor(long matrixSize, bool requiresReadWriteReorganisation)
        {
            // Memory for matrix A (Host and GPU) - Matrix to be inverted
            long matrixMemory = matrixSize * matrixSize * sizeof(double);

            // Memory for the pivot array (used in LU decomposition)
            long pivotArrayMemory = matrixSize * sizeof(int);

            // Memory for the info array (single int value for error info)
            long infoArrayMemory = sizeof(int);

            // Workspace memory required for LU decomposition (approx. 2x matrix size)
            long workspaceMemory = matrixMemory * 2;

            // Memory for identity matrix (for solving AX = I)
            long identityMatrixMemory = matrixSize * matrixSize * sizeof(double);

            // Total memory required on the GPU (for matrix, pivot array, workspace, and identity matrix)
            long totalGPUMemory = matrixMemory + pivotArrayMemory + infoArrayMemory + workspaceMemory + identityMatrixMemory;

            // Total memory required on the host (for the matrix and identity matrix)
            long totalHostMemory = matrixMemory + identityMatrixMemory;

            // Total memory usage (Host + GPU)
            long totalMemoryUsed = totalGPUMemory + totalHostMemory;
            if (requiresReadWriteReorganisation)
                totalMemoryUsed += 2L * sizeof(double) * matrixSize;//used in writing to disk to reorganise from column major

            return totalMemoryUsed;
        }


        // Helper method to check cuBLAS status and throw an exception if there's an error
        private static Action<int, string> Get_CheckStatus_ForInversion(int size)
        {
            return Get_CheckStatusForCuSolver(() => $"size: {size}.");
        }
        private static Action<int, string> Get_CheckCublasStatus_ForMultiply(int rowsA, int colsA, int colsB)
        {
            return Get_CheckStatusForCublas(() => $"rowsA: {rowsA}, colsA: {colsA}, colsB: {colsB}.");
        }
        private static Action<int, string> Get_CheckStatusForCublas(Func<string> getAdditionalInfo)
        {
            return (status, errorMessage) =>
            {
                if (Enum.IsDefined(typeof(CublasStatus), status))
                {
                    CublasStatus cublasStatus = (CublasStatus)status;
                    switch(cublasStatus)
                    {
                        case CublasStatus.Success:
                        return;
                        case CublasStatus.AllocFailed:
                            throw new OutOfMemoryException();
                        default:
                            throw new Exception($"{errorMessage} with {nameof(CublasStatus)} {Enum.GetName(typeof(CublasStatus), cublasStatus)}. Additional info: {getAdditionalInfo?.Invoke()}");
                    }
                }
                throw new Exception($"{errorMessage} with unknown {nameof(CublasStatus)} {status}. Additional info: {getAdditionalInfo?.Invoke()}");
            };
        }
        private static Action<int, string> Get_CheckStatusForCuSolver(Func<string> getAdditionalInfo)
        {
            return (status, errorMessage) =>
            {
                if (Enum.IsDefined(typeof(cusolverStatus), status))
                {
                    cusolverStatus cusolverStatus = (cusolverStatus)status;
                    switch (cusolverStatus)
                    {
                        case cusolverStatus.Success:
                                return;
                        case cusolverStatus.AllocFailed:
                            throw new OutOfMemoryException();
                        default:
                            throw new Exception($"{errorMessage} with {nameof(cusolverStatus)} {Enum.GetName(typeof(cusolverStatus), cusolverStatus)}. Additional info: {getAdditionalInfo?.Invoke()}");
                    }
                }
                throw new Exception($"{errorMessage} with unknown {nameof(cusolverStatus)} {status}. Additional info: {getAdditionalInfo?.Invoke()}");
            };
        }
    }
}