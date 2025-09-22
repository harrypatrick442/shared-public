using Core.Configuration;
using Core.Maths.Enums;
using Core.Maths.Exceptions;
using Core.Maths.IterativeSolvers.NewtonRaphson.Delegates;
using Core.Maths.Tolerances;
using Core.Maths.Vectors;
using System;
using System.Linq;
using System.Threading;

namespace Core.Maths.IterativeSolvers.NewtonRaphson
{
    public static class NewtonRaphsonMatrixSolver
    {
        private const double SmallValueThreshold = 1e-12; // Threshold to avoid errors from excessively small values

        public static NewtonRaphsonMatrixSolutionWithEvaluatedTolerances? Solve(
            double[] initialGuess,
            DelegateFunctionEvaluationAndSolveForDeltaX functionEvaluationAndSolveForDeltaX,
            NewtonRaphsonStoppingParametersMatrixContextualized stoppingParameters,
            AbsoluteTolerancesVector absoluteTolerances,
            CancellationToken cancellationToken)
        {
            var evaluate = Create_Evaluate(initialGuess, stoppingParameters, absoluteTolerances);
            double[] currentXf = VectorHelper.Clone(initialGuess);
            int cyclesRequired = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                cyclesRequired++;
                double[] residual;
                functionEvaluationAndSolveForDeltaX(out residual,
                    out double[] xAtEndOfIteration, cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;

                // Regularize small residual values
                //RegularizeSmallValues(residual);

                bool shouldStop = evaluate(xAtEndOfIteration, residual,
                    out NewtonRaphsonMatrixSolutionWithEvaluatedTolerances? solution,
                    out bool reachedMaxIterations);
                if (cancellationToken.IsCancellationRequested) break;
                if (shouldStop)
                {
                    if (solution != null)
                    {
                        solution!.CyclesRequired = cyclesRequired;
                        solution!.ReachedMaxIterations = reachedMaxIterations;
                    }
                    return solution!;
                }
            };
            return null;
        }

