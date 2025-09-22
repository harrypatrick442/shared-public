namespace Core.Maths.Tensors.Interfaces
{
    public interface IAbstractEncapsulatedJaggedDoubleMatrixWriteOnly
    {
        void CacheOldValues();
        void Clear();
        void Decrement(int row, int column, double value);
        void Increment(int row, int column, double value);
    }
}