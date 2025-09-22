using System;

namespace Core.Maths.IterativeSolvers.NewtonRaphson
{
    /// <summary>
    /// For solving non-linear
    /// </summary>
    public class NewtonRaphsonStoppingParametersMatrix
    {
        private const double DEFAULT_RESIDUAL_RELATIVE_MAGNITUDE_TOLERANCE = 1e-6;
        private const double DEFAULT_RESIDUAL_RELATIVE_CHANGE_TOLERANCE = 1e-4;
        private const double DEFAULT_RESIDUAL_CURRENT_OVER_OLDEST_STAGNATION_THRESHOLD = 0.99;
        private const int DEFAULT_STAGNATION_WINDOW_SIZE = 5;
        private const double DEFAULT_STEP_RELATIVE_MAGNITUDE_TOLERANCE = 1e-6;
        private const double DEFAULT_STEP_RELATIVE_CHANGE_TOLERANCE = 1e-4;
        private const double DEFAULT_STEP_CURRENT_OVER_OLDEST_STAGNATION_THRESHOLD = 0.99;

        public double MaximumRelativeTolerance { get; }
        public double MaximumResidualTolerance { get; }
        public double DesiredMaximumRelativeTolerance { get; }
        public double DesiredMaximumResidualTolerance { get; }
        public double TargetRelativeTolerance { get; }
        public double TargetResidualTolerance { get; }
        public double RelativeToleranceWeight { get; }
        public double ResidualToleranceWeight { get; }
        public int? MaxConvergenceSteps { get; }
        public bool FailIfExceedsMaxConvergenceSteps { get; }

        // Newly added properties
        public double ResidualRelativeMagnitudeTolerance { get; }
        public double ResidualRelativeChangeTolerance { get; }
        public double ResidualCurrentOverOldestStagnationThreshold { get; }
        public int StagnationWindowSize { get; }
        public double StepRelativeMagnitudeTolerance { get; }
        public double StepRelativeChangeTolerance { get; }
        public double StepCurrentOverOldestStagnationThreshold { get; }

