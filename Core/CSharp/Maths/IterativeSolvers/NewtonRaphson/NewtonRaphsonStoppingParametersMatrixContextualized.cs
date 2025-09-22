using System;

namespace Core.Maths.IterativeSolvers.NewtonRaphson
{
    /// <summary>
    /// For solving non-linear
    /// </summary>
    public class NewtonRaphsonStoppingParametersMatrixContextualized
    {
        public double[] MaximumRelativeTolerances { get; }
        public double[] MaximumResidualTolerances { get; }
        public double[] DesiredMaximumRelativeTolerances { get; }
        public double[] DesiredMaximumResidualTolerances { get; }
        public double[] TargetRelativeTolerances { get; }
        public double[] TargetResidualTolerances { get; }
        public double[] RelativeToleranceWeights { get; }
        public double[] ResidualToleranceWeights { get; }
        public int? MaxConvergenceSteps { get; }
        public bool FailIfExceedsMaxConvergenceSteps { get; }

        // Newly added properties
        public double[] ResidualRelativeMagnitudeTolerances { get; }
        public double[] ResidualRelativeChangeTolerances { get; }
        public double[] ResidualCurrentOverOldestStagnationThresholds { get; }
        public int StagnationWindowSize { get; }
        public double[] StepRelativeMagnitudeTolerances { get; }
        public double[] StepRelativeChangeTolerances { get; }
        public double[] StepCurrentOverOldestStagnationThresholds { get; }

