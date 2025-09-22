using System;

namespace Core.Maths.Tolerances
{
    public class AbsoluteTolerance
    {
        public double TargetTolerance { get; }
        public double DesiredMaxTolerance { get; }
        public double MaxTolerance { get; }
        public double Weight { get; }
        public AbsoluteTolerance(double targetTolerance, 
            double maxTolerance, double desiredMaxTolerance, double weight)
        {
            if (maxTolerance < 0)
                throw new ArgumentException($"{nameof(maxTolerance)} must be greater or equal to zero");
            if (desiredMaxTolerance < 0)
                throw new ArgumentException($"{nameof(maxTolerance)} must be greater or equal to zero");
            if (targetTolerance < 0)
                throw new ArgumentException($"{nameof(targetTolerance)} must be greater or equal to zero");
            if (targetTolerance > maxTolerance)
                throw new ArgumentException($"{nameof(targetTolerance)} must be less or equal to {nameof(maxTolerance)}");
            if (desiredMaxTolerance > maxTolerance)
                throw new ArgumentException($"{nameof(desiredMaxTolerance)} must be less or equal to {nameof(maxTolerance)}");
            if (targetTolerance > desiredMaxTolerance)
                throw new ArgumentException($"{nameof(targetTolerance)} must be less or equal to {nameof(desiredMaxTolerance)}");
            TargetTolerance = targetTolerance;
            DesiredMaxTolerance = desiredMaxTolerance;
            MaxTolerance = maxTolerance;
            Weight = weight;
        }
    }
}
