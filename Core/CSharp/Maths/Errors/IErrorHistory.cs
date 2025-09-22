namespace Core.Maths.Errors
{
    public interface IErrorHistory
    {
        public double Value { get; }
        public void Add(double error);
        public void Clear();
    }
}