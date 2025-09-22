namespace Core.Maths.Tensors
{
    public interface IRowsColumnsValuesBuffer
    {
        public int[] Indices { get; }
        public int[] Columns { get; }
        public double[] Values { get; }
        public int Length { get; }
    }
}