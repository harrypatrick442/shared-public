using Core.Maths.Enums;
using Core.Maths.Exceptions;
using Core.Maths.IterativeSolvers.NewtonRaphson.Delegates;
using Core.Maths.Tolerances;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Maths.IterativeSolvers.NewtonRaphson
{
    public static class NewtonRaphsonSingleSolver
    {
        public static double Solve(
            double initialGuess,
            DelegateCalculateSingle calculate,
            NewtonRaphsonStoppingParametersSingle? stoppingParameters = null)
        {
            if (stoppingParameters == null)
            {
                stoppingParameters = NewtonRaphsonStoppingParametersSingle.Default;
            }
            double x = initialGuess;
            Action handleIterationLimit = stoppingParameters.IterationLimit != null
                ? Create_HandleIterationLimit((int)stoppingParameters.IterationLimit)
                : () => { };
            double dampingFactor = stoppingParameters.DampingFactor;
            Func<double, double>? shrinkDampingFactor = null;
            Func<double, double>? growDampingFactor = null;
            LinkedList<(double xAtStartOfIteration, double dampingFactorUsedOnIteration)>? previouses;
            if (stoppingParameters.DynamicDampingFactor)
            {
                previouses = new LinkedList<(double previousX, double dampingFactorUsed)>();
                shrinkDampingFactor = Create_ShrinkDampingFactor(stoppingParameters);
                growDampingFactor = Create_GrowDampingFactor(stoppingParameters);
            }
            else
            {
                previouses = null;
            }
            double f, fPrime;
            while (true)
            {
                previouses?.AddLast((x, dampingFactor));
                try
                {
                    calculate(x, out f, out fPrime);
                    /*
                    if (Math.Abs(fPrime) < stoppingParameters.MinSlope)
                    {
                        throw new ConvergenceException("Slope is too small, risking instability in Newton-Raphson.");
                    }*/
                    if (growDampingFactor != null)
                    {
                        dampingFactor = growDampingFactor(dampingFactor);
                    }
                }
                catch (ConvergenceException cEx)
                {
                    if (shrinkDampingFactor != null)
                    {
                        double newDampingFactor = shrinkDampingFactor.Invoke(dampingFactor);
                        while (true)
                        {
                            if (!previouses!.Any())
                            {
                                throw new ConvergenceException(
                                    ConvergenceFailureType.CouldNotBacktrack,
                                    () => "Failed to converge as could backtrack no further while shrinking damping factor!",
                                    cEx
                                );
                            }
                            var (previousX, dampingFactorUsed) = previouses!.Last();
                            previouses!.RemoveLast();
                            if (dampingFactorUsed > dampingFactor)
                            {
                                x = previousX;
                                break;
                            }
                        }
                        dampingFactor = newDampingFactor;
                        continue;
                    }
                    throw new ConvergenceException(
                        () => $"Failed to converge. Consider allowing a dynamic damping factor by setting " +
                            $"{nameof(stoppingParameters.DampingFactor)} to null", cEx
                    );
                }

                if (Math.Abs(fPrime) < 1e-12)
                {
                    throw new ConvergenceException(
                        ConvergenceFailureType.DerivativeTooSmall,
                        () => "PolynomialDerivative too small for stability in Newton-Raphson."
                    );
                }
                double deltaX = f / fPrime;
                deltaX *= dampingFactor;
                x -= deltaX;
                if (Math.Abs(deltaX) < stoppingParameters.AbsoluteTolerance)
                {
                    return x;
                }
                handleIterationLimit();
            }
        }
        private static Action Create_HandleIterationLimit(int iterationLimit)
        {
            int nIterations = 0;
            return () =>
            {
                if (nIterations++ >= iterationLimit)
                {
                    throw new ConvergenceException(
                        ConvergenceFailureType.ExceededMaxIterations,
                        () => "Newton-Raphson did not converge because iteration limit was reached."
                    );
                }
            };

        }
        private static Func<double, double> Create_ShrinkDampingFactor(
            NewtonRaphsonStoppingParametersSingle stoppingParameters)
        {
            return (dampingFactor) =>
            {
                double newDampingFactor = stoppingParameters.DampingFactorShrinkFactor * dampingFactor;
                if (stoppingParameters.MinDampingFactor != null
                && newDampingFactor < (double)stoppingParameters.MinDampingFactor)
                {
                    newDampingFactor = (double)stoppingParameters.MinDampingFactor;
                }
                return newDampingFactor;
            };
        }
        private static Func<double, double> Create_GrowDampingFactor(
            NewtonRaphsonStoppingParametersSingle stoppingParameters)
        {
            return (dampingFactor) =>
            {
                double newDampingFactor = stoppingParameters.DampingFactorGrowthFactor * dampingFactor;
                if (stoppingParameters.MaxDampingFactor != null
                && newDampingFactor > (double)stoppingParameters.MaxDampingFactor)
                {
                    newDampingFactor = (double)stoppingParameters.MaxDampingFactor;
                }
                return newDampingFactor;
            };
        }
    }
}
