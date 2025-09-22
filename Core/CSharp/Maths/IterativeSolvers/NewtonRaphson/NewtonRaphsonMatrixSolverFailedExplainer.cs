using Core.Configuration;
using Core.Maths.Enums;
using Core.Maths.Exceptions;
using Core.Maths.IterativeSolvers.NewtonRaphson.Delegates;
using Core.Maths.IterativeSolvers.NewtonRaphson.Enums;
using Core.Maths.Tolerances;
using Core.Maths.Vectors;
using Core.Queueing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Core.Maths.IterativeSolvers.NewtonRaphson
{
    public static class NewtonRaphsonMatrixSolverFailedExplainer
    {
        public static NRFailedExplanation ExplainFailedConvergence(
            NewtonRaphsonMatrixSolutionWithEvaluatedTolerances solution)
        {
            VectorErrors? absoluteMaxErrors = null;
            VectorErrors? relativeMaxErrors = null;
            VectorErrors? residualMaxErrors = null;
            VectorErrors? absoluteTargetErrors = null;
            VectorErrors? relativeTargetErrors = null;
            VectorErrors? residualTargetErrors = null;
            if (!solution.FulfilledMaxAll) {
                if (!solution.FulfilledMaxAbsolute) {
                    absoluteMaxErrors = ExplainVectorNormalizedErrors(solution.AbsoluteMaxErrors, NRVectorType.AbsoluteMax);
                }
                if (!solution.FulfilledMaxRelative)
                {
                    relativeMaxErrors = ExplainVectorNormalizedErrors(solution.RelativeMaxErrors, NRVectorType.RelativeMax);
                }
                if (!solution.FulfilledMaxResidual)
                {
                    residualMaxErrors = ExplainVectorNormalizedErrors(solution.ResidualMaxErrors, NRVectorType.ResidualMax);
                }
            }
            if (!solution.FulfilledTargetAll)
            {
                if (!solution.FulfilledTargetAbsolute)
                {
                    absoluteTargetErrors = ExplainVectorNormalizedErrors(solution.AbsoluteTargetErrors, NRVectorType.AbsoluteTarget);
                }
                if (!solution.FulfilledTargetRelative)
                {
                    relativeTargetErrors = ExplainVectorNormalizedErrors(solution.RelativeTargetErrors, NRVectorType.RelativeTarget);
                }
                if (!solution.FulfilledTargetResidual)
                {
                    residualTargetErrors = ExplainVectorNormalizedErrors(solution.ResidualTargetErrors, NRVectorType.ResidualTarget);
                }
            }
            return new NRFailedExplanation(absoluteMaxErrors, relativeMaxErrors, residualMaxErrors,
                absoluteTargetErrors, relativeTargetErrors, residualTargetErrors);
        }
        private static VectorErrors? ExplainVectorNormalizedErrors(
            double[] normalizedErrors, NRVectorType vectorType) {
            List<RowError>? rowErrors = null;
            for(int i=0; i < normalizedErrors.Length; i++)
            {
                double normalizedError = normalizedErrors[i];
                if (normalizedError <= 1)
                    continue;
                if(rowErrors == null) 
                    rowErrors = new List<RowError>();
                rowErrors.Add(new RowError(index: i, normalizedError: normalizedError));
            }
            if (rowErrors == null) 
                return null;
            return new VectorErrors(vectorType, rowErrors.ToArray());
        }
    }
}