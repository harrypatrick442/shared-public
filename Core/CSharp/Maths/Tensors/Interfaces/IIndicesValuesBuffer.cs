namespace Core.Maths.Tensors
{
    public interface IIndicesValuesBuffer
    {
        public int[] Indices { get; }
        public double[] Values { get; }
        public int Length { get; }
    }
}