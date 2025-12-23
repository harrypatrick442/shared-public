using System.IO;
using System;
using Core.FileSystem;
using InfernoDispatcher;
using InfernoDispatcher.Tasks;
using Core.Cleanup;
using InfernoDispatcher.Locking;
using Core.Pool;
using MathNet.Numerics;


namespace Core.Maths.BlockOperationMatrices
{
    public abstract class BlockOperationMatrix:IDisposable
    {
        protected const int FILE_STREAM_BUFFER_SIZE = 4096;
        protected const int EXTRA_MEMORY_FOR_GOOD_MESURE = 100000;
        protected const string DEFAULT_NAME = "M";
        protected bool _Disposed = false;
        protected InfernoTaskWithResultBase<BlockOperationMatrix>? _InvertTask;
        protected object _LockObjectFileAccess = new object();
        protected readonly object _LockObjectInternalTask = new object();
        private static readonly object _LockObjectNMatrixCount = new object();
        private int _NMatrix;
        private static int _NMatrixCount = 0;
        protected WorkingDirectoryManager _WorkingDirectoryManager;
        public string FilePath { get; protected set; }
        public int NRows { get; protected set; }
        public int NColumns { get; protected set; }

        public bool Partitioned { get; protected set; }
        public BlockOperationMatrix(
            int nRows, 
            int nColumns,
            WorkingDirectoryManager workingDirectoryManager
            )
        {
            NRows = nRows;
            NColumns = nColumns;
            _WorkingDirectoryManager = workingDirectoryManager;
            FilePath = workingDirectoryManager.NewBinFile();
            lock (_LockObjectNMatrixCount) {
                _NMatrix = _NMatrixCount++;
            }
        }
        public abstract InfernoTaskWithResultBase<BlockOperationMatrix> AddOnNewThread(BlockOperationMatrix other, CleanupHandler? cleanupHandler,
            InfernoFiniteResourceSemaphore? memoryAllocationSemaphore);
        public abstract InfernoTaskWithResultBase<BlockOperationMatrix> InvertOnNewThread(
            CleanupHandler? cleanupHandler,
            InfernoFiniteResourceSemaphore memoryAllocationLock,
            GPUMathsParameters? gpuMathsParameters,
            MathsRunningMode runningMode,
            CompositeProgressHandler? parentProgressHandler,
            bool cache = false);
        public abstract double[] MultiplyByVector(double[] vector, int startOffset);
        public abstract InfernoTaskWithResultBase<double[]> MultiplyByVectorOnNewThread(
            double[] vector, int startOffset, InfernoFiniteResourceSemaphore? memoryAllocationSemaphore);

        public abstract InfernoTaskWithResultBase<BlockOperationMatrix> MultiplyOnNewThread(
            BlockOperationMatrix other, CleanupHandler? cleanupHandler,
            InfernoFiniteResourceSemaphore memoryAllocationLock,
            GPUMathsParameters? gpuMathsParameters,
            MathsRunningMode runningMode,
            CompositeProgressHandler? parentProgressHandler);
        public abstract InfernoTaskWithResultBase<BlockOperationMatrix> ScaleOnNewThread(
            double scale, CleanupHandler? cleanupHandler,
            InfernoFiniteResourceSemaphore? memoryAllocationSemaphore);

        public abstract InfernoTaskWithResultBase<BlockOperationMatrix> SubtractOnNewThread(
            BlockOperationMatrix other, CleanupHandler? cleanupHandler,
            InfernoFiniteResourceSemaphore? memoryAllocationSemaphore);
        protected void UsingBlockMatrixReader(Action<BlockMatrixReader> callback) {

            lock (_LockObjectFileAccess)
            {
                using (var blockMatrixReader = new BlockMatrixReader(FilePath))
                {
                    callback(blockMatrixReader);
                }
            }
        }
        public static void UsingBlockMatrixReaders(BlockOperationMatrix a, BlockOperationMatrix b,
            Action<BlockMatrixReader, BlockMatrixReader> callback)
        {
            if (a._NMatrix < b._NMatrix)
            {
                a.UsingBlockMatrixReader(brA => {
                    b.UsingBlockMatrixReader(brB =>
                    {
                        callback(brA, brB);
                    });
                });
                return;
            }
            b.UsingBlockMatrixReader(brB => {
                a.UsingBlockMatrixReader(brA =>
                {
                    callback(brA, brB);
                });
            });
        }
        protected void UsingBlockMatrixWriter(Action<BlockMatrixWriter> callback) {

            lock (_LockObjectFileAccess)
            {
                using (var blockMatrixWriter = new BlockMatrixWriter(FilePath))
                {
                    callback(blockMatrixWriter);
                }
            }
        }
        public abstract double[][] ReadIntoMemory();
        public abstract void ReadIntoMemory(double[][] result, int offsetTop, int offsetLeft);
        private void CheckNotDisposed() {
            if (_Disposed) throw new ObjectDisposedException(nameof(BlockOperationMatrix));
        }
        public virtual void Dispose() {
            if (_Disposed) return;
            _Disposed = true;
            try
            {
                File.Delete(FilePath);
            }
            catch { }
             
        }
    }
}
