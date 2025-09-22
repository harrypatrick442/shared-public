using Core.Maths.Tensors.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Value = Core.Maths.Tensors.ThreadSafeSparseJaggedDoubleMatrixCache_Value;
namespace Core.Maths.Tensors
{
    public class ThreadSafeSparseJaggedDoubleMatrixCache : IAbstractEncapsulatedJaggedDoubleMatrixWriteOnly
    {
        protected Dictionary<int, Dictionary<int, Value>>
            _MapRowColumnToValue = new Dictionary<int, Dictionary<int, Value>>();
        protected HashSet<Value> _Values = new HashSet<Value>();
        private int _NRows;
        private int _NColumns;
        public double[][] GetAllValues()
        {
            double[][] matrix = MatrixHelper.Create(_NRows, _NColumns);
            foreach (var rowAndMapColumnToValue in _MapRowColumnToValue) {
                foreach (var columnValuePair in rowAndMapColumnToValue.Value)
                {
                    int row = rowAndMapColumnToValue.Key;
                    int column = columnValuePair.Key;
                    Value value = columnValuePair.Value;
                    if (row != value.RowIndex) {
                        throw new Exception("Row mismatch");
                    }
                    if (column != value.ColumnIndex) {
                        throw new Exception("Column mismatch");
                    }
                    matrix[row][column] = value.Value;
                }
            }
            return matrix;
        }
        public ThreadSafeSparseJaggedDoubleMatrixCache(int nRows, int nColumns)
        {
            _NRows = nRows;
            _NColumns = nColumns;
        }
        public void GetChangedValues(Action<IEnumerable<IMatrixIndexValue>> callback) {
            lock (_MapRowColumnToValue) {
                callback(_Values.Where(v => v.Changed));
            }
        }
        public void CacheOldValues()
        {
            lock (_MapRowColumnToValue)
            {
                foreach (var value in _Values)
                {
                    value.CacheOldValues();
                }
            }
        }
        public void Clear()
        {
            lock (_MapRowColumnToValue)
            {
                foreach (var value in _Values)
                {
                    value.Clear();
                }
            }
        }

        public void Decrement(int row, int column, double value)
        {
            if (value == 0) return;
            lock (_MapRowColumnToValue)
            {
                GetValue(row, column).Decrement(value);
            }
        }

        public void Increment(int row, int column, double value)
        {
            if (value == 0) return;
            lock (_MapRowColumnToValue)
            {
                GetValue(row, column).Increment(value);
            }
        }
        private Value GetValue(int rowIndex, int columnIndex) {

            int rowMajorIndex = rowIndex * _NColumns + columnIndex;
            int columnMajorIndex = columnIndex * _NRows + rowIndex;
            if (!_MapRowColumnToValue.TryGetValue(rowIndex, 
                out Dictionary<int, Value>? row)) {
                var newValue = new Value(rowIndex, columnIndex, rowMajorIndex, columnMajorIndex);
                _Values.Add(newValue);
                row = new Dictionary<int, Value> { { columnIndex,  newValue} };
                _MapRowColumnToValue[rowIndex] = row;
                return newValue;
            }
            if (!row.TryGetValue(columnIndex, out Value? value)) {
                value = new Value(rowIndex, columnIndex, rowMajorIndex, columnMajorIndex);
                _Values.Add(value);
                row[columnIndex] = value;
            }
            return value;
        }
    }
}
