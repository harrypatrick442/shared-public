using Core.Maths.Tensors.Interfaces;
using System;
using System.Collections.Generic;

namespace Core.Maths.Tensors
{
    public class DynamicResizingRowsColumnsValuesBuffer: IRowsColumnsValuesBuffer
    {
        public int[] Indices { get; protected set; }
        public int[] Columns { get; protected set; }
        public double[] Values { get; protected set; }
        private int _NextIndex;
        public int Length => _NextIndex;
        public DynamicResizingRowsColumnsValuesBuffer(int initialLength) { 
            Indices = new int[initialLength];
            Columns = new int[initialLength];
            Values = new double[initialLength];
        }
        public void Clear() {
            _NextIndex = 0;
        }
        public void AddRange(IEnumerable<IRowColumnValue> entries) { 
            foreach(var entry in entries)
            {
                Add(entry.Row, entry.Column, entry.Value);
            }
        }
        public void Add(int row, int column, double value)
        {
            if(_NextIndex>=Indices.Length)
            {
                IncreaseSize();
            }
            Indices[_NextIndex] = row;
            Columns[_NextIndex] = column;
            Values[_NextIndex] = value;
            _NextIndex++;
        }
        private void IncreaseSize() {
            int[] currentRows = Indices;
            int[] currentColumns = Columns;
            double[] currentValues = Values;
            int currentLength = currentRows.Length;
            int newLength = currentLength * 2;
            Indices = new int[newLength];
            Columns = new int[newLength];
            Values = new double[newLength];
            Array.Copy(currentRows, 0, Indices, 0, currentLength);
            Array.Copy(currentColumns, 0, Columns, 0, currentLength);
            Array.Copy(currentValues, 0, Values, 0, currentLength);
        }
    }
}