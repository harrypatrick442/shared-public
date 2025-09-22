using System;

namespace Core.Maths.IterativeSolvers.NewtonRaphson
{
    /// <summary>
    /// For solving non linear
    /// </summary>
    public class NewtonRaphsonStoppingParametersSingle
    {
        private const double DEFAULT_ABSOLUTE_TOLERANCE = 1e-10,
            DEFAULT_DAMPING_FACTOR = 0.5d,
            DEFAULT_DAMPING_FACTOR_SHRINK_FACTOR = 0.1d,
            DEFAULT_DAMPING_FACTOR_GROWTH_FACTOR = 10d,
            DEFUALT_MIN_DAMPING_FACTOR = 1e-12,
            DEFUALT_MAX_DAMPING_FACTOR = 0.5d,
            DEFAULT_MIN_SLOPE = 1e-12;
        private const int DEFAULT_ITERATION_LIMIT = 10000;
        public static readonly NewtonRaphsonStoppingParametersSingle Default =
            new NewtonRaphsonStoppingParametersSingle(
                absoluteTolerance: DEFAULT_ABSOLUTE_TOLERANCE,
                iterationLimit: DEFAULT_ITERATION_LIMIT,
                dampingFactor: DEFAULT_DAMPING_FACTOR,
                dynamicDampingFactor: true,
                dampingFactorShrinkFactor: DEFAULT_DAMPING_FACTOR_SHRINK_FACTOR,
                dampingFactorGrowthFactor: DEFAULT_DAMPING_FACTOR_GROWTH_FACTOR
            );
        public double AbsoluteTolerance { get; }
        public int? IterationLimit { get; }
        public double DampingFactor { get; }
        public bool DynamicDampingFactor { get; }
        public double DampingFactorShrinkFactor { get; }
        public double DampingFactorGrowthFactor { get; }
        public double? MinDampingFactor { get; }
        public double? MaxDampingFactor { get; }
        public NewtonRaphsonStoppingParametersSingle(
            double absoluteTolerance = DEFAULT_ABSOLUTE_TOLERANCE,
            int? iterationLimit = DEFAULT_ITERATION_LIMIT,
            double dampingFactor = DEFAULT_DAMPING_FACTOR,
            bool dynamicDampingFactor = true,
            double dampingFactorShrinkFactor = DEFAULT_DAMPING_FACTOR_SHRINK_FACTOR,
            double dampingFactorGrowthFactor = DEFAULT_DAMPING_FACTOR_GROWTH_FACTOR,
            double? minDampingFactor = DEFUALT_MIN_DAMPING_FACTOR,
            double? maxDampingFactor = DEFUALT_MAX_DAMPING_FACTOR,
            double minSlope = DEFAULT_MIN_SLOPE)
        {
            if (absoluteTolerance < 0)
                throw new ArgumentException(nameof(absoluteTolerance));
            if (iterationLimit < 0)
                throw new ArgumentException(nameof(iterationLimit));
            if (dampingFactor <= 0)
            {
                throw new ArgumentException($"{nameof(dampingFactor)} must be greater than 0");
            }
            if (dynamicDampingFactor)
            {
                if (dampingFactorShrinkFactor <= 0)
                {
                    throw new ArgumentException($"{nameof(dampingFactorShrinkFactor)} must be greater than 0 for {nameof(dynamicDampingFactor)} = true");
                }
                if (dampingFactorShrinkFactor >= 1)
                {
                    throw new ArgumentException($"{nameof(dampingFactorShrinkFactor)} must be less than 1 for {nameof(dynamicDampingFactor)} = true");
                }
                if (dampingFactorGrowthFactor <= 1)
                {
                    throw new ArgumentException($"{nameof(dampingFactorGrowthFactor)} must be greater than 1 for {nameof(dynamicDampingFactor)} = true");
                }
                if (minDampingFactor != null)
                {
                    if ((double)minDampingFactor <= 0)
                    {
                        throw new ArgumentException($"{nameof(minDampingFactor)} must be greater than 0 for {nameof(dynamicDampingFactor)} = true and {nameof(minDampingFactor)} != null");
                    }
                    if ((double)minDampingFactor > dampingFactor)
                    {
                        throw new ArgumentException($"{nameof(minDampingFactor)} must be less or equal to {nameof(dampingFactor)}: {dampingFactor} for {nameof(dynamicDampingFactor)} = true and {nameof(minDampingFactor)} != null");
                    }
                }
                if (maxDampingFactor != null)
                {
                    if ((double)maxDampingFactor > 1)
                    {
                        throw new ArgumentException($"{nameof(maxDampingFactor)} must be less or equal to 1 for {nameof(dynamicDampingFactor)} = true and {nameof(maxDampingFactor)} != null");
                    }
                    if (minDampingFactor != null)
                    {
                        if ((double)maxDampingFactor < (double)minDampingFactor)
                        {
                            throw new ArgumentException($"{nameof(maxDampingFactor)} must be greater or equal to {nameof(minDampingFactor)} for {nameof(dynamicDampingFactor)} = true and {nameof(minDampingFactor)} != null and {nameof(maxDampingFactor)} != null");
                        }
                    }
                }
            }
            AbsoluteTolerance = absoluteTolerance;
            IterationLimit = iterationLimit;
            DampingFactor = dampingFactor;
            DynamicDampingFactor = dynamicDampingFactor;
            DampingFactorShrinkFactor = dampingFactorShrinkFactor;
            DampingFactorGrowthFactor = dampingFactorGrowthFactor;
            MinDampingFactor = minDampingFactor;
            MaxDampingFactor = maxDampingFactor;
        }
    }
}