        public NewtonRaphsonStoppingParametersMatrix(
            double maximumRelativeTolerance,
            double desiredMaximumRelativeTolerance,
            double targetRelativeTolerance,
            double maximumResidualTolerance,
            double desiredMaximumResidualTolerance,
            double targetResidualTolerance,
            double relativeToleranceWeight,
            double residualToleranceWeight,
            int? maxConvergenceSteps,
            bool failIfExceedsMaxConvergenceSteps,
            double residualRelativeMagnitudeTolerance = DEFAULT_RESIDUAL_RELATIVE_MAGNITUDE_TOLERANCE,
            double residualRelativeChangeTolerance = DEFAULT_RESIDUAL_RELATIVE_CHANGE_TOLERANCE,
            double residualCurrentOverOldestStagnationThreshold = DEFAULT_RESIDUAL_CURRENT_OVER_OLDEST_STAGNATION_THRESHOLD,
            int stagnationWindowSize = DEFAULT_STAGNATION_WINDOW_SIZE,
            double stepRelativeMagnitudeTolerance = DEFAULT_STEP_RELATIVE_MAGNITUDE_TOLERANCE,
            double stepRelativeChangeTolerance = DEFAULT_STEP_RELATIVE_CHANGE_TOLERANCE,
            double stepCurrentOverOldestStagnationThreshold = DEFAULT_STEP_CURRENT_OVER_OLDEST_STAGNATION_THRESHOLD)
        {
            if (maximumRelativeTolerance <= 0)
                throw new ArgumentException($"{nameof(maximumRelativeTolerance)} must be greater than zero");
            if (maximumResidualTolerance <= 0)
                throw new ArgumentException($"{nameof(maximumResidualTolerance)} must be greater than zero");
            if (desiredMaximumRelativeTolerance <= 0)
                throw new ArgumentException($"{nameof(desiredMaximumRelativeTolerance)} must be greater than zero");
            if (desiredMaximumResidualTolerance <= 0)
                throw new ArgumentException($"{nameof(desiredMaximumResidualTolerance)} must be greater than zero");
            if (targetRelativeTolerance <= 0)
                throw new ArgumentException($"{nameof(targetRelativeTolerance)} must be greater than zero");
            if (targetResidualTolerance <= 0)
                throw new ArgumentException($"{nameof(targetResidualTolerance)} must be greater than zero");

            if (targetRelativeTolerance > maximumRelativeTolerance)
                throw new ArgumentException($"{nameof(targetRelativeTolerance)} must be less or equal to {nameof(maximumRelativeTolerance)}");
            if (desiredMaximumRelativeTolerance > maximumRelativeTolerance)
                throw new ArgumentException($"{nameof(desiredMaximumRelativeTolerance)} must be less or equal to {nameof(maximumRelativeTolerance)}");
            if (targetRelativeTolerance > desiredMaximumRelativeTolerance)
                throw new ArgumentException($"{nameof(targetRelativeTolerance)} must be less or equal to {nameof(desiredMaximumRelativeTolerance)}");

            if (targetResidualTolerance > maximumResidualTolerance)
                throw new ArgumentException($"{nameof(targetResidualTolerance)} must be less or equal to {nameof(maximumResidualTolerance)}");
            if (desiredMaximumResidualTolerance > maximumResidualTolerance)
                throw new ArgumentException($"{nameof(desiredMaximumResidualTolerance)} must be less or equal to {nameof(maximumResidualTolerance)}");
            if (targetResidualTolerance > desiredMaximumResidualTolerance)
                throw new ArgumentException($"{nameof(targetResidualTolerance)} must be less or equal to {nameof(desiredMaximumResidualTolerance)}");

            if (maxConvergenceSteps <= 0)
                throw new ArgumentException(nameof(maxConvergenceSteps));
            if (relativeToleranceWeight <= 0)
                throw new ArgumentException(nameof(relativeToleranceWeight));
            if (residualToleranceWeight <= 0)
                throw new ArgumentException(nameof(residualToleranceWeight));

            // Validate new parameters
            if (residualRelativeMagnitudeTolerance <= 0)
                throw new ArgumentException($"{nameof(residualRelativeMagnitudeTolerance)} must be greater than zero");
            if (residualRelativeChangeTolerance <= 0)
                throw new ArgumentException($"{nameof(residualRelativeChangeTolerance)} must be greater than zero");
            if (residualCurrentOverOldestStagnationThreshold <= 0 || residualCurrentOverOldestStagnationThreshold > 1)
                throw new ArgumentException($"{nameof(residualCurrentOverOldestStagnationThreshold)} must be in the range (0, 1]");
            if (stagnationWindowSize <= 0)
                throw new ArgumentException($"{nameof(stagnationWindowSize)} must be greater than zero");
            if (stepRelativeMagnitudeTolerance <= 0)
                throw new ArgumentException($"{nameof(stepRelativeMagnitudeTolerance)} must be greater than zero");
            if (stepRelativeChangeTolerance <= 0)
                throw new ArgumentException($"{nameof(stepRelativeChangeTolerance)} must be greater than zero");
            if (stepCurrentOverOldestStagnationThreshold <= 0 || stepCurrentOverOldestStagnationThreshold > 1)
                throw new ArgumentException($"{nameof(stepCurrentOverOldestStagnationThreshold)} must be in the range (0, 1]");

            MaximumRelativeTolerance = maximumRelativeTolerance;
            MaximumResidualTolerance = maximumResidualTolerance;
            DesiredMaximumRelativeTolerance = desiredMaximumRelativeTolerance;
            DesiredMaximumResidualTolerance = desiredMaximumResidualTolerance;
            TargetRelativeTolerance = targetRelativeTolerance;
            TargetResidualTolerance = targetResidualTolerance;
            MaxConvergenceSteps = maxConvergenceSteps;
            FailIfExceedsMaxConvergenceSteps = failIfExceedsMaxConvergenceSteps;
            RelativeToleranceWeight = relativeToleranceWeight;
            ResidualToleranceWeight = residualToleranceWeight;

            // Assign new parameters
            ResidualRelativeMagnitudeTolerance = residualRelativeMagnitudeTolerance;
            ResidualRelativeChangeTolerance = residualRelativeChangeTolerance;
            ResidualCurrentOverOldestStagnationThreshold = residualCurrentOverOldestStagnationThreshold;
            StagnationWindowSize = stagnationWindowSize;
            StepRelativeMagnitudeTolerance = stepRelativeMagnitudeTolerance;
            StepRelativeChangeTolerance = stepRelativeChangeTolerance;
            StepCurrentOverOldestStagnationThreshold = stepCurrentOverOldestStagnationThreshold;
        }
    }
}
