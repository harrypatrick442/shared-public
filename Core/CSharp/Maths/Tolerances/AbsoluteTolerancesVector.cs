using Core.Maths.Matrices;
using System;

namespace Core.Maths.Tolerances
{
    public class AbsoluteTolerancesVector
    {
        public int Length { get; }
        public double[] TargetTolerances { get; }
        public double[] DesiredMaxTolerances { get; }
        public double[] MaxTolerances { get; }
        public double[] Weights { get; }
        public AbsoluteTolerancesVector(double[] targetTolerances,
            double[] maxTolerances, double[] desiredMaxTolerances, double[] weights)
        {
            Length = targetTolerances.Length;
            if (Length != maxTolerances.Length)
                throw new ArgumentException($"{nameof(maxTolerances)}.{nameof(maxTolerances.Length)} was different to {nameof(targetTolerances)}.{nameof(targetTolerances.Length)}");

            if (Length != desiredMaxTolerances.Length)
                throw new ArgumentException($"{nameof(desiredMaxTolerances)}.{nameof(desiredMaxTolerances.Length)} was different to {nameof(targetTolerances)}.{nameof(targetTolerances.Length)}");

            if (Length != weights.Length)
                throw new ArgumentException($"{nameof(weights)}.{nameof(weights.Length)} was different to {nameof(targetTolerances)}.{nameof(targetTolerances.Length)}");

            TargetTolerances = targetTolerances;
            DesiredMaxTolerances = desiredMaxTolerances;
            MaxTolerances = maxTolerances;
            Weights = weights;
        }
        public AbsoluteTolerancesVector ToConditioned(ConditionedSystem conditionedSystem)
        {
                return new AbsoluteTolerancesVector(
                targetTolerances: conditionedSystem.RowScaleVector(
                    this.TargetTolerances),
                desiredMaxTolerances: conditionedSystem.RowScaleVector(
                    this.DesiredMaxTolerances),
                maxTolerances: conditionedSystem.RowScaleVector(
                    this.MaxTolerances),
                weights: this.Weights
            );
        }
    }
}