        private static DelegateEvaluateSolutionMatrixForm Create_Evaluate(
            double[] initialX,
            NewtonRaphsonStoppingParametersMatrixContextualized stoppingParameters,
            AbsoluteTolerancesVector absoluteTolerances)
        {
            DelegateNewtonRaphsonEvaluateTolerances evaluateTolerances =
                Create_EvaluateTolerances(stoppingParameters, absoluteTolerances);
            Func<Func<NewtonRaphsonMatrixSolutionWithEvaluatedTolerances>,bool > checkReachedMaxIterations = Create_CheckReachedMaxIterations(
                stoppingParameters);
            double[] lastX = initialX;
            DelegateCheckConvergence checkResidualsHasConverged = Create_CheckResidualHasConvergedNoNorm(
                stoppingParameters.ResidualRelativeMagnitudeTolerances,
                stoppingParameters.ResidualRelativeChangeTolerances,
                stoppingParameters.ResidualCurrentOverOldestStagnationThresholds,
                stoppingParameters.StagnationWindowSize
            );
            DelegateCheckConvergence checkStepHasConverged = Create_CheckStepHasConvergedNoNorm(
                stoppingParameters.StepRelativeMagnitudeTolerances,
                stoppingParameters.StepRelativeChangeTolerances,
                stoppingParameters.StepCurrentOverOldestStagnationThresholds,
                stoppingParameters.StagnationWindowSize
            );
            DelegateCheckFullConvergence checkHasConverged = (double[] residual, double[] currentX) => {
                bool residualHasConverted = checkResidualsHasConverged(residual, out ConvergenceReason reasonResidual);
                bool stepHasConverted = checkStepHasConverged(currentX, out ConvergenceReason reasonStep);
                bool converged =  residualHasConverted || stepHasConverted;                
                return converged;
            };
            NewtonRaphsonMatrixSolutionWithEvaluatedTolerances? currentBestSolution = null;
            return (double[] currentX, double[] residual,
                out NewtonRaphsonMatrixSolutionWithEvaluatedTolerances? solution, out bool reachedMaxIterations) =>
            {
                reachedMaxIterations = checkReachedMaxIterations(()=> evaluateTolerances(currentX, lastX, residual));
                if (reachedMaxIterations)
                {
                    var newSolution = evaluateTolerances(currentX, lastX, residual);
                    solution = DetermineBestSolution(currentBestSolution, newSolution);
                    return true;
                }
                if (checkHasConverged(residual, currentX))
                {
                    var newSolution = evaluateTolerances(currentX, lastX, residual);
                    if (newSolution.FulfilledTargetAll) {
                        solution = newSolution;
                        return true;
                    }
                    currentBestSolution = DetermineBestSolution(
                        currentBestSolution, newSolution);
                    solution = currentBestSolution;
                }
                lastX = currentX;
                solution = null;
                return false;
            };
            /*
            return (double[] currentX, double[] residual,
                out MatrixSolutionWithEvaluatedTolerances? solution, out bool reachedMaxIterations) =>
            {
                solution = currentBestSolution;
                MatrixSolutionWithEvaluatedTolerances newSolution
                    = evaluateTolerances(currentX, lastX, residual);
                // Regularize small values in residual to avoid instability
                //RegularizeSmallValues(residual);

                lastX = currentX;
                reachedMaxIterations = checkReachedMaxIterations();
                if (newSolution.FulfilledTargetAll)
                {
                    currentBestSolution = newSolution; // Update the best solution
                    solution = newSolution;
                    return true;
                }
                if (currentBestSolution == null || newSolution.FulfilledMaxAll
                    && newSolution.MaxErrorAll < currentBestSolution.MaxErrorAll)
                {
                    currentBestSolution = newSolution;
                    solution = newSolution;
                    bool converged = checkHasConverged(residual, currentX);
                    return reachedMaxIterations || converged;
                }
                bool converged2 = checkHasConverged(residual, currentX);
                return reachedMaxIterations || converged2;
            };
            */
        }
        private static NewtonRaphsonMatrixSolutionWithEvaluatedTolerances
            DetermineBestSolution(
                NewtonRaphsonMatrixSolutionWithEvaluatedTolerances? currentSolution,
                NewtonRaphsonMatrixSolutionWithEvaluatedTolerances newSolution)
        {
            if (currentSolution == null) return newSolution;
            if (currentSolution.FulfilledMaxAll) {
                if (newSolution.FulfilledMaxAll) {
                    return newSolution.MaxErrorAll < currentSolution.MaxErrorAll
                        ? newSolution : currentSolution;

                }
                return currentSolution;
            }
            if (newSolution.FulfilledMaxAll) {
                return newSolution;
            }
            return newSolution.MaxErrorAll < currentSolution.MaxErrorAll
                ? newSolution : currentSolution;
        }

        private static Func<Func<NewtonRaphsonMatrixSolutionWithEvaluatedTolerances>, bool> Create_CheckReachedMaxIterations(
            NewtonRaphsonStoppingParametersMatrixContextualized stoppingParameters)
        {
            int nChecks = 0;
            if (stoppingParameters.MaxConvergenceSteps == null)
            {
                return (ignore) =>
                {
                    nChecks++;
                    return false;
                };
            }
            int maxConvergenceSteps = (int)stoppingParameters.MaxConvergenceSteps;
            return (Func<NewtonRaphsonMatrixSolutionWithEvaluatedTolerances>getSolution) =>
            {
                nChecks++;
                if (nChecks < maxConvergenceSteps)
                {
                    return false;
                }
                var bestSolution = getSolution()!;
                if (!stoppingParameters.FailIfExceedsMaxConvergenceSteps
                    || bestSolution.FulfilledMaxAll)
                {
                    return true;
                }
                throw new NewtonRaphsonMatrixConvergenceException(
                    ConvergenceFailureType.ExceededMaxIterations,
                    () => $"Failed to converge. Explanation: {bestSolution.FailureExplanation?.ToString()}",
                    bestSolution
                );
            };
        }

