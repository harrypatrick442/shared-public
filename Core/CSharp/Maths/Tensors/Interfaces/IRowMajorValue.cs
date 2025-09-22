namespace Core.Maths.Tensors.Interfaces
{
    public interface IMatrixIndexValue
    {
        public int RowMajorIndex { get; }
        public int ColumnMajorIndex { get; }
        public double Value { get; }
    }
}