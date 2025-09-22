using Core.Maths.Tensors.Interfaces;

namespace Core.Maths.Tensors
{
    public class ThreadSafeSparseDoubleVectorCache_Value : IIndexValue
    {
        private double _OldValueBeforeClear;
        public double Value { get; protected set; }
        public bool Changed => Value != _OldValueBeforeClear;
        public int Index{get;}
        public ThreadSafeSparseDoubleVectorCache_Value(int index)
        {
            Index = index;
        }

        public void Clear()
        {
            Value = 0;
        }
        public void CacheOldValue()
        {
            _OldValueBeforeClear = Value;
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
