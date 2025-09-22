namespace Core.Maths.IterativeSolvers.NewtonRaphson
{
    internal class SumSquaresMaxAndTarget
    {
        public double Max { get; private set; }
        public double Target { get; private set; }
        public int NEntries { get; private set; }
        public SumSquaresMaxAndTarget()
        {

        }
        public void Add(double max, double target)
        {
            Max += max;
            Target += target;
            NEntries++;
        }
        public void Clear()
        {
            Max = 0;
            Target = 0;
            NEntries = 0;
        }
    }
}