        private static DelegateNewtonRaphsonEvaluateTolerances Create_EvaluateTolerances(
            NewtonRaphsonStoppingParametersMatrixContextualized stoppingParameters,
            AbsoluteTolerancesVector absoluteTolerances)
        {
            int lengthX = absoluteTolerances.Length;
            double totalAbsoluteWeight = absoluteTolerances.Weights.Sum();
            double totalRelativeWeight = stoppingParameters.RelativeToleranceWeights.Sum();
            double totalResidualWeight = stoppingParameters.ResidualToleranceWeights.Sum();
            double totalWeight = totalAbsoluteWeight
                 + totalRelativeWeight + totalResidualWeight;
            return (currentX, lastX, residual) =>
            {
                bool fulfilledMaxRelative = true, fulfilledDesiredMaxRelative = true,
                    fulfilledTargetRelative = true,
                    fulfilledMaxResidual = true, fulfilledDesiredMaxResidual = true,
                    fulfilledTargetResidual = true,
                    fulfilledMaxAbsolute = true, fulfilledDesiredMaxAbsolute = true,
                    fulfilledTargetAbsolute = true;
                double[] dXs = new double[lengthX];
                double relativeMaxErrorTotal = 0;
                double relativeTargetErrorTotal = 0;
                double residualMaxErrorTotal = 0;
                double residualTargetErrorTotal = 0;
                double absoluteMaxErrorTotalXWeight = 0;
                double absoluteTargetErrorTotalXWeight = 0;
                double[] absoluteMaxErrors = new double[lengthX];
                double[] relativeMaxErrors = new double[lengthX];
                double[] residualMaxErrors = new double[lengthX];
                double[] absoluteTargetErrors = new double[lengthX];
                double[] relativeTargetErrors = new double[lengthX];
                double[] residualTargetErrors = new double[lengthX];
                for (int i = 0; i < lengthX; i++)
                {
                    double x_i_plus_one = currentX[i];
                    double x_i = lastX[i];
                    double dX = Math.Abs(x_i_plus_one - x_i);
                    dXs[i] = dX;

                    // Regularize small dX values to prevent instability
                    //if (dX < SmallValueThreshold) dX = SmallValueThreshold;

                    double absoluteMaxError = Math.Abs(dX / absoluteTolerances.MaxTolerances[i]);
                    double absoluteDesiredMaxError = Math.Abs(dX / absoluteTolerances.DesiredMaxTolerances[i]);
                    double absoluteTargetError = Math.Abs(dX / absoluteTolerances.TargetTolerances[i]);
                    absoluteMaxErrorTotalXWeight += absoluteMaxError * absoluteTolerances.Weights[i];
                    absoluteTargetErrorTotalXWeight += absoluteTargetError * absoluteTolerances.Weights[i];
                    fulfilledTargetAbsolute &= absoluteTargetError <= 1;
                    fulfilledDesiredMaxAbsolute &= absoluteDesiredMaxError <= 1;
                    fulfilledMaxAbsolute &= absoluteMaxError <= 1;
                    absoluteMaxErrors[i] = absoluteMaxError;
                    absoluteTargetErrors[i] = absoluteTargetError;

                    double maxXi = Math.Max(Math.Abs(x_i_plus_one), Math.Abs(x_i));
                    double relativeMaxError;
                    double relativeTargetError;
                    if (maxXi < SmallValueThreshold)
                    {
                        relativeMaxError = 0;
                        relativeTargetError = 0;
                    }
                    else
                    {
                        relativeMaxError = Math.Abs(dX / (stoppingParameters.MaximumRelativeTolerances[i] * maxXi));
                        relativeMaxErrors[i] = relativeMaxError;
                        fulfilledMaxRelative &= relativeMaxError <= 1;
                        relativeMaxErrorTotal += relativeMaxError;

                        double relativeDesiredMaxError = Math.Abs(
                            dX / (stoppingParameters.DesiredMaximumRelativeTolerances[i] * maxXi));
                        fulfilledDesiredMaxRelative &= relativeDesiredMaxError <= 1;

                        relativeTargetError = Math.Abs(dX / (stoppingParameters.TargetRelativeTolerances[i] * maxXi));
                        relativeTargetErrors[i] = relativeTargetError;
                        fulfilledTargetRelative &= relativeTargetError <= 1;
                        relativeTargetErrorTotal += relativeTargetError;
                    }

                    double r = residual[i];
                    double residualMaxError = Math.Abs(r / stoppingParameters.MaximumResidualTolerances[i]);
                    residualMaxErrors[i] = residualMaxError;
                    double residualDesiredMaxError = Math.Abs(r / stoppingParameters.DesiredMaximumResidualTolerances[i]);
                    double residualTargetError = Math.Abs(r / stoppingParameters.TargetResidualTolerances[i]);
                    residualTargetErrors[i] = residualTargetError;
                    residualMaxErrorTotal += residualMaxError;
                    residualTargetErrorTotal += residualTargetError;
                    if (residualMaxError > 1) { 
                    
                    }
                    fulfilledMaxResidual &= residualMaxError <= 1;
                    fulfilledDesiredMaxResidual &= residualDesiredMaxError <= 1;
                    fulfilledTargetResidual &= residualTargetError <= 1;
                }
                bool fulfilledMaxAll = fulfilledMaxAbsolute && fulfilledMaxRelative && fulfilledMaxResidual;
                bool fulfilledDesiredMaxAll = fulfilledDesiredMaxAbsolute && fulfilledDesiredMaxRelative && fulfilledDesiredMaxResidual;
                bool fulfilledTargetAll = fulfilledTargetAbsolute && fulfilledTargetRelative && fulfilledTargetResidual;
                // Simple equation for combined tolerance evaluation
                double maxErrorAll = (absoluteMaxErrorTotalXWeight
                                        + (relativeMaxErrorTotal * totalRelativeWeight)
                                        + (residualMaxErrorTotal * totalResidualWeight)
                                      ) / totalWeight;
                double targetErrorAll = (absoluteTargetErrorTotalXWeight
                                        + (relativeTargetErrorTotal * totalRelativeWeight)
                                        + (residualTargetErrorTotal * totalResidualWeight)
                                      ) / totalWeight;
                return new NewtonRaphsonMatrixSolutionWithEvaluatedTolerances(
                    X: currentX,
                    dXs: dXs,
                    absoluteMaxErrors: absoluteMaxErrors,
                    absoluteTargetErrors: absoluteTargetErrors,
                    relativeMaxErrors: relativeMaxErrors,
                    relativeTargetErrors: relativeTargetErrors,
                    residualMaxErrors: residualMaxErrors,
                    residualTargetErrors: residualTargetErrors,
                    residual: residual,
                    fulfilledMaxAbsolute: fulfilledMaxAbsolute,
                    fulfilledTargetAbsolute: fulfilledTargetAbsolute,
                    fulfilledMaxRelative: fulfilledMaxRelative,
                    fulfilledTargetRelative: fulfilledTargetRelative,
                    fulfilledMaxResidual: fulfilledMaxResidual,
                    fulfilledTargetResidual: fulfilledTargetResidual,
                    fulfilledMaxAll: fulfilledMaxAll,
                    fulfilledDesiredMaxAll: fulfilledDesiredMaxAll,
                    fulfilledTargetAll: fulfilledTargetAll,
                    maxErrorAll: maxErrorAll,
                    targetErrorAll: targetErrorAll
               );
            };
        }
        private delegate bool DelegateCheckFullConvergence(double[] currentResidual, double[] currentX);
        /*
        private static DelegateCheckConvergence Create_CheckConvergence(
            double residualTolerance, double stepTolerance,
            int length)
        {
            bool hasPrevious = false;
            double[] previousResiduals = new double[length];
            double[] previousXs = new double[length];
            return (double[] currentResiduals, double[] currentXs) =>
            {
                if (!hasPrevious)
                {
                    hasPrevious = true;
                    Array.Copy(currentResiduals, previousResiduals, length);
                    Array.Copy(currentXs, previousXs, length);
                    return false; // No stopping condition on the first iteration
                }

                // Residual Reduction Ratio (per-residual basis with small value fallback)
                try
                {
                    double residualErrorsSum = 0;
                    double stepErrorsSum = 0;
                    double[] residualErrors = new double[length];
                    double[] stepErrors = new double[length];
                    for (int i = 0; i < length; i++)
                    {
                        double currentResidual = currentResiduals[i];
                        double previousResidual = previousResiduals[i];
                        double maxResidual = Math.Max(Math.Abs(currentResidual),
                            Math.Abs(previousResidual));
                        double residualError =
                            Math.Abs(currentResidual - previousResidual)
                            / Math.Max(SmallValueThreshold, maxResidual);
                        residualErrors[i] = residualError;
                        residualErrorsSum += residualError;
                        double currentX = currentXs[i];
                        double previousX = previousXs![i];
                        double maxStep = Math.Max(Math.Abs(currentX),
                            Math.Abs(previousX));
                        double stepError = Math.Abs(currentX - previousX)
                        / Math.Max(SmallValueThreshold, maxStep);
                        stepErrorsSum += stepError;
                        stepErrors[i] = stepError;
                    }
                    double averageResidualError = residualErrorsSum / (double)length;
                    double averageStepError = stepErrorsSum / (double)length;
                    Console.WriteLine("previous residuals: ");
                    Console.WriteLine(VectorHelper.ToString(previousResiduals));
                    Console.WriteLine("current residuals: ");
                    Console.WriteLine(VectorHelper.ToString(currentResiduals));
                    Console.WriteLine("residual errors: ");
                    Console.WriteLine(VectorHelper.ToString(residualErrors));
                    Console.WriteLine("residual error: " + averageResidualError);
                    Console.WriteLine(VectorHelper.ToString(stepErrors));
                    Console.WriteLine("step error: " + averageStepError);
                    bool residualConverged = averageResidualError
                        <= residualTolerance;
                    bool stepConverged = averageStepError <= stepTolerance;
                    if (!residualConverged) return false;
                    if (!stepConverged) return false;
                    return true;
                }
                finally
                {
                    Array.Copy(currentResiduals, previousResiduals, length);
                    Array.Copy(currentXs, previousXs, length);
                }
            };
        }*/
            /*
            private const double RelativeMagnitudeTolerance = 1e-6; // Residual magnitude tolerance
            private const double RelativeChangeTolerance = 1e-4; // Relative change tolerance
            private const double CurrentOverOldestStagnationThreshold = 1e-3; // Stagnation tolerance
            private const int StagnationWindow = 5; // Window for stagnation detection
            */

