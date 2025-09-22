namespace Core.Maths.Tensors.Interfaces
{
    public interface IAbstractEncapsulatedDoubleVectorWriteOnly
    {
        void CacheOldValues();
        void Clear();
        void Decrement(int index, double value);
        void Increment(int index, double value);
    }
}