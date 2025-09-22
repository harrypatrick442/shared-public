using System;
using System.Collections.Generic;
using Core.Maths.Matrices;

namespace Core.Maths
{
    public class SimpleBigMatrix: IBigMatrix
    {
        private readonly double[][] _matrix;
        public double[][] Data { get { return _matrix; } }
        public int NRows { get; }
        public int NColumns { get; }

        public double ProportionOfMatrixInMemory => 1;

        public double ProportionOfMaxCacheSizeUsed => 0;

        public SimpleBigMatrix(int nRows, int nColumns)
        {
            NRows = nRows;
            NColumns = nColumns;
            _matrix = new double[nRows][];

            // Initialize the matrix
            for (int i = 0; i < nRows; i++)
            {
                _matrix[i] = new double[nColumns];
            }
        }

        // First level indexer to access rows
        public double[] this[int rowIndex]
        {
            get
            {
                ValidateRowIndex(rowIndex);
                return _matrix[rowIndex];
            }
            set
            {
                ValidateRowIndex(rowIndex);
                if (value.Length != NColumns)
                    throw new ArgumentException($"RowIndex length must match the number of columns ({NColumns}).");
                _matrix[rowIndex] = value;
            }
        }

        // Second level indexer to access individual elements
        public double this[int rowIndex, int columnIndex]
        {
            get
            {
                ValidateIndices(rowIndex, columnIndex);
                return _matrix[rowIndex][columnIndex];
            }
            set
            {
                ValidateIndices(rowIndex, columnIndex);
                _matrix[rowIndex][columnIndex] = value;
            }
        }

        public double[] MultiplyByVector(IList<double> v)
        {
            if (v.Count != NColumns)
                throw new ArgumentException($"Vector length must match the number of columns ({NColumns}).");

            double[] result = new double[NRows];

            for (int rowIndex = 0; rowIndex < NRows; rowIndex++)
            {
                double sum = 0;
                for (int colIndex = 0; colIndex < NColumns; colIndex++)
                {
                    sum += _matrix[rowIndex][colIndex] * v[colIndex];
                }
                result[rowIndex] = sum;
            }

            return result;
        }

        public MathNet.Numerics.LinearAlgebra.Vector<double> MultiplyByVectorToMathNet(IList<double> vector)
        {
            return MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(MultiplyByVector(vector));
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

        public void Dispose()
        {

        }

        public double[] ReadRow(int rowIndex)
        {
            return _matrix[rowIndex];
        }

        public void WriteRow(int rowIndex, double[] values)
        {
            _matrix[rowIndex] = values;
        }
        public byte[] ReadBlockBytes(int nRows, int nColumns, int offsetTop, int offsetLeft)
        {
            // Validate the requested block size and offsets
            if (nRows <= 0 || nColumns <= 0)
                throw new ArgumentException("Number of rows and columns must be positive.");
            if (offsetTop < 0 || offsetLeft < 0)
                throw new ArgumentOutOfRangeException("Offsets must be non-negative.");
            if (offsetTop + nRows > NRows || offsetLeft + nColumns > NColumns)
                throw new ArgumentException("Requested block exceeds _Matrix dimensions.");

            // Calculate the size of the byte array
            byte[] blockBytes = new byte[nRows * nColumns * sizeof(double)];

            // Copy the matrix block to the byte array manually
            int byteIndex = 0;

            for (int row = 0; row < nRows; row++)
            {
                for (int col = 0; col < nColumns; col++)
                {
                    // Get the value from the matrix
                    double value = _matrix[offsetTop + row][offsetLeft + col];

                    // Convert the double value to bytes
                    byte[] bytes = BitConverter.GetBytes(value);

                    // Copy the bytes to the blockBytes array
                    for (int b = 0; b < sizeof(double); b++)
                    {
                        blockBytes[byteIndex++] = bytes[b];
                    }
                }
            }

            return blockBytes;
        }



        public void DumpCached(bool flush)
        {

        }
    }
}
