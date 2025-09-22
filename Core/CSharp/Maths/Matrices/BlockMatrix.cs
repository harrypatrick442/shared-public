using Core.Pool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Core.Maths.Matrices
{
    public class BlockMatrix : IBigMatrix
    {
        private const int DEFAULT_BLOCK_SIZE = 4096;
        public int NRows { get; }
        public int NColumns { get; }
        public int NRowBlocks { get; }
        private long _NBytesPerColumn;
        private long _NBytesPerRow;
        private double[] _DoubleRow;
        private bool _Disposed = false;
        private FileStream _FileStream;
        private LinkedList<BlockMatrix_Block> _LRUList;  // LRU list to track access order
        private Dictionary<long, LinkedListNode<BlockMatrix_Block>> _BlockCache;  // Cache for fast lookup
        private int _BlockSize;  // 4 KB block size
        public int BlockSize { get { return _BlockSize; } }
        private string _FilePath;
        private int _UpperBoundMaxCacheSizeBlocks;  // Maximum number of blocks in cache
        private int _LowerBoundMaxCacheSizeBlocks;
        private int _NReadsPerUpdateMaxCacheSize;
        private int _NReadsSinceUpdateMaxCacheSize;
        private double _ProportionFreeMemoryConsume;
        private bool _FixInitialCacheSizeInfoForDebugOnly;
        private bool _VerboseOverflow;
        private const double PROPORTION_MAX_CACHE_SIZE_USABLE_TO_ALLOW_FOR_PERIODIC_CACHE_SIZE_CHECK = 0.9d;
        public double[][] Data
        {
            get
            {
                CheckNotDisposed();

                // Initialize a jagged array to hold all the rows of the matrix
                double[][] data = new double[NRows][];

                // Populate each row by reading the corresponding row from the matrix
                for (int i = 0; i < NRows; i++)
                {
                    data[i] = ReadRow(i);
                }

                return data;
            }
        }
        public double ProportionOfMatrixInMemory
        {
            get
            {
                double proportion = _LRUList.Count * (double)_BlockSize / ((double)_NBytesPerRow * NRows);
                if (proportion > 1) proportion = 1;
                return proportion;
            }
        }
        public double ProportionOfMaxCacheSizeUsed
        {
            get
            {
                double proportion = _LRUList.Count * (double)_BlockSize / _UpperBoundMaxCacheSizeBlocks;
                if (proportion > 1) proportion = 1;
                return proportion;
            }
        }
        public BlockMatrix(int nRows, int nColumns, string filePath,
            int blockSize = DEFAULT_BLOCK_SIZE, double proportionFreeMemoryConsume = 0.8,
            bool fixInitialCacheSizeInfoForDebugOnly = false, StandardProgressHandler? progressHandler = null, bool verboseOverflow = false)
        {
            _BlockSize = blockSize;
            _ProportionFreeMemoryConsume = proportionFreeMemoryConsume;
            _FixInitialCacheSizeInfoForDebugOnly = fixInitialCacheSizeInfoForDebugOnly;
            UpdateCacheSizeInfo();
            if (_BlockSize % sizeof(double) != 0)
            {
                throw new InvalidOperationException($"BlockMatrix_Block size must be a multiple of {sizeof(double)} bytes.");
            }
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            NRows = nRows;
            NColumns = nColumns;
            _NBytesPerColumn = sizeof(double);
            _NBytesPerRow = NColumns * _NBytesPerColumn;
            _DoubleRow = new double[NColumns];
            _FilePath = filePath;
            _FileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _BlockCache = new Dictionary<long, LinkedListNode<BlockMatrix_Block>>();
            _LRUList = new LinkedList<BlockMatrix_Block>();  // List to track access order
                                                             // Write zeros to the entire file to initialize it
            _VerboseOverflow = verboseOverflow;
            InitializeFileWithZeros(progressHandler);
        }

        private void InitializeFileWithZeros(StandardProgressHandler? progressHandler)
        {
            // Total number of bytes for the matrix
            long totalBytes = NRows * _NBytesPerRow;

            // Create a buffer of zeros to write
            byte[] zeroBuffer = new byte[_BlockSize]; // Block-sized chunk of zeros

            long bytesWritten = 0;

            // Write zeros to the file in chunks of  _BlockSize
            Action? updateProgress = progressHandler?.GetUpdateProgress(
                (int)(totalBytes / _BlockSize), 100);
            while (bytesWritten < totalBytes)
            {
                long bytesToWrite = Math.Min(_BlockSize, totalBytes - bytesWritten);
                _FileStream.Write(zeroBuffer, 0, (int)bytesToWrite);
                bytesWritten += bytesToWrite;
                updateProgress?.Invoke();
            }
            progressHandler?.Set(1);

            // Flush to ensure all zeros are written to disk
            _FileStream.Flush();
        }
        ~BlockMatrix()
        {
            Dispose();
        }
        private void UpdateCacheSizeInfo()
        {
            var memoryMetrics = MemoryManagement.MemoryHelper.GetMemoryMetricsNow();
            long freeMemory = (long)memoryMetrics.FreeMb * 1000000;
            _UpperBoundMaxCacheSizeBlocks = (int)(freeMemory * _ProportionFreeMemoryConsume * PROPORTION_MAX_CACHE_SIZE_USABLE_TO_ALLOW_FOR_PERIODIC_CACHE_SIZE_CHECK / _BlockSize);
            _LowerBoundMaxCacheSizeBlocks = (int)(_UpperBoundMaxCacheSizeBlocks * PROPORTION_MAX_CACHE_SIZE_USABLE_TO_ALLOW_FOR_PERIODIC_CACHE_SIZE_CHECK);
            _NReadsPerUpdateMaxCacheSize = (int)(_UpperBoundMaxCacheSizeBlocks * (1 - PROPORTION_MAX_CACHE_SIZE_USABLE_TO_ALLOW_FOR_PERIODIC_CACHE_SIZE_CHECK));
        }
        private void OverflowIfNecessary()
        {
            if (_BlockCache.Count < _UpperBoundMaxCacheSizeBlocks) return;
            Overflow();
        }
        private void Overflow()
        {
            int nEntriesToOverflow = _BlockCache.Count - _LowerBoundMaxCacheSizeBlocks;
            StandardProgressHandler? progressHandler = _VerboseOverflow ? new StandardProgressHandler() : null;
            var updateProgress = progressHandler?.GetUpdateProgress(nEntriesToOverflow, nEntriesToOverflow > 100 ? nEntriesToOverflow / 100 : nEntriesToOverflow);
            progressHandler?.RegisterPrintPercentSameLine($"Partially overflowing {nameof(BlockMatrix)} to disk: ");
            while (nEntriesToOverflow > 0)
            {
                LinkedListNode<BlockMatrix_Block> oldestNode = _LRUList.Last;
                if (oldestNode == null) return;
                BlockMatrix_Block block = oldestNode.Value;
                Flush(oldestNode.Value);
                _BlockCache.Remove(block.Index);
                _LRUList.RemoveLast();
                nEntriesToOverflow--;
                updateProgress?.Invoke();
            }
            progressHandler?.Set(1);
        }
        private void UpdateCacheSizeAndOverflowIfNecessary()
        {
            if (++_NReadsSinceUpdateMaxCacheSize < _NReadsPerUpdateMaxCacheSize)
            {
                return;
            }
            _NReadsSinceUpdateMaxCacheSize = 0;
            if (!_FixInitialCacheSizeInfoForDebugOnly)
                UpdateCacheSizeInfo();
            OverflowIfNecessary();
        }
        public MathNet.Numerics.LinearAlgebra.Vector<double> MultiplyByVectorToMathNet(IList<double> vector)
        {
            return MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(MultiplyByVector(vector));
        }

        // First level indexer to access rows
        public double this[int rowIndex, int columnIndex]
        {
            get
            {
                return ReadValue(rowIndex, columnIndex);
            }
            set
            {
                WriteValue(rowIndex, columnIndex, value);
            }
        }
        public double ReadValue(int rowIndex, int columnIndex)
        {
            CheckNotDisposed();
            ValidateIndices(rowIndex, columnIndex);
            GetBlockIndexAndOffsetInBlockFromRow(rowIndex, columnIndex, out long blockIndex, out int blockOffset);
            BlockMatrix_Block block = GetBlock(blockIndex);
            return BitConverter.ToDouble(block.Data, blockOffset);
        }
        public void WriteValue(int rowIndex, int columnIndex, double value)
        {
            CheckNotDisposed();
            ValidateIndices(rowIndex, columnIndex);
            GetBlockIndexAndOffsetInBlockFromRow(rowIndex, columnIndex, out long blockIndex, out int blockOffset);
            BlockMatrix_Block block = GetBlock(blockIndex);
            Span<byte> blockSpan = block.Data.AsSpan(blockOffset, sizeof(double));
            MemoryMarshal.Write(blockSpan, ref value);  // Write the double value to the block's data
            block.IsDirty = true;
        }
        private BlockMatrix_Block GetBlock(long blockIndex)
        {

            if (_BlockCache.TryGetValue(blockIndex, out LinkedListNode<BlockMatrix_Block>? node))
            {
                _LRUList.Remove(node!);
                _LRUList.AddFirst(node!);
                return node.Value;
            }
            UpdateCacheSizeAndOverflowIfNecessary();
            byte[] blockData = new byte[_BlockSize];

            _FileStream.Seek(blockIndex * _BlockSize, SeekOrigin.Begin);
            _FileStream.Read(blockData, 0, _BlockSize);

            BlockMatrix_Block block = new BlockMatrix_Block(blockIndex, blockData);
            node = new LinkedListNode<BlockMatrix_Block>(block);
            _LRUList.AddFirst(node);
            _BlockCache[blockIndex] = node;
            return block;
        }
        public double[] ReadRow(int rowIndex)
        {
            double[] row = new double[NColumns];
            ReadRow(rowIndex, row);
            return row;
        }
        private void ReadRow(int rowIndex, double[] row)
        {
            CheckNotDisposed();
            ValidateRowIndex(rowIndex);
            GetBlockIndexAndOffsetInBlockFromRow(rowIndex, 0, out long blockIndex, out int blockOffset);
            long nBytesLeftToRead = _NBytesPerRow;
            long nBytesRead = 0;
            int doubleIndex = 0;
            do
            {
                BlockMatrix_Block block = GetBlock(blockIndex);
                long nBytesCanReadInBlock = _BlockSize - blockOffset;
                long nBytesToRead = nBytesLeftToRead > nBytesCanReadInBlock ? nBytesCanReadInBlock : nBytesLeftToRead;
                Span<byte> blockSpan = block.Data.AsSpan(blockOffset, (int)nBytesToRead);
                Span<double> doubleSpan = MemoryMarshal.Cast<byte, double>(blockSpan);
                doubleSpan.CopyTo(row.AsSpan(doubleIndex));
                doubleIndex += doubleSpan.Length;
                nBytesLeftToRead -= nBytesToRead;
                nBytesRead += nBytesToRead;
                blockIndex++;
                blockOffset = 0;
            }
            while (nBytesLeftToRead > 0);
        }
        public void WriteRow(int rowIndex, double[] values)
        {
            CheckNotDisposed();
            ValidateRowIndex(rowIndex);
            if (values.Length != NColumns)
                throw new IndexOutOfRangeException($"The length of {nameof(values)} ({values.Length})was not equal to {nameof(NColumns)} ({NColumns})");
            GetBlockIndexAndOffsetInBlockFromRow(rowIndex, 0, out long blockIndex, out int blockOffset);

            Span<byte> byteValues = MemoryMarshal.Cast<double, byte>(values);
            long nBytesLeftToWrite = _NBytesPerRow;
            long nBytesWritten = 0;
            do
            {
                BlockMatrix_Block block = GetBlock(blockIndex);
                long nBytesCanWriteInBlock = _BlockSize - blockOffset;
                long nBytesToWrite = nBytesLeftToWrite > nBytesCanWriteInBlock ? nBytesCanWriteInBlock : nBytesLeftToWrite;
                // Copy the relevant bytes directly from byteValues into the block's data
                byteValues.Slice((int)nBytesWritten, (int)nBytesToWrite)
                    .CopyTo(block.Data.AsSpan(blockOffset, (int)nBytesToWrite));
                nBytesLeftToWrite -= nBytesToWrite;
                nBytesWritten += nBytesToWrite;
                blockIndex++;
                blockOffset = 0;
                block.IsDirty = true;
            }
            while (nBytesLeftToWrite > 0);
        }
        private void GetBlockIndexAndOffsetInBlockFromRow(int rowIndex, int columnIndex, out long blockIndex, out int offsetInBlock)
        {
            long position = rowIndex * _NBytesPerRow + columnIndex * _NBytesPerColumn;
            blockIndex = position / _BlockSize;
            offsetInBlock = (int)(position % _BlockSize);
        }
        private long GetPositionFromRowColumn(int rowIndex, int columnIndex)
        {
            return rowIndex * _NBytesPerRow + columnIndex * _NBytesPerColumn;
        }
        private void SeekRowColumn(int rowIndex, int columnIndex)
        {
            long position = GetPositionFromRowColumn(rowIndex, columnIndex);
            _FileStream.Seek(position, SeekOrigin.Begin);
        }
        private void SeekRow(int rowIndex)
        {
            long position = rowIndex * _NBytesPerRow;
            _FileStream.Seek(position, SeekOrigin.Begin);
        }
        private void CheckNotDisposed()
        {
            if (_Disposed)
                throw new ObjectDisposedException(nameof(BlockMatrix));
        }
        private void ValidateIndices(int rowIndex, int columnIndex)
        {
            ValidateRowIndex(rowIndex);
            ValidateColumnIndex(columnIndex);
        }
        private void ValidateRowIndex(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= NRows)
                throw new IndexOutOfRangeException("RowIndex index out of range.");
        }
        private void ValidateColumnIndex(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= NColumns)
                throw new IndexOutOfRangeException("ColumnIndex index out of range.");
        }
        public double[] MultiplyByVector(IList<double> v)
        {
            // Check if the vector's length matches the number of columns in the matrix
            if (v.Count != NColumns)
                throw new ArgumentException($"Vector length ({v.Count}) must match the number of columns ({NColumns}) in the _Matrix.");

            // Create a result vector with the same number of elements as the number of rows in the matrix
            double[] result = new double[NRows];

            // Perform matrix-vector multiplication
            for (int rowIndex = 0; rowIndex < NRows; rowIndex++)
            {
                ReadRow(rowIndex, _DoubleRow);  // Read the current row of the matrix
                double sum = 0;

                // Multiply the row by the vector
                for (int colIndex = 0; colIndex < NColumns; colIndex++)
                {
                    sum += _DoubleRow[colIndex] * v[colIndex];
                }

                // Store the result for the current row
                result[rowIndex] = sum;
            }

            return result;  // Return the resulting vector
        }

        public static double[] operator *(BlockMatrix matrix, double[] vector)
        {
            return matrix.MultiplyByVector(vector);
        }
        public static double[] operator *(BlockMatrix matrix, MathNet.Numerics.LinearAlgebra.Vector<double> vector)
        {
            return matrix.MultiplyByVector(vector);
        }
        // Flush all cached blocks to the file
        public void Flush()
        {
            foreach (var block in _LRUList)
            {
                Flush(block);
            }
        }
        private void Flush(BlockMatrix_Block block)
        {

            if (block.IsDirty)
            {
                _FileStream.Seek(block.Index * _BlockSize, SeekOrigin.Begin);
                _FileStream.Write(block.Data, 0, _BlockSize);
                _FileStream.Flush();
                block.IsDirty = false;  // Mark as not dirty after flush
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    _FileStream?.Dispose();
                    _FileStream = null;
                    try
                    {
                        File.Delete(_FilePath);
                    }
                    catch { }
                }

                _Disposed = true;
            }
        }

        public byte[] ReadBlockBytes(int nRows, int nColumns, int offsetTop, int offsetLeft)
        {
            // Validate the input dimensions
            if (offsetTop < 0 || offsetTop + nRows > NRows || offsetLeft < 0 || offsetLeft + nColumns > NColumns)
                throw new IndexOutOfRangeException($"Requested block exceeds _Matrix dimensions. nRows:{nRows}, nColumns:{nColumns}, offsetTop:{offsetTop}, offsetLeft:{offsetLeft}");

            byte[] result = new byte[nRows * nColumns * sizeof(double)];
            long bytesPerRow = nColumns * sizeof(double);

            int resultOffset = 0;

            for (int row = 0; row < nRows; row++)
            {
                // Calculate the row index in the full matrix
                int rowIndex = offsetTop + row;

                // Get the block data from the matrix
                GetBlockIndexAndOffsetInBlockFromRow(rowIndex, offsetLeft, out long blockIndex, out int blockOffset);

                long nBytesLeftToRead = bytesPerRow;
                long nBytesRead = 0;

                while (nBytesLeftToRead > 0)
                {
                    BlockMatrix_Block block = GetBlock(blockIndex);

                    // Calculate how many bytes we can read from this block
                    long nBytesCanReadInBlock = _BlockSize - blockOffset;
                    long nBytesToRead = Math.Min(nBytesLeftToRead, nBytesCanReadInBlock);

                    // Copy data from the block to the result buffer
                    Buffer.BlockCopy(block.Data, blockOffset, result, resultOffset, (int)nBytesToRead);

                    // Update offsets and counters
                    resultOffset += (int)nBytesToRead;
                    nBytesLeftToRead -= nBytesToRead;
                    nBytesRead += nBytesToRead;

                    // Move to the next block if necessary
                    blockIndex++;
                    blockOffset = 0;  // Reset to the beginning of the next block
                }
            }/*
            double[][] doubles = ExtractPartitionFromData(Data, nRows, nColumns, offsetTop, offsetLeft);
            var res2 = ConvertDoubleJaggedArrayToByteArray(ReadPartition(nRows, nColumns, offsetTop, offsetLeft));
            var fromParittion  = ConvertDoubleJaggedArrayToByteArray(ExtractPartitionFromData(Data, nRows, nColumns, offsetTop, offsetLeft));

            for (int i = 0; i < res2.Length; i++)
            {
                if (res2[i] != result[i])
                {

                }
                if (fromParittion[i] != result[i])
                {

                }
            }*/
            return result;
        }
        public double[][] ReadPartition(int nRows, int nColumns, int offsetTop, int offsetLeft)
        {
            // Validate the input dimensions
            if (offsetTop < 0 || offsetTop + nRows > NRows || offsetLeft < 0 || offsetLeft + nColumns > NColumns)
                throw new IndexOutOfRangeException($"Requested partition exceeds _Matrix dimensions. nRows:{nRows}, nColumns:{nColumns}, offsetTop:{offsetTop}, offsetLeft:{offsetLeft}");

            // Initialize a jagged array to hold the partition
            double[][] partition = new double[nRows][];

            for (int row = 0; row < nRows; row++)
            {
                // Calculate the row index in the full matrix
                int rowIndex = offsetTop + row;

                // Initialize the row in the partition
                partition[row] = new double[nColumns];

                // Get block index and offset for the current row
                GetBlockIndexAndOffsetInBlockFromRow(rowIndex, offsetLeft, out long blockIndex, out int blockOffset);

                long nBytesLeftToRead = nColumns * sizeof(double); // Number of bytes to read for this row
                int doubleIndex = 0;

                // Read bytes from the block and populate the partition row
                while (nBytesLeftToRead > 0)
                {
                    BlockMatrix_Block block = GetBlock(blockIndex);

                    // Calculate how many bytes can be read from this block
                    long nBytesCanReadInBlock = _BlockSize - blockOffset;
                    long nBytesToRead = Math.Min(nBytesLeftToRead, nBytesCanReadInBlock);

                    // Get the byte span from the block and convert it to double
                    Span<byte> blockSpan = block.Data.AsSpan(blockOffset, (int)nBytesToRead);
                    Span<double> doubleSpan = MemoryMarshal.Cast<byte, double>(blockSpan);

                    // Copy the double values into the partition row
                    doubleSpan.CopyTo(partition[row].AsSpan(doubleIndex));

                    // Update offsets and counters
                    doubleIndex += doubleSpan.Length;
                    nBytesLeftToRead -= nBytesToRead;
                    blockIndex++;
                    blockOffset = 0;  // Reset offset for the next block
                }
            }

            return partition;
        }
        public static byte[] ConvertDoubleJaggedArrayToByteArray(double[][] jaggedArray)
        {
            if (jaggedArray == null || jaggedArray.Length == 0)
            {
                throw new ArgumentException("The input jagged array is null or empty.");
            }

            // Calculate the total number of elements
            int totalElements = jaggedArray.Sum(row => row.Length);

            // Allocate a byte array with the appropriate size to hold all double values
            // Each double is 8 bytes, so total size = totalElements * 8
            byte[] byteArray = new byte[totalElements * sizeof(double)];

            int byteOffset = 0;

            // Iterate through each row in the jagged array
            foreach (var row in jaggedArray)
            {
                // Convert each row (double[]) into a byte[] and copy to the final byte array
                int rowSizeInBytes = row.Length * sizeof(double);
                Buffer.BlockCopy(row, 0, byteArray, byteOffset, rowSizeInBytes);
                byteOffset += rowSizeInBytes;
            }

            return byteArray;
        }
        public double[][] ExtractPartitionFromData(double[][] data, int nRows, int nColumns, int offsetTop, int offsetLeft)
        {
            // Validate input
            if (data == null || data.Length == 0 || data[0].Length == 0)
            {
                throw new ArgumentException("The input data array is null or empty.");
            }
            if (offsetTop < 0 || offsetTop + nRows > data.Length)
            {
                throw new ArgumentOutOfRangeException("Invalid row range for partition.");
            }
            if (offsetLeft < 0 || offsetLeft + nColumns > data[0].Length)
            {
                throw new ArgumentOutOfRangeException("Invalid column range for partition.");
            }

            // Create a new jagged array to hold the partition
            double[][] partition = new double[nRows][];

            // Copy the selected portion of the data into the new partition
            for (int i = 0; i < nRows; i++)
            {
                partition[i] = new double[nColumns];
                Array.Copy(data[offsetTop + i], offsetLeft, partition[i], 0, nColumns);
            }

            return partition;
        }

        public void DumpCached(bool flush)
        {
            if (flush)
            {
                foreach (var block in _BlockCache.Values.Select(v => v.Value))
                {
                    Flush(block);
                }
            }
            _BlockCache.Clear();
            _LRUList.Clear();
        }
    }
}
