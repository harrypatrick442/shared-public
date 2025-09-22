using Core.FileSystem;
using Core.Maths.Core.Maths;
using Core.MemoryManagement;
using Shutdown;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Core.Maths.Matrices
{
    public class RowBlockMatrix_ : IDisposable
    {
        public int NRows { get; }
        public int NColumns { get; }
        public int NRowBlocks { get; }
        private bool _Disposed = false;
        private int _NRowsPerBlockExcludingLast;
        private RowBlockMatrix_RowBlock[] _RowBlocks;
        private LinkedList<RowBlockMatrix_RowBlock> _LoadedLatestAccessedLast = new LinkedList<RowBlockMatrix_RowBlock>();
        public RowBlockMatrix_(int nRows, int nColumns, int nRowBlocks, string directoryPath)
        {
            Directory.CreateDirectory(directoryPath);
            NRows = nRows;
            NColumns = nColumns;
            NRowBlocks = nRowBlocks;
            _RowBlocks = new RowBlockMatrix_RowBlock[nRowBlocks];
            _NRowsPerBlockExcludingLast = nRows / nRowBlocks;
            int i = 0;
            while (i < nRowBlocks)
            {
                string filePath = Path.Combine(directoryPath, $"b_{i}.bin");
                _RowBlocks[i] = new RowBlockMatrix_RowBlock(
                    i,
                    nRows: i >= nRowBlocks - 1 ? nRows - i * _NRowsPerBlockExcludingLast : _NRowsPerBlockExcludingLast,
                    nColumns,
                    filePath
                );
                i++;
            }
        }
        public double[] MultiplyByVector(IList<double> v)
        {
            if (_Disposed) throw new ObjectDisposedException(nameof(BlockMatrix));
            if (NColumns != v.Count)
            {
                throw new ArgumentException("Matrix column count must match vector length.");
            }
            int nRowBlock = 0;
            int rowIndex = 0;
            double[] result = new double[NColumns];
            while (nRowBlock < NRows)
            {

                RowBlockMatrix_RowBlock rowBlock = GetLoadedRowBlock_IfLoadedMoveToEndOfLatestAccessedLast(nRowBlock);
                double[][] data = rowBlock.Data;
                for (int i = 0; i < rowBlock.NRows; i++)
                {
                    double r = 0;
                    for (int j = 0; j < NColumns; j++)
                    {
                        r += data[i][j] * v[j];
                    }
                    result[rowIndex++] = r;
                }
            }
            return result;
        }
        public void OperateOnRow(int rowIndex, Action<double[]> callback)
        {
            int nRowBlock = rowIndex / _NRowsPerBlockExcludingLast;
            int indexInBlock = rowIndex - nRowBlock * _NRowsPerBlockExcludingLast;
            RowBlockMatrix_RowBlock rowBlock = GetLoadedRowBlock_IfLoadedMoveToEndOfLatestAccessedLast(nRowBlock);
            double[] row = rowBlock.Data[indexInBlock];
            callback(row);
            rowBlock.RequiresSaveOnUnload = true;
        }
        public MathNet.Numerics.LinearAlgebra.Vector<double> MultiplyByVectorToMathNet(IList<double> vector)
        {
            return MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(MultiplyByVector(vector));
        }
        public static double[] operator *(RowBlockMatrix_ matrix, double[] vector)
        {
            return matrix.MultiplyByVector(vector);
        }
        public static double[] operator *(RowBlockMatrix_ matrix, MathNet.Numerics.LinearAlgebra.Vector<double> vector)
        {
            return matrix.MultiplyByVector(vector);
        }
        private void MoveRowToEndOfLatestAccessedLast(RowBlockMatrix_RowBlock rowBlock)
        {
            _LoadedLatestAccessedLast.Remove(rowBlock.LinkedListNode);
            _LoadedLatestAccessedLast.AddLast(rowBlock);
        }
        private RowBlockMatrix_RowBlock GetLoadedRowBlock_IfLoadedMoveToEndOfLatestAccessedLast(int nRowBlock)
        {
            var rowBlock = _RowBlocks[nRowBlock];
            if (rowBlock.Loaded)
            {
                MoveRowToEndOfLatestAccessedLast(rowBlock);
                return rowBlock;
            }
            var lastAccessedRowBlockNode = _LoadedLatestAccessedLast.First;
            if (lastAccessedRowBlockNode != null)
            {
                lastAccessedRowBlockNode.Value.Unload();
                _LoadedLatestAccessedLast.Remove(lastAccessedRowBlockNode);
            }
            rowBlock.Load();
            rowBlock.LinkedListNode = _LoadedLatestAccessedLast.AddLast(rowBlock);
            return rowBlock;
        }

        public void Dispose()
        {
            foreach (RowBlockMatrix_RowBlock rowBlock in _LoadedLatestAccessedLast)
            {
                rowBlock.Dispose();
            }
            _Disposed = true;
        }
        public static RowBlockMatrix_ Create(int nRows, int nColumns, double roughProportionFreeMemoryTake, string directoryPath)
        {
            var memoryMetrics = MemoryHelper.GetMemoryMetricsNow();
            long totalMemoryToTake = (long)(memoryMetrics.FreeMb * 1000000d * roughProportionFreeMemoryTake);
            long totalData = nRows * (nColumns * sizeof(double) + nint.Size);
            double nRowBlocksRequired = totalData / totalMemoryToTake;
            int nRowBlocks = (int)Math.Ceiling(nRowBlocksRequired);
            if (nRowBlocks < 1) nRowBlocks = 1;
            return new RowBlockMatrix_(nRows, nColumns, nRowBlocks, directoryPath);
        }
        public static RowBlockMatrix_ ForMaximumMemoryFootprintPerBlock(int nRows, int nColumns, long bytes, string directoryPath)
        {
            long totalData = nRows * (nColumns * sizeof(double) + nint.Size);
            double nRowBlocksRequired = totalData / bytes;
            int nRowBlocks = (int)Math.Ceiling(nRowBlocksRequired);
            if (nRowBlocks < 1) nRowBlocks = 1;
            return new RowBlockMatrix_(nRows, nColumns, nRowBlocks, directoryPath);
        }
    }
}
