using System.IO;
using System;
using Core.FileSystem;
using InfernoDispatcher;
using InfernoDispatcher.Tasks;
using Core.Cleanup;
using InfernoDispatcher.Locking;
using Core.MemoryManagement;
using Core.Timing;
using Core.Maths.CUBLAS;
using Core.Pool;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections;


namespace Core.Maths.BlockOperationMatrices
{
    public class BlockOperationMatrixNonPartitioned
        : BlockOperationMatrix
    {
        private static volatile int debugCount = 0;
        private const long MIN_N_OPERATIONS_TO_USE_GPU_FOR_MULTIPLICATION = 48000000;
        public BlockOperationMatrixNonPartitioned(
            int nRows, int nColumns,
            WorkingDirectoryManager workingDirectoryManager) : base(nRows, nColumns, workingDirectoryManager)
        {
        }
        public BlockOperationMatrixNonPartitioned(int nRows, int nColumns,
            WorkingDirectoryManager workingDirectoryManager,
            double[][] data) : base(
                nRows, nColumns, workingDirectoryManager)
        {
            WriteToDiskNoLock(data);
        }
        public BlockOperationMatrixNonPartitioned(int nRows, int nColumns,
            WorkingDirectoryManager workingDirectoryManager,
            double[] data,
            bool isColumnMajor) : base(
                nRows, nColumns, workingDirectoryManager)
        {
            if (isColumnMajor)
                WriteToDiskNoLockColumnMajor(data);
            else
                WriteToDiskNoLockRowMajor(data);
        }
        public BlockOperationMatrixNonPartitioned(int nRows, int nColumns,
            WorkingDirectoryManager workingDirectoryManager,
            byte[] data)
            : base(nRows, nColumns, workingDirectoryManager)
        {
            double[] doubleArray = new double[nRows * nColumns];
            Buffer.BlockCopy(data, 0, doubleArray, 0, data.Length);
            UsingBlockMatrixWriter(bmw =>
                bmw.Write(doubleArray));
        }

        private void WriteToDiskNoLock(double[][] data)
        {
            UsingBlockMatrixWriter(bmw =>
            {
                double[] buffer = new double[NColumns];
                for (int row = 0; row < NRows; row++)
                {
                    if (data[row].Length != NColumns)
                    {
                        throw new ArgumentException("All rows must have the same number of columns.");
                    }
                    Array.Copy(data[row], 0, buffer, 0, NColumns);
                    bmw.Write(buffer);
                }
            });
        }
        private void WriteToDiskNoLockRowMajor(double[] src)
        {
            if (src.Length != NRows * NColumns)
            {
                throw new ArgumentException("Data size does not match _Matrix dimensions.");
            }
            UsingBlockMatrixWriter(bmw =>
            {
                double[] buffer = new double[NColumns];

                for (int row = 0; row < NRows; row++)
                {
                    // Copy row data from the 1D array into the buffer
                    Array.Copy(src, row * NColumns, buffer, 0, NColumns);

                    // Write the buffer (row) to the file
                    bmw.Write(buffer);
                }
            });
        }

        private void WriteToDiskNoLockColumnMajor(double[] data)
        {
            if (data.Length != NRows * NColumns)
            {
                throw new ArgumentException("Data size does not match _Matrix dimensions.");
            }

            UsingBlockMatrixWriter(bmw =>
            {

                // Loop over rows and write column-major data row by row
                for (int row = 0; row < NRows; row++)
                {
                    double[] buffer = new double[NColumns];
                    for (int col = 0; col < NColumns; col++)
                    {
                        // Column-major indexing: col * NRows + row
                        double value = data[col * NRows + row];
                        buffer[col] = value;
                    }

                    // Write the buffer (representing a full row in column-major order) to the file
                    bmw.Write(buffer);
                }
            });
        }
        public override InfernoTaskWithResultBase<BlockOperationMatrix> InvertOnNewThread(
            CleanupHandler? cleanupHandlerCallerParent,
            InfernoFiniteResourceSemaphore? mainMemoryAllocationSemaphore,
            GPUMathsParameters? gpuMathsParameters,
            MathsRunningMode runningMode,
            CompositeProgressHandler? parentProgressHandler,
            bool cache = false)
        {
            BinaryProgressHandler? progressHandler = null;
            if (parentProgressHandler!=null)
            {
                progressHandler = new BinaryProgressHandler();
                parentProgressHandler.AddChild(progressHandler);
            }
            var task = (runningMode switch
            {
                MathsRunningMode.GpuOnly =>
                    InvertOnNewThread_GpuOnly(
                        cleanupHandlerCallerParent,
                        mainMemoryAllocationSemaphore,
                        gpuMathsParameters),
                MathsRunningMode.CpuOnly =>
                    InvertOnNewThread_CpuOnly(
                        cleanupHandlerCallerParent,
                        mainMemoryAllocationSemaphore),
                MathsRunningMode.Whatever =>
                    InvertOnNewThread_WhateverHardareAvailable(
                        cleanupHandlerCallerParent,
                        mainMemoryAllocationSemaphore,
                        gpuMathsParameters),
                _=>
                    InvertOnNewThread_WhateverHardareAvailable(
                        cleanupHandlerCallerParent,
                        mainMemoryAllocationSemaphore,
                        gpuMathsParameters)

            });
            task.ThenWhatever(doneState=> {
                if (doneState.Exception == null && !doneState.Canceled)
                    progressHandler?.Set(true);
            });
            return task;
        }

        private InfernoTaskWithResultBase<BlockOperationMatrix> InvertOnNewThread_WhateverHardareAvailable(
            CleanupHandler? cleanupHandlerCaller,
            InfernoFiniteResourceSemaphore? mainMemoryAllocationSemaphore,
            GPUMathsParameters? gpuMathsParameters)
        {
            var runOnCpu = () => Invert_CPU_WithMemorySemaphore(cleanupHandlerCaller, mainMemoryAllocationSemaphore);
            if (gpuMathsParameters == null)
            {
                return runOnCpu();
            }
            //long nOperationsRequired = MatrixHelper.CalculateMatrixMultiplicationNOperations(NRows, NColumns, other.NColumns);
            long nRowsTimesNColumns = NRows * NColumns;
            long memoryRequiredCpu = MatrixHelper.EstimateMemoryForInvert(NRows);
            long memoryRequiredGpu = MatrixHelper.EstimateCPUOrGPUMemoryForMatrixInverseWithGPUColumnMajor(NRows, 
                requiresReadWriteReorganisation:false/*included down below for main memory only*/);
            if (nRowsTimesNColumns
                < gpuMathsParameters.MinNValuesInMatrixToUseGpuForInversion)
            {
                return runOnCpu();
            }
            var runGPU = () => InvertWithGPU_NoLock(
                cleanupHandlerCaller, gpuMathsParameters.CudaContextAssignedThreadPool);
            if (gpuMathsParameters.MemoryAllocationLock == null)
            {
                return runGPU();
            }
            Func<bool> doesntHaveEnoughGPUMemory = () => {
                return MemoryHelper.GetGPUMemoryMetrics().Free * gpuMathsParameters.MaxProportionMemoryCanUse
                   < memoryRequiredGpu;
            };
            if (doesntHaveEnoughGPUMemory())
            {
                return runOnCpu();
            }
            object lockObjectNeedToDoWithCPU = new object();
            bool needToDoWithCPU = false;
            var attemptGPUTask = ()=>gpuMathsParameters.MemoryAllocationLock.EnterCreateTask
             <BlockOperationMatrix>
             (memoryRequiredGpu, () =>
             {
                 bool n = doesntHaveEnoughGPUMemory();
                 lock (lockObjectNeedToDoWithCPU)
                 {
                     needToDoWithCPU = n;
                 }
                 if (n)
                 {
                     return InactiveInfernoTaskWithResult.NewSuccess<BlockOperationMatrix>(null);
                 }
                 return runGPU();
             })
         .ThenCreateTask<BlockOperationMatrix>(r =>
         {
             lock (lockObjectNeedToDoWithCPU)
             {
                 if (!needToDoWithCPU)
                 {
                     return InactiveInfernoTaskWithResult.NewSuccess(r);
                 }
             }
             return InvertOnNewThread_CPU_NoMemorySemaphore(cleanupHandlerCaller);
         });
            return mainMemoryAllocationSemaphore == null ? attemptGPUTask() : 
                mainMemoryAllocationSemaphore.EnterCreateTask(
                memoryRequiredGpu 
                +(3 * FILE_STREAM_BUFFER_SIZE)
                + (NColumns * sizeof(double))//Switching to and from column major
                + EXTRA_MEMORY_FOR_GOOD_MESURE, attemptGPUTask);
        }
        private InfernoTaskWithResultBase<BlockOperationMatrix> InvertOnNewThread_CpuOnly(
            CleanupHandler? cleanupHandlerCaller,
            InfernoFiniteResourceSemaphore? mainMemoryAllocationSemaphore)
        {
            long memoryRequired = MatrixHelper.EstimateMemoryForInvert(NRows);
            var attemptNoSemaphore = () => InvertOnNewThread_CPU_NoMemorySemaphore(cleanupHandlerCaller);
            return mainMemoryAllocationSemaphore == null 
                ? attemptNoSemaphore() 
                : mainMemoryAllocationSemaphore.EnterCreateTask(memoryRequired, attemptNoSemaphore);
        }
        private InfernoTaskWithResultBase<BlockOperationMatrix> InvertOnNewThread_GpuOnly(
            CleanupHandler? cleanupHandlerCaller,
            InfernoFiniteResourceSemaphore? mainMemoryAllocationSemaphore,
            GPUMathsParameters? gpuMathsParameters)
        {
            if (gpuMathsParameters == null)
            {
                throw new ArgumentException($"Since runningMode was set to {Enum.GetName(typeof(MathsRunningMode), MathsRunningMode.GpuOnly)}, {gpuMathsParameters} cannot be null");
            }
            long memoryRequired = MatrixHelper.EstimateCPUOrGPUMemoryForMatrixInverseWithGPUColumnMajor(NRows, 
                    false/*included down below for the main memory only*/);
            var runGPU = () => InvertWithGPU_NoLock(cleanupHandlerCaller,
                gpuMathsParameters.CudaContextAssignedThreadPool);

            var attemptGpuTask = gpuMathsParameters.MemoryAllocationLock == null
                ? runGPU
                : ()=>gpuMathsParameters.MemoryAllocationLock.EnterCreateTask<BlockOperationMatrix>
                 (memoryRequired, () =>
                 {
                     return runGPU();
                 });
            return mainMemoryAllocationSemaphore == null
                ? attemptGpuTask() : mainMemoryAllocationSemaphore.EnterCreateTask(
                    memoryRequired
                + (3 * FILE_STREAM_BUFFER_SIZE)
                + (NColumns * sizeof(double))//Switching to and from column major
                + EXTRA_MEMORY_FOR_GOOD_MESURE, attemptGpuTask);
        }
        public override InfernoTaskWithResultBase<BlockOperationMatrix> MultiplyOnNewThread(
            BlockOperationMatrix other, CleanupHandler? cleanupHandlerCaller,
            InfernoFiniteResourceSemaphore? mainMemoryAllocationSemaphore,
            GPUMathsParameters? gpuMathsParameters,
            MathsRunningMode runningMode,
            CompositeProgressHandler? parentProgressHandler)
        {
            BinaryProgressHandler? progressHandler = null;
            if (parentProgressHandler != null) {
                progressHandler = new BinaryProgressHandler();
                parentProgressHandler.AddChild(progressHandler);
            }
            var task = runningMode switch
            {
                MathsRunningMode.GpuOnly =>
                    MultiplyOnNewThread_GpuOnly(other, cleanupHandlerCaller, mainMemoryAllocationSemaphore,
                        gpuMathsParameters),
                MathsRunningMode.CpuOnly =>
                    MultiplyOnNewThread_CpuOnly(other, cleanupHandlerCaller, mainMemoryAllocationSemaphore),
                MathsRunningMode.Whatever =>
                    MultiplyOnNewThread_WhateverHardwareAvailable(other, cleanupHandlerCaller,
                        mainMemoryAllocationSemaphore, gpuMathsParameters),
                _ => throw new UnreachableException(Enum.GetName(typeof(MathsRunningMode), runningMode))
            };
            task.ThenWhatever(doneState => {
                if (doneState.Exception == null)
                    progressHandler?.Set(true);
            });
            return task;
        }
        private InfernoTaskWithResultBase<BlockOperationMatrix> MultiplyOnNewThread_WhateverHardwareAvailable(
            BlockOperationMatrix other, CleanupHandler? cleanupHandlerCaller,
            InfernoFiniteResourceSemaphore? mainMemoryAllocationSemaphore, GPUMathsParameters? gpuMathsParameters)
        {
            var runOnCpuWithMemorySemaphore = () => Multiply_CPU_WithMemorySemaphore(other, cleanupHandlerCaller, mainMemoryAllocationSemaphore);
            if (gpuMathsParameters == null)
            {
                return runOnCpuWithMemorySemaphore();
            }
            long nOperationsRequired = MatrixHelper.CalculateMatrixMultiplicationNOperations(NRows, NColumns, other.NColumns);
            if (nOperationsRequired
                < gpuMathsParameters.MinNOperationsToUseGpuForMultiplication)
            {
                return runOnCpuWithMemorySemaphore();
            }
            long memoryRequiredGpu = MatrixHelper.
                EstimateGPUOrCPUMemoryForMatrixMultiplyWithGPUColumnMajor(NRows, NColumns, other.NColumns, true);
            var runGPUNoMemorySemaphore = () => Multiply_GPU_NoMemorySemaphore(other, cleanupHandlerCaller, gpuMathsParameters.CudaContextAssignedThreadPool);
            if (gpuMathsParameters.MemoryAllocationLock == null)
            {
                return runGPUNoMemorySemaphore();
            }
            Func<bool> doesntHaveEnoughGPUMemory = () =>
            {
                return MemoryHelper.GetGPUMemoryMetrics().Free * gpuMathsParameters.MaxProportionMemoryCanUse
                   < memoryRequiredGpu;
            };
            if (doesntHaveEnoughGPUMemory())
            {
                return runOnCpuWithMemorySemaphore();
            }
            object lockObjectNeedToDoWithCPU = new object();
            bool needToDoWithCPU = false;
            //Notice it requires the same amount of memory on gpu and on cpu. This memory use is a good approximation
            //Basically two matrices in one out. 
            var attemptGPUTask = ()=>gpuMathsParameters.MemoryAllocationLock.EnterCreateTask
            <BlockOperationMatrix>
            (memoryRequiredGpu, () =>
            {
                bool n = doesntHaveEnoughGPUMemory();
                lock (lockObjectNeedToDoWithCPU)
                {
                    needToDoWithCPU = n;
                }
                if (n)
                {
                    return InactiveInfernoTaskWithResult.NewSuccess<BlockOperationMatrix>(null);
                }
                return runGPUNoMemorySemaphore();
            })
            .ThenCreateTask<BlockOperationMatrix>(r =>
            {
                lock (lockObjectNeedToDoWithCPU)
                {
                    if (!needToDoWithCPU)
                    {
                        return InactiveInfernoTaskWithResult.NewSuccess(r);
                    }
                }
                return Multiply_CPU_NoMemorySemaphore(other, cleanupHandlerCaller);
            });
            return mainMemoryAllocationSemaphore == null ? attemptGPUTask() : mainMemoryAllocationSemaphore.EnterCreateTask(memoryRequiredGpu, attemptGPUTask);
        }
        private InfernoTaskWithResultBase<BlockOperationMatrix> MultiplyOnNewThread_GpuOnly(
            BlockOperationMatrix other, CleanupHandler? cleanupHandlerCaller,
            InfernoFiniteResourceSemaphore? mainMemoryAllocationSemaphore, GPUMathsParameters? gpuMathsParameters)
        {
            if (gpuMathsParameters == null) throw new ArgumentNullException(nameof(GPUMathsParameters));
            long memoryRequiredGpuOrCpu = MatrixHelper
                .EstimateGPUOrCPUMemoryForMatrixMultiplyWithGPUColumnMajor(NRows, NColumns, other.NColumns, 
                    requiresReadWriteReorganisation:false/*included down below for the main memory equation*/);
            var runGPUNoMemorySemaphore = () => Multiply_GPU_NoMemorySemaphore(other, cleanupHandlerCaller, gpuMathsParameters.CudaContextAssignedThreadPool);
            var attemptGPUTask = gpuMathsParameters?.MemoryAllocationLock == null
                ? runGPUNoMemorySemaphore
                : () => gpuMathsParameters.MemoryAllocationLock.EnterCreateTask<BlockOperationMatrix>
                (memoryRequiredGpuOrCpu, runGPUNoMemorySemaphore);
            var run= mainMemoryAllocationSemaphore == null 
                ? attemptGPUTask 
                : () => mainMemoryAllocationSemaphore.EnterCreateTask(
                    memoryRequiredGpuOrCpu
                + (3 * FILE_STREAM_BUFFER_SIZE)
                +(NColumns*sizeof(double))//Switching to and from column major
                + EXTRA_MEMORY_FOR_GOOD_MESURE, attemptGPUTask);
            return MakeMultipleAttemptsIfNecessary(run, shouldReattemptOnException: IsOutOfMemoryException, delayBetweenAttemptsMilliseconds: 1000);
        }
        private bool IsOutOfMemoryException(Exception ex) {
            return typeof(OutOfMemoryException).IsAssignableFrom(ex.GetType());
        }
        private InfernoTaskWithResultBase<BlockOperationMatrix> MakeMultipleAttemptsIfNecessary(
            Func<InfernoTaskWithResultBase<BlockOperationMatrix>> run, Func<Exception, bool> shouldReattemptOnException,
            int? delayBetweenAttemptsMilliseconds) {
            var returnTask = new InactiveInfernoTaskWithResult<BlockOperationMatrix>(null, null);
            Action? attempt = null;
            Action<Exception> handleException = (ex) => {
                if (!shouldReattemptOnException(ex))
                {
                    returnTask.Fail(ex);
                }
                if (delayBetweenAttemptsMilliseconds == null)
                {
                    attempt!();
                }
                else
                {
                    Task.Delay((int)delayBetweenAttemptsMilliseconds)
                    .ContinueWith((ignore) =>
                    {
                        attempt!();
                    });
                }
            };
            attempt = () =>
            {
                try
                {
                    run().ThenWhatever((doneState) =>
                    {
                        if (returnTask.Cancelled)
                        {
                            return;
                        }
                        if (doneState.Exception != null)
                        {
                            handleException(doneState.Exception);
                            return;
                        }
                        returnTask.Success((BlockOperationMatrix)doneState.Result![0]);
                    });
                }
                catch (OutOfMemoryException ex) {
                    handleException(ex);   
                }
                catch (Exception ex)
                {
                    returnTask.Fail(ex);
                }
            };
            attempt();
            return returnTask;
        }
        private InfernoTaskWithResultBase<BlockOperationMatrix> MultiplyOnNewThread_CpuOnly(
            BlockOperationMatrix other, CleanupHandler? cleanupHandlerCaller,
            InfernoFiniteResourceSemaphore? mainMemoryAllocationSemaphore)
        {
            long memoryRequiredGpu = MatrixHelper.EstimateMemoryForMultiplyMultipleThreads(
                NRows, NColumns, other.NRows, other.NColumns, Environment.ProcessorCount);
            var run = () => Multiply_CPU_NoMemorySemaphore(other, cleanupHandlerCaller);
            return mainMemoryAllocationSemaphore == null 
                ? run() : 
                mainMemoryAllocationSemaphore.EnterCreateTask(memoryRequiredGpu, run);
        }
        private InfernoTaskWithResultBase<BlockOperationMatrix> Invert_CPU_WithMemorySemaphore(
            CleanupHandler? cleanupHandlerCaller,
            InfernoFiniteResourceSemaphore? mainMemoryAllocationSemaphore)
        {
            if (mainMemoryAllocationSemaphore == null) {
                return InvertOnNewThread_CPU_NoMemorySemaphore(cleanupHandlerCaller);
            }
            long memoryRequiredCpu = MatrixHelper.EstimateMemoryForInvert(NRows);
            return mainMemoryAllocationSemaphore.EnterCreateTask<BlockOperationMatrix>(memoryRequiredCpu, 
                ()=>InvertOnNewThread_CPU_NoMemorySemaphore(cleanupHandlerCaller));
        }
        private InfernoTaskWithResultBase<BlockOperationMatrix> InvertOnNewThread_CPU_NoMemorySemaphore(
            CleanupHandler? cleanupHandlerCaller)
        {
            return Dispatcher.Instance.Run(() =>
            {
                var result = (BlockOperationMatrix)new BlockOperationMatrixNonPartitioned(
                    NRows, NColumns, _WorkingDirectoryManager,
                    MatrixHelper.Invert(ReadIntoMemory())
                );
                cleanupHandlerCaller?.Add(result);
                return result;
            });
        }
        private InfernoTaskWithResultBase<BlockOperationMatrix> InvertWithGPU_NoLock(
            CleanupHandler? cleanupHandlerCaller, CudaContextAssignedThreadPool cusolverThreadPool) {

            double[] matrix = ReadIntoMemoryColumnMajor();
            return MatrixHelper.MatrixInverseWithGPUColumnMajor(matrix, NRows, cusolverThreadPool).Then(resultValues=>{
                    var result = new BlockOperationMatrixNonPartitioned(
                        NRows, NColumns, _WorkingDirectoryManager,
                        resultValues,
                        isColumnMajor: true
                    );
                    cleanupHandlerCaller?.Add(result);
                    return (BlockOperationMatrix)result;
                });
        }
        public BlockOperationMatrix MultiplyWithCPU(
            BlockOperationMatrix other, CleanupHandler? cleanupHandlerCaller)
        {
            if (other.Partitioned) throw new ArgumentException("Should not be attmepting to multiply a partitioned result with a non partitioned result");
            var otherNonParittioned = CheckNonPartitioned(other);
            double[][] sourceA = ReadIntoMemory();
            double[][] sourceB = otherNonParittioned.ReadIntoMemory();
            double[][] resultValues = MatrixHelper.Multiply(sourceA, sourceB);
            BlockOperationMatrixNonPartitioned result = new BlockOperationMatrixNonPartitioned(
                resultValues.Length, resultValues[0].Length, _WorkingDirectoryManager, resultValues);
            cleanupHandlerCaller?.Add(result);
            return (BlockOperationMatrix)result;
        }
        public InfernoTaskWithResultBase<BlockOperationMatrix> Multiply_CPU_NoMemorySemaphore(
            BlockOperationMatrix other, CleanupHandler? cleanupHandlerCaller)
        {
            if (other.Partitioned) throw new ArgumentException("Should not be attmepting to multiply a partitioned result with a non partitioned result");
            var otherNonParittioned = CheckNonPartitioned(other);
            double[][] sourceA = ReadIntoMemory();
            double[][] sourceB = otherNonParittioned.ReadIntoMemory();
            return MatrixHelper.MultiplyMultipleThreads(sourceA, sourceB).Then(resultValues =>
            {
                BlockOperationMatrixNonPartitioned result = new BlockOperationMatrixNonPartitioned(
                    resultValues.Length, resultValues[0].Length, _WorkingDirectoryManager, resultValues);
                cleanupHandlerCaller?.Add(result);
                return (BlockOperationMatrix)result;
            });
        }
        public InfernoTaskWithResultBase<BlockOperationMatrix> Multiply_GPU_NoMemorySemaphore(
            BlockOperationMatrix other, CleanupHandler? cleanupHandlerCaller, CudaContextAssignedThreadPool cudaThreadPool)
        {
            long startTime = TimeHelper.MillisecondsNow;
            if (other.Partitioned) throw new ArgumentException("Should not be attmepting to multiply a partitioned result with a non partitioned result");
            var otherNonParittioned = CheckNonPartitioned(other);
            double[] sourceA = ReadIntoMemoryColumnMajor();
            double[] sourceB= otherNonParittioned.ReadIntoMemoryColumnMajor();
            long secondTime = TimeHelper.MillisecondsNow - startTime;
            startTime = TimeHelper.MillisecondsNow;
            int debug = debugCount++;
            return MatrixHelper.MatrixMultiplyWithGPUColumnMajor(
                sourceA, sourceB, NRows, NColumns, otherNonParittioned.NColumns, cudaThreadPool)
            .ThenDebug(resultValues => {
                BlockOperationMatrixNonPartitioned result = new BlockOperationMatrixNonPartitioned(
                    NRows, otherNonParittioned.NColumns, _WorkingDirectoryManager,
                    resultValues, true);
                cleanupHandlerCaller?.Add(result);
                return (BlockOperationMatrix)result;
            }, debug);
        }
        public override InfernoTaskWithResultBase<BlockOperationMatrix> ScaleOnNewThread(double scale, CleanupHandler? cleanupHandler, InfernoFiniteResourceSemaphore? memoryAllocationSemaphore)
        {
            if (memoryAllocationSemaphore == null)
            {
                return Dispatcher.Instance.Run(() => ScaleNoMemorySemaphore(scale, cleanupHandler));
            }
            long memoryRequired = (2 * FILE_STREAM_BUFFER_SIZE) + EXTRA_MEMORY_FOR_GOOD_MESURE;
            return memoryAllocationSemaphore.Enter(memoryRequired, () => ScaleNoMemorySemaphore(scale, cleanupHandler));
        }
        private BlockOperationMatrix ScaleNoMemorySemaphore(double scale, CleanupHandler? cleanupHandlerCaller)
        {
            BlockOperationMatrixNonPartitioned result = new BlockOperationMatrixNonPartitioned(
                NRows, NColumns, _WorkingDirectoryManager);
            cleanupHandlerCaller?.Add(result);
            result.UsingBlockMatrixWriter(resultWriter =>
            {
                this.UsingBlockMatrixReader(reader =>
                {
                    for (int i = 0; i < NRows; i++)
                    {
                        for (int j = 0; j < NColumns; j++)
                        {
                            double aValue = reader.ReadDouble();
                            resultWriter.Write(scale * aValue);
                        }
                    }
                });
            });
            return (BlockOperationMatrix)result;
        }
        public override InfernoTaskWithResultBase<double[]> MultiplyByVectorOnNewThread(
            double[] vector, int vectorOffset, InfernoFiniteResourceSemaphore? memoryAllocationSemaphore)
        {
            if (memoryAllocationSemaphore == null)
            {
                return Dispatcher.Instance.Run(() => MultiplyByVector(vector, vectorOffset));
            }
            long memoryRequired = (2 * FILE_STREAM_BUFFER_SIZE) + EXTRA_MEMORY_FOR_GOOD_MESURE;
            return memoryAllocationSemaphore.Enter(memoryRequired, () => MultiplyByVector(vector, vectorOffset));
        }
        public override double[] MultiplyByVector(double[] vector, int vectorOffset)
        {
            double[] results = new double[NRows];

            // Ensure the reader starts from the beginning of the file for each row read.
            this.UsingBlockMatrixReader(reader =>
            {
                int resultIndex = 0; // Tracks which result index we are on
                int r = 0;           // Tracks the current row index
                while (r < NRows)
                {
                    double rowResult = 0;
                    int vectorIndex = vectorOffset; // Ensure we apply the correct vector offset
                    int c = 0;                      // Tracks the current column index

                    // Read a row from the file and multiply it by the vector
                    while (c < NColumns)
                    {
                        double matrixValue = reader.ReadDouble();
                        if (vectorIndex >= vector.Length)
                        {

                        }
                        rowResult += vector[vectorIndex++] * matrixValue; // Multiply by vector and accumulate result
                        c++;
                    }
                    results[resultIndex++] = rowResult; // Store the result for this row
                    r++;
                }
            });

            return results;
        }
        //CausingProblem
        //if it returns a task and doesnt consume a task then it instantly runs and schedules that task. tasks dependent on other tasks consume a task.
        private BlockOperationMatrix TwoMatrixValueToValueOperation(BlockOperationMatrix other, Func<double, double, double> operation, CleanupHandler? cleanupHandlerCaller)
        {

            BlockOperationMatrixNonPartitioned result = new BlockOperationMatrixNonPartitioned(
                NRows, NColumns, _WorkingDirectoryManager);
            cleanupHandlerCaller?.Add(result);
            result.UsingBlockMatrixWriter(resultWriter =>
            {
                BlockOperationMatrix.UsingBlockMatrixReaders(this, other, (readerA, readerB) =>
                {
                    for (int i = 0; i < NRows; i++)
                    {
                        for (int j = 0; j < NColumns; j++)
                        {
                            double aValue = readerA.ReadDouble();
                            double bValue = readerB.ReadDouble();
                            resultWriter.Write(operation(aValue, bValue));
                        }
                    }
                });
            });
            return (BlockOperationMatrix)result;
        }
        public override double[][] ReadIntoMemory()
        {
            lock (_LockObjectFileAccess)
            {
                double[][] result = new double[NRows][];
                UsingBlockMatrixReader(bmr =>
                {
                    double[] buffer = new double[NColumns]; // Buffer to hold a row (NColumns * 8 bytes)
                    for (int row = 0; row < NRows; row++)
                    {
                        double[] values = bmr.Read(NColumns);
                        if (values.Length != NColumns)
                        {
                            throw new EndOfStreamException("Unexpected end of file while reading double values.");
                        }
                        result[row] = values;
                    }
                });
                return result;
            }
        }
        public override void ReadIntoMemory(double[][] result, int offsetTop, int offsetLeft)
        {

            if (result == null || result.Length == 0 || result[0].Length == 0)
            {
                throw new ArgumentException("Result result must not be null or empty.");
            }

            // Validate that the result matrix can accommodate the data based on the provided offsets
            if (offsetTop < 0 || offsetLeft < 0 || offsetTop + NRows > result.Length || offsetLeft + NColumns > result[0].Length)
            {
                throw new ArgumentOutOfRangeException("Offsets and result size do not allow for reading src into the specified region.");
            }
            UsingBlockMatrixReader((br =>
            {
                for (int i = 0; i < NRows; i++)
                {
                    for (int j = 0; j < NColumns; j++)
                    {
                        result[i + offsetTop][j + offsetLeft] = br.ReadDouble();
                    }
                }
            }));
            /*
            return;
            lock (_LockObjectFileAccess)
            {

                // Open the file stream to start reading from the beginning of the data
                FileStream fs = GetFileStreamAtBeginning();

                // Buffer to hold a single row (NColumns * 8 bytes, since each double is 8 bytes)
                byte[] buffer = new byte[NColumns * sizeof(double)];

                // Loop over each row of the source data
                for (int row = 0; row < NRows; row++)
                {
                    // Read a full row (as bytes) into the buffer
                    int bytesRead = fs.Read(buffer, 0, buffer.Length);
                    if (bytesRead != buffer.Length)
                    {
                        throw new EndOfStreamException("Unexpected end of file while reading double values.");
                    }

                    // Copy the buffer into the appropriate location in the result matrix
                    Buffer.BlockCopy(buffer, 0, result[offsetTop + row], offsetLeft * sizeof(double), buffer.Length);
                }
            }*/
        }
        public double[] ReadIntoMemoryColumnMajor()
{
    lock (_LockObjectFileAccess)
    {
        // Create a 1D array to store the matrix in column-major order
        double[] result = new double[NRows * NColumns];

        UsingBlockMatrixReader(bmr =>
        {
            // Loop over rows and columns, and populate the result in column-major order
            for (int row = 0; row < NRows; row++)
            {
                // Read a full row (as bytes) into the buffer
                double[] values = bmr.Read(NColumns);
                if (values.Length != NColumns)
                {
                    throw new EndOfStreamException("Unexpected end of file while reading double values.");
                }

                // Copy the row data to the correct column-major position
                for (int col = 0; col < NColumns; col++)
                {
                    // Column-major indexing: col * NRows + row
                    result[col * NRows + row] = values[col];
                }
            }
        });

        return result; // Return matrix in column-major order
    }
}

        private BlockOperationMatrixNonPartitioned CheckNonPartitioned(BlockOperationMatrix other)
        {
            if (other.Partitioned)
            {
                throw new ArgumentException($"{nameof(BlockOperationMatrix)} {nameof(other)} with path:{other.FilePath} was not NON partitioned");
            }
            BlockOperationMatrixNonPartitioned nonPartitioned = (BlockOperationMatrixNonPartitioned)other;
            return nonPartitioned;
        }

        private InfernoTaskWithResultBase<BlockOperationMatrix>  Multiply_CPU_WithMemorySemaphore(
            BlockOperationMatrix other, CleanupHandler? cleanupHandlerCaller,
            InfernoFiniteResourceSemaphore? memoryAllocationSemaphore) {
            //Three double[][] arrays, two for sources one for result to go into memory
            if (memoryAllocationSemaphore == null) {
                return Multiply_CPU_NoMemorySemaphore(other, cleanupHandlerCaller);
            }
            long cpuMemoryRequired = 3 * NRows * ((NColumns * sizeof(double)) + IntPtr.Size);
            return memoryAllocationSemaphore.EnterCreateTask<BlockOperationMatrix>(cpuMemoryRequired, 
                () => Multiply_CPU_NoMemorySemaphore(other, cleanupHandlerCaller));
        }
        public override InfernoTaskWithResultBase<BlockOperationMatrix> AddOnNewThread(
            BlockOperationMatrix other, CleanupHandler? cleanupHandlerCaller,
            InfernoFiniteResourceSemaphore? memoryAllocationSemaphore)
        {
            var run = () => TwoMatrixValueToValueOperation(other, (a, b) => (a + b), cleanupHandlerCaller);
            if (memoryAllocationSemaphore == null)
            {
                return Dispatcher.Instance.Run(run);
            }
            long memoryRequired = (2 * FILE_STREAM_BUFFER_SIZE) + EXTRA_MEMORY_FOR_GOOD_MESURE;
            return memoryAllocationSemaphore.Enter(memoryRequired, run);
        }

        public override InfernoTaskWithResultBase<BlockOperationMatrix> SubtractOnNewThread(
            BlockOperationMatrix other, CleanupHandler? cleanupHandlerCaller,
            InfernoFiniteResourceSemaphore? memoryAllocationSemaphore)
        {
            var run = ()=>TwoMatrixValueToValueOperation(
                other, (a, b) => (a - b), cleanupHandlerCaller);
            if (memoryAllocationSemaphore == null)
            {
                return Dispatcher.Instance.Run(run);
            }
            long memoryRequired = (2*FILE_STREAM_BUFFER_SIZE) + EXTRA_MEMORY_FOR_GOOD_MESURE;
            return memoryAllocationSemaphore.Enter(memoryRequired, run);
        }
    }
}
