using Core.Maths.Tensors.Interfaces;

namespace Core.Maths.Tensors
{
    public class ThreadSafeSparseJaggedDoubleMatrixCache_Value : IMatrixIndexValue
    {
        private double _CurrentValueCached;
        public double Value { get; protected set; }
        public bool Changed => Value != _CurrentValueCached;
        public int RowMajorIndex{get;}
        public int ColumnMajorIndex { get;}
        public int RowIndex { get; }
        public int ColumnIndex { get; }
        public ThreadSafeSparseJaggedDoubleMatrixCache_Value(
            int rowIndex, int columnIndex,
            int rowMajorIndex, int columnMajorIndex)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            RowMajorIndex = rowMajorIndex;
            ColumnMajorIndex = columnMajorIndex;
        }

        public void Clear()
        {
            Value = 0;
        }
        public void CacheOldValues()
        {
            _CurrentValueCached = Value;
        }
        public void Increment(double value)
        {
            Value += value;
        }
        public void Decrement(double value)
        {
            Value -= value;
        }
    }
}
