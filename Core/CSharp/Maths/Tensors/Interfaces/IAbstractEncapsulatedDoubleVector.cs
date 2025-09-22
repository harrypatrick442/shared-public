namespace Core.Maths.Tensors.Interfaces
{
    public interface IAbstractEncapsulatedDoubleVector : IAbstractEncapsulatedJaggedDoubleMatrixWriteOnly
    {
        double Read(int index);
    }
}