namespace Core.Maths.IterativeSolvers.NewtonRaphson
{
    public class RowError
    {
        public int Index { get; private set; }
        public double NormalizedError { get; private set; }
        public RowError(int index, double normalizedError)
        {
            Index = index;
            NormalizedError = normalizedError;
        }
    }
}
