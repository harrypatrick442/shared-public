using Core.Maths.Tensors.Interfaces;
using System;
namespace Core.Maths.Tensors
{
    public class ThreadSafeDoubleVector: IAbstractEncapsulatedDoubleVectorWriteOnly
    {
        private readonly ThreadSafeDouble[] _Vector;
        private int _Size;
        public ThreadSafeDoubleVector(int size)
        {
            _Size = size;
            _Vector = new ThreadSafeDouble[size];
            for (int i = 0; i < size; i++)
            {
                _Vector[i] = new ThreadSafeDouble();
            }
        }

        public void CacheOldValues()
        {
            foreach (var v in _Vector)
                v.CacheOldValue();
        }

        public void Clear() {
            foreach (var v in _Vector)
                v.Clear();
        }

        public void Decrement(int index, double value)
        {
            _Vector[index].Decrement(value);
        }

        public void Increment(int index, double value)
        {
            _Vector[index].Increment(value);
        }

        public double[] TakeUnsafe()
        {
            double[] values = new double[_Size];
            for (int i = 0; i < _Size; i++)
            {
                values[i] = _Vector[i].TakeUnsafe();
            }
            return values;
        }

        public double TakeUnsafe(int index)
        {
            return _Vector[index].TakeUnsafe();
        }
    }
}
