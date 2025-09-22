using Core.Maths.Tensors.Interfaces;
using System;
using System.Collections.Generic;

namespace Core.Maths.Tensors
{
    public class DynamicResizingIndexValuesBuffer : IIndicesValuesBuffer
    {
        public int[] Indices { get; protected set; }
        public double[] Values { get; protected set; }
        private int _NextIndex;
        public int Length => _NextIndex;
        public DynamicResizingIndexValuesBuffer(int initialLength) { 
            Indices = new int[initialLength];
            Values = new double[initialLength];
        }
        public void Clear()
        {
            _NextIndex = 0;
        }
        public void AddRange(IEnumerable<IIndexValue> entries) { 
            foreach(var entry in entries)
            {
                Add(entry.Index, entry.Value);
            }
        }
        public void AddRangeRowMajor(IEnumerable<IMatrixIndexValue> entries) { 
            foreach(var entry in entries)
            {
                Add(entry.RowMajorIndex, entry.Value);
            }
        }
        public void AddRangeColumnMajor(IEnumerable<IMatrixIndexValue> entries)
        {
            foreach (var entry in entries)
            {
                Add(entry.ColumnMajorIndex, entry.Value);
            }
        }
        public void Add(int index, double value)
        {
            if(_NextIndex>=Indices.Length)
            {
                IncreaseSize();
            }
            Indices[_NextIndex] = index;
            Values[_NextIndex] = value;
            _NextIndex++;
        }
        private void IncreaseSize() {
            int[] currentIndices = Indices;
            double[] currentValues = Values;
            int currentLength = currentIndices.Length;
            int newLength = currentLength * 2;
            Indices = new int[newLength];
            Values = new double[newLength];
            Array.Copy(currentIndices, 0, Indices, 0, currentLength);
            Array.Copy(currentValues, 0, Values, 0, currentLength);
        }
    }
}