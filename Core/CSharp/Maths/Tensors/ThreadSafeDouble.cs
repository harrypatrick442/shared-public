using System;

namespace Core.Maths.Tensors
{
    public class ThreadSafeDouble
    {
        private double _OldValue;
        private double _Value = 0;
        public double TakeUnsafe()
        {
            lock (this)
            {
                return _Value;
            }
        }
        public void CacheOldValue() {
            lock (this) {
                _OldValue = _Value;
            }
        }
        public void Clear() {
            lock (this)
            {
                _Value = 0;
            }
        }
        public void Increment(double value)
        {
            lock (this)
            {
                _Value += value;
            }
        }
        public void Decrement(double value)
        {
            lock (this)
            {
                _Value -= value;
            }
        }

        public static ThreadSafeDouble operator -(ThreadSafeDouble d, double value)
        {
            lock (d)
            {
                d._Value -= value;
            }
            return d;
        }
    }
}
