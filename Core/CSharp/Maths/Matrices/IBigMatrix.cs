using System;
using System.Collections.Generic;

namespace Core.Maths.Matrices
{
    public interface IBigMatrix : IDisposable
    {

        public double[][] Data { get; }
        int NRows { get; }
        int NColumns { get; }
        public double[] ReadRow(int rowIndex);
        public void WriteRow(int rowIndex, double[] values);

        // Indexer to get/set individual elements
        public double this[int rowIndex, int columnIndex] { get; set; }

        // Method to multiply the matrix by a vector
        public double[] MultiplyByVector(IList<double> v);
        public byte[] ReadBlockBytes(int nRows, int nColumns, int offsetTop, int offsetLeft);
        public MathNet.Numerics.LinearAlgebra.Vector<double> MultiplyByVectorToMathNet(IList<double> vector);
        public void DumpCached(bool flush);

        public double ProportionOfMatrixInMemory
        {
            get;
        }
        public double ProportionOfMaxCacheSizeUsed
        {
            get;
        }
    }
}