        public NewtonRaphsonStoppingParametersMatrixContextualized(
            double[] maximumRelativeTolerances,
            double[] desiredMaximumRelativeTolerances,
            double[] targetRelativeTolerances,
            double[] maximumResidualTolerances,
            double[] desiredMaximumResidualTolerances,
            double[] targetResidualTolerances,
            double[] relativeToleranceWeights,
            double[] residualToleranceWeights,
            int? maxConvergenceSteps,
            bool failIfExceedsMaxConvergenceSteps,
            double[] residualRelativeMagnitudeTolerances,
            double[] residualRelativeChangeTolerances,
            double[] residualCurrentOverOldestStagnationThresholds,
            int stagnationWindowSize,
            double[] stepRelativeMagnitudeTolerances,
            double[] stepRelativeChangeTolerances,
            double[] stepCurrentOverOldestStagnationThresholds)
        {
            int length = maximumRelativeTolerances.Length;
            for (int i = 0; i < length; i++)
            {
                if (maximumRelativeTolerances[i] <= 0)
                    throw new ArgumentException($"{nameof(maximumRelativeTolerances)}[{i}] must be greater than zero");
                if (maximumResidualTolerances[i] <= 0)
                    throw new ArgumentException($"{nameof(maximumResidualTolerances)}[{i}] must be greater than zero");
                if (desiredMaximumRelativeTolerances[i] <= 0)
                    throw new ArgumentException($"{nameof(desiredMaximumRelativeTolerances)}[{i}] must be greater than zero");
                if (desiredMaximumResidualTolerances[i] <= 0)
                    throw new ArgumentException($"{nameof(desiredMaximumResidualTolerances)}[{i}] must be greater than zero");
                if (targetRelativeTolerances[i] <= 0)
                    throw new ArgumentException($"{nameof(targetRelativeTolerances)}[{i}] must be greater than zero");
                if (targetResidualTolerances[i] <= 0)
                    throw new ArgumentException($"{nameof(targetResidualTolerances)}[{i}] must be greater than zero");

                if (targetRelativeTolerances[i] > maximumRelativeTolerances[i])
                    throw new ArgumentException($"{nameof(targetRelativeTolerances)}[{i}] must be less or equal to {nameof(maximumRelativeTolerances)}[{i}]");
                if (desiredMaximumRelativeTolerances[i] > maximumRelativeTolerances[i])
                    throw new ArgumentException($"{nameof(desiredMaximumRelativeTolerances)}[{i}] must be less or equal to {nameof(maximumRelativeTolerances)}[{i}]");
                if (targetRelativeTolerances[i] > desiredMaximumRelativeTolerances[i])
                    throw new ArgumentException($"{nameof(targetRelativeTolerances)} must be less or equal to {nameof(desiredMaximumRelativeTolerances)}[{i}]");

                if (targetResidualTolerances[i] > maximumResidualTolerances[i])
                    throw new ArgumentException($"{nameof(targetResidualTolerances)}[{i}] must be less or equal to {nameof(maximumResidualTolerances)}[{i}]");
                if (desiredMaximumResidualTolerances[i] > maximumResidualTolerances[i])
                    throw new ArgumentException($"{nameof(desiredMaximumResidualTolerances)}[{i}] must be less or equal to {nameof(maximumResidualTolerances)}[{i}]");
                if (targetResidualTolerances[i] > desiredMaximumResidualTolerances[i])
                    throw new ArgumentException($"{nameof(targetResidualTolerances)}[{i}] must be less or equal to {nameof(desiredMaximumResidualTolerances)}[{i}]");

                if (relativeToleranceWeights[i] <= 0)
                    throw new ArgumentException(nameof(relativeToleranceWeights));
                if (residualToleranceWeights[i] <= 0)
                    throw new ArgumentException(nameof(residualToleranceWeights));

                if (residualRelativeMagnitudeTolerances[i] <= 0)
                    throw new ArgumentException($"{nameof(residualRelativeMagnitudeTolerances)}[{i}] must be greater than zero");
                if (residualRelativeChangeTolerances[i] <= 0)
                    throw new ArgumentException($"{nameof(residualRelativeChangeTolerances)}[{i}] must be greater than zero");
                if (residualCurrentOverOldestStagnationThresholds[i] <= 0 || residualCurrentOverOldestStagnationThresholds[i] > 1)
                    throw new ArgumentException($"{nameof(residualCurrentOverOldestStagnationThresholds)}[{i}] must be in the range (0, 1]");
                if (stagnationWindowSize <= 0)
                    throw new ArgumentException($"{nameof(stagnationWindowSize)}[{i}] must be greater than zero");
                if (stepRelativeMagnitudeTolerances[i] <= 0)
                    throw new ArgumentException($"{nameof(stepRelativeMagnitudeTolerances)}[{i}] must be greater than zero");
                if (stepRelativeChangeTolerances[i] <= 0)
                    throw new ArgumentException($"{nameof(stepRelativeChangeTolerances)}[{i}] must be greater than zero");
                if (stepCurrentOverOldestStagnationThresholds[i] <= 0 || stepCurrentOverOldestStagnationThresholds[i] > 1)
                    throw new ArgumentException($"{nameof(stepCurrentOverOldestStagnationThresholds)}[{i}] must be in the range (0, 1]");

            }
            if (maxConvergenceSteps <= 0)
                throw new ArgumentException(nameof(maxConvergenceSteps));
            // Validate new parameters
            MaximumRelativeTolerances = maximumRelativeTolerances;
            MaximumResidualTolerances = maximumResidualTolerances;
            DesiredMaximumRelativeTolerances = desiredMaximumRelativeTolerances;
            DesiredMaximumResidualTolerances = desiredMaximumResidualTolerances;
            TargetRelativeTolerances = targetRelativeTolerances;
            TargetResidualTolerances = targetResidualTolerances;
            MaxConvergenceSteps = maxConvergenceSteps;
            FailIfExceedsMaxConvergenceSteps = failIfExceedsMaxConvergenceSteps;
            RelativeToleranceWeights = relativeToleranceWeights;
            ResidualToleranceWeights = residualToleranceWeights;

            // Assign new parameters
            ResidualRelativeMagnitudeTolerances = residualRelativeMagnitudeTolerances;
            ResidualRelativeChangeTolerances = residualRelativeChangeTolerances;
            ResidualCurrentOverOldestStagnationThresholds = residualCurrentOverOldestStagnationThresholds;
            StagnationWindowSize = stagnationWindowSize;
            StepRelativeMagnitudeTolerances = stepRelativeMagnitudeTolerances;
            StepRelativeChangeTolerances = stepRelativeChangeTolerances;
            StepCurrentOverOldestStagnationThresholds = stepCurrentOverOldestStagnationThresholds;
        }
    }
}
