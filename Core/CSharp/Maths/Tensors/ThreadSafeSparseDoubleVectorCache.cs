using Core.Maths.Tensors.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Value = Core.Maths.Tensors.ThreadSafeSparseDoubleVectorCache_Value;
namespace Core.Maths.Tensors
{
    public class ThreadSafeSparseDoubleVectorCache: IAbstractEncapsulatedDoubleVectorWriteOnly
    {
        protected Dictionary<int, Value> 
            _MapIndexToValue = new Dictionary<int, Value>();
        private int _Index;
        public ThreadSafeSparseDoubleVectorCache(int index)
        {
            _Index = index;
        }
        public double[] GetAllValues()
        {
            double[] vector = new double[_Index];
            foreach (var indexValuePair in _MapIndexToValue)
            {
                int index = indexValuePair.Key;
                Value value = indexValuePair.Value;
                vector[index] = value.Value;
            }
            return vector;
        }
        public void GetChangedValues(Action<IEnumerable<IIndexValue>> callback)
        {
            lock (_MapIndexToValue)
            {
                callback(_MapIndexToValue.Values.Where(v => v.Changed));
            }
        }

        public void Clear()
        {
            lock (_MapIndexToValue)
            {
                foreach (var value in _MapIndexToValue.Values)
                {
                    value.Clear();
                }
            }
        }
        public void CacheOldValues()
        {
            lock (_MapIndexToValue)
            {
                foreach (var value in _MapIndexToValue.Values)
                {
                    value.CacheOldValue();
                }
            }
        }

        public void Decrement(int index, double value)
        {
            if (value == 0) return;
            lock (_MapIndexToValue)
            {
                GetValue(index).Decrement(value);
            }
        }

        public void Increment(int index, double value)
        {
            if (value == 0) return;
            lock (_MapIndexToValue)
            {
                GetValue(index).Increment(value);
            }
        }
        private Value GetValue(int index) {
            if (_MapIndexToValue.TryGetValue(index, out Value? value)) {
                return value;
            }
            var newValue = new Value(index);
            _MapIndexToValue[index] = newValue;
            return newValue;
        }
    }
}
