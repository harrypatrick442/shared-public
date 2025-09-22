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
        protected void UsingFileStreamAtBeginning(Action<FileStream> callback)
        {
            lock (_LockObjectFileAccess)
            {
                using (FileStream fs = new FileStream(FilePath,
                FileMode.OpenOrCreate, FileAccess.ReadWrite,
                FileShare.None, FILE_STREAM_BUFFER_SIZE))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    callback(fs);
                    fs.Flush();
                }
            }
        }
        protected void UsingBinaryReader(Action<BinaryReader> callback) {
                UsingFileStreamAtBeginning(fs =>
                {
                    using (var binaryReader = new BinaryReader(fs, System.Text.Encoding.Default, leaveOpen: true))
                    {
                        callback(binaryReader);
                    }
                });
        }
        public static void UsingBinaryReaders(BlockOperationMatrix a, BlockOperationMatrix b, Action<BinaryReader, BinaryReader> callback)
        {
            if (a._NMatrix < b._NMatrix)
            {
                a.UsingBinaryReader(brA => {
                    b.UsingBinaryReader(brB =>
                    {
                        callback(brA, brB);
                    });
                });
                return;
            }
            b.UsingBinaryReader(brB => {
                a.UsingBinaryReader(brA =>
                {
                    callback(brA, brB);
                });
            });
        }
        protected void UsingBinaryWriter(Action<BinaryWriter> callback) {
                UsingFileStreamAtBeginning(fs =>
                {
                    using (var binaryWriter = new BinaryWriter(fs, System.Text.Encoding.Default, leaveOpen: true))
                    {
                        callback(binaryWriter);
                    }
                });
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
