namespace Core.Maths.Tensors.Interfaces
{
    public interface IAbstractEncapsulatedJaggedDoubleMatrix : IAbstractEncapsulatedJaggedDoubleMatrixWriteOnly
    {
        double Read(int row, int column);
    }
}