            // Entry for residual history
        private class ResidualHistoryEntry
        {
            public double[] Residuals { get; }
            public double Norm { get; }

            public ResidualHistoryEntry(double[] residuals, double norm)
            {
                Residuals = residuals;
                Norm = norm;
            }
        }
        private static DelegateCheckConvergence Create_CheckResidualHasConvergedNoNorm(
            double[] relativeMagnitudeTolerances,
            double[] relativeChangeTolerances,
            double[] currentOverOldestStagnationThresholds,
            int stagnationWindowSize)
        {
            CyclicalBuffer<double[]> residualsHistory = new CyclicalBuffer<double[]>(stagnationWindowSize);

            return (double[] currentResiduals, out ConvergenceReason reason) =>
            {

                double[]? previous = residualsHistory.GetLatest();
                residualsHistory.Add(currentResiduals);

                if (previous == null)
                {
                    reason = ConvergenceReason.NotConverged;
                    return false;
                }
                double[] changeErrorNormalized = currentResiduals.Zip(previous, (c, p) => 
                    Math.Abs(c - p) 
                    / Math.Max(Math.Max(Math.Abs(c), Math.Abs(p)), SmallValueThreshold)).ToArray();

                if (!changeErrorNormalized.Zip(relativeChangeTolerances, (r, tolerance) => r > tolerance).Any())
                {
                    reason = ConvergenceReason.RelativeChangeBelowTolerance;
                    return true;
                }

                if (residualsHistory.IsBufferFull)
                {
                    double[] oldestEntry = residualsHistory.GetOldest()!;

                    // Prevent division by zero and handle small values with epsilon
                    var reductionRatio = currentResiduals.Zip(oldestEntry, (c, o) => 
                    Math.Max(c, SmallValueThreshold) / Math.Max(o, SmallValueThreshold)).ToArray();
                    if (reductionRatio.Zip(currentOverOldestStagnationThresholds, (r, t) => r >= t).All(b=>b))
                    {
                        reason = ConvergenceReason.StagnationThresholdExceeded;
                        return true;
                    }
                }

                reason = ConvergenceReason.NotConverged;
                return false;
            };
        }
        private static DelegateCheckConvergence Create_CheckResidualHasConverged(
            double relativeMagnitudeTolerance,
            double relativeChangeTolerance,
            double currentOverOldestStagnationThreshold,
            int stagnationWindowSize)
        {
            CyclicalBuffer<ResidualHistoryEntry> residualsHistory = new CyclicalBuffer<ResidualHistoryEntry>(stagnationWindowSize);

            return (double[] currentResiduals, out ConvergenceReason reason) =>
            {
                double currentNorm = VectorHelper.EuclideanL2Norm(currentResiduals);

                ResidualHistoryEntry? previous = residualsHistory.GetLatest();
                residualsHistory.Add(new ResidualHistoryEntry(currentResiduals, currentNorm));

                if (previous == null)
                {
                    reason = ConvergenceReason.NotConverged;
                    return false;
                }

                if (currentNorm <= relativeMagnitudeTolerance)
                {
                    reason = ConvergenceReason.MagnitudeBelowTolerance;
                    return true;
                }

                double deltaResidualsNorm = VectorHelper.EuclideanL2Norm(
                    currentResiduals.Zip(previous.Residuals, (a, b) => a - b).ToArray());

                double relativeChange = deltaResidualsNorm / Math.Max(
                    Math.Max(currentNorm, previous.Norm), relativeMagnitudeTolerance);

                if (relativeChange < relativeChangeTolerance)
                {
                    reason = ConvergenceReason.RelativeChangeBelowTolerance;
                    return true;
                }

                if (residualsHistory.IsBufferFull)
                {
                    ResidualHistoryEntry oldest = residualsHistory.GetOldest()!;
                    double reductionRatio = Math.Max(currentNorm, SmallValueThreshold) 
                        / Math.Max(oldest.Norm, SmallValueThreshold);
                    if (reductionRatio >= currentOverOldestStagnationThreshold)
                    {
                        reason = ConvergenceReason.StagnationThresholdExceeded;
                        return true;
                    }
                }

                reason = ConvergenceReason.NotConverged;
                return false;
            };
        }
        private static DelegateCheckConvergence Create_CheckStepHasConvergedNoNorm(
    double[] relativeMagnitudeTolerances,
    double[] relativeChangeTolerances,
    double[] currentOverOldestStagnationThresholds,
    int stagnationWindowSize)
        {
            CyclicalBuffer<double[]> stepHistory = new CyclicalBuffer<double[]>(stagnationWindowSize);

            return (double[] currentX, out ConvergenceReason reason) =>
            {
                double[]? previousEntry = stepHistory.GetLatest();
                stepHistory.Add(currentX);

                if (previousEntry == null)
                {
                    reason = ConvergenceReason.NotConverged;
                    return false;
                }

                double[] changeErrorNormalized = currentX.Zip(previousEntry, (c, p) =>
                    Math.Abs(c - p)
                    / Math.Max(Math.Max(Math.Abs(c), Math.Abs(p)), SmallValueThreshold))
                .ToArray();

                if (changeErrorNormalized.Zip(relativeChangeTolerances, (r, t) => r <= t).All(b=>b))
                {
                    reason = ConvergenceReason.RelativeChangeBelowTolerance;
                    return true;
                }

                if (stepHistory.IsBufferFull)
                {
                    double[] oldestEntry = stepHistory.GetOldest()!;

                    var reductionRatio = currentX.Zip(oldestEntry, (c, o) => Math.Max(Math.Abs(c - o), SmallValueThreshold) 
                    / Math.Max(Math.Max(Math.Abs(c), Math.Abs(o)), SmallValueThreshold));
                    if (reductionRatio.Zip(currentOverOldestStagnationThresholds, (r, t) => r >= t).All(b=>b))
                    {
                        reason = ConvergenceReason.StagnationThresholdExceeded;
                        return true;
                    }
                }

                reason = ConvergenceReason.NotConverged;
                return false;
            };
        }

