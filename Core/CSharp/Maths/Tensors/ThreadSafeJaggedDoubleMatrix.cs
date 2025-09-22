using Core.Maths.Tensors.Interfaces;

namespace Core.Maths.Tensors
{
    public class ThreadSafeJaggedDoubleMatrix : IAbstractEncapsulatedJaggedDoubleMatrix
    {
        private readonly int _Rows;
        private readonly ThreadSafeDoubleVector[] _Matrix;
        public ThreadSafeJaggedDoubleMatrix(int rows, int cols)
        {
            _Rows = rows;
            _Matrix = new ThreadSafeDoubleVector[rows];
            for (int i = 0; i < rows; i++)
            {
                _Matrix[i] = new ThreadSafeDoubleVector(cols);
            }
        }
        public void Increment(int row, int column, double value)
        {
            _Matrix[row].Increment(column, value);
        }
        public void Decrement(int row, int column, double value)
        {
            if (row == 5 && column == 0)
            {

            }
            _Matrix[row].Decrement(column,  value);
        }
        public double Read(int row, int column)
        {
            return _Matrix[row].TakeUnsafe(column);
        }
        /*
        public ThreadSafeDoubleVector this[int index]
        {
            get
            {
                return _Matrix[index];
            }
        }*/
        public void CacheOldValues()
        {
            foreach (var row in _Matrix)
            {
                row.CacheOldValues();
            }
        }
        public void Clear()
        {
            foreach (var row in _Matrix)
            {
                row.Clear();
            }
        }
        public double[][] TakeUnsafe()
        {
            double[][] matrix = new double[_Rows][];
            for (int i = 0; i < _Rows; i++)
            {
                matrix[i] = _Matrix[i].TakeUnsafe();
            }
            return matrix;
        }
    }
}
