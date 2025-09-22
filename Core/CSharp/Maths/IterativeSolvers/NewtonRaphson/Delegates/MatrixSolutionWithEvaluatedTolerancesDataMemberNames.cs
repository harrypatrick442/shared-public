using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Maths.IterativeSolvers.NewtonRaphson
{
    public static class MatrixSolutionWithEvaluatedTolerancesDataMemberNames
    {
        public const string 
            X = "x",
            CyclesRequired = "cyclesRequired",
            ReachedMaxIterations = "reachedMaxIterations",
            Dxs = "dXs",
            AbsoluteMaxErrors = "absoluteMaxErrors",
            RelativeMaxErrors = "relativeMaxErrors",
            ResidualMaxErrors = "residualMaxErrors",
            AbsoluteTargetErrors = "absoluteTargetErrors",
            RelativeTargetErrors = "relativeTargetErrors",
            ResidualTargetErrors = "residualTargetErrors",
            Residual = "residual",
            FulfilledMaxAbsolute = "fulfilledMaxAbsolute",
            FulfilledTargetAbsolute = "fulfilledTargetAbsolute",
            FulfilledMaxRelative = "fulfilledMaxRelative",
            FulfilledTargetRelative = "fulfilledTargetRelative",
            FulfilledMaxResidual = "fulfilledMaxResidual",
            FulfilledTargetResidual = "fulfilledTargetResidual",
            FulfilledMaxAll = "fulfilledMaxAll",
            FulfilledDesiredMaxAll = "fulfilledDesiredMaxAll",
            FulfilledTargetAll = "fulfilledTargetAll",
            TargetErrorAll = "targetErrorAll",
            MaxErrorAll = "maxErrorAll",
            MaxResidualMaxError = "maxResidualMaxError",
            MaxRelativeMaxErrors = "maxRelativeMaxErrors",
            MaxAbsoluteMaxError = "maxAbsoluteMaxError",
            FailureExplanation = "failureExplanation";
    }
}