        private static DelegateCheckConvergence Create_CheckStepHasConverged(
            double relativeMagnitudeTolerance,
            double relativeChangeTolerance,
            double currentOverOldestStagnationThreshold,
            int stagnationWindowSize)
        {
            CyclicalBuffer<StepHistoryEntry> stepHistory = new CyclicalBuffer<StepHistoryEntry>(stagnationWindowSize);

            return (double[] currentX, out ConvergenceReason reason) =>
            {
                double currentNorm = VectorHelper.EuclideanL2Norm(currentX);

                StepHistoryEntry? previousEntry = stepHistory.GetLatest();
                stepHistory.Add(new StepHistoryEntry(currentX, currentNorm));

                if (previousEntry == null)
                {
                    reason = ConvergenceReason.NotConverged;
                    return false;
                }

                double deltaStepNorm = VectorHelper.EuclideanL2Norm(
                    currentX.Zip(previousEntry.Step, (current, previous) => current - previous).ToArray());

                double relativeChange = deltaStepNorm / Math.Max(
                    Math.Max(currentNorm, previousEntry.Norm), relativeMagnitudeTolerance);

                if (relativeChange < relativeChangeTolerance)
                {
                    reason = ConvergenceReason.RelativeChangeBelowTolerance;
                    return true;
                }

                if (stepHistory.IsBufferFull)
                {
                    StepHistoryEntry oldestEntry = stepHistory.GetOldest()!;
                    double reductionRatio = Math.Max(Math.Abs(currentNorm - oldestEntry.Norm), SmallValueThreshold) 
                    / Math.Max(Math.Max(currentNorm, oldestEntry.Norm), SmallValueThreshold);

                    if (reductionRatio >= currentOverOldestStagnationThreshold)
                    {
                        reason = ConvergenceReason.StagnationThresholdExceeded;
                        return true;
                    }
                }

                reason = ConvergenceReason.NotConverged;
                return false;
            };
        }

        private class StepHistoryEntry
        {
            public double[] Step { get; }
            public double Norm { get; }

            public StepHistoryEntry(double[] step, double norm)
            {
                Step = step;
                Norm = norm;
            }
        }
        private enum ConvergenceReason
        {
            MagnitudeBelowTolerance,
            RelativeChangeBelowTolerance,
            StagnationThresholdExceeded,
            NotConverged
        }
        private delegate bool DelegateCheckConvergence(double[] current, out ConvergenceReason reason);
        


    }
}
/*
            double[]? previousResidual = null;
            double[]? previousX = null;
            return (double[] currentResidual, double[] currentX) =>
            {
                if (previousResidual == null)
                {
                    previousResidual = currentResidual;
                    previousX = currentX;
                    return false; // No stopping condition on the first iteration
                }

                // Residual Reduction Ratio (per-residual basis with small value fallback)
                double residualReduction = currentResidual.Zip(previousResidual, (c, p) =>
                {
                    double maxResidual = Math.Max(Math.Abs(c), Math.Abs(p));
                    return Math.Abs(c - p) / Math.Max(SmallValueThreshold, maxResidual); // Use small value fallback
                }).Average();

                // Step Size (per-value basis with small value fallback)
                double stepSize = currentX.Zip(previousX!, (c, p) =>
                {
                    double maxStep = Math.Max(Math.Abs(c), Math.Abs(p));
                    return Math.Abs(c - p) / Math.Max(SmallValueThreshold, maxStep); // Use small value fallback
                }).Average();

                // Update previous residuals and X for the next iteration
                previousResidual = currentResidual;
                previousX = currentX;

                // Check thresholds
                return residualReduction < residualTolerance || stepSize < stepTolerance;
            };*/
