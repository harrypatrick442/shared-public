using Core.Maths.Matrices;
using JSON;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using D = Core.Maths.IterativeSolvers.NewtonRaphson
    .MatrixSolutionWithEvaluatedTolerancesDataMemberNames;
namespace Core.Maths.IterativeSolvers.NewtonRaphson
{
    [DataContract]
    public class NewtonRaphsonMatrixSolutionWithEvaluatedTolerances
    {

        private NRFailedExplanation? _FailureExplanation;
        [JsonPropertyName(D.FailureExplanation)]
        [JsonInclude]
        [DataMember(Name = D.FailureExplanation)]
        public NRFailedExplanation? FailureExplanation
        {
            get
            {
                if (_FailureExplanation == null)
                {
                    _FailureExplanation = NewtonRaphsonMatrixSolverFailedExplainer.ExplainFailedConvergence(this);
                }
                return _FailureExplanation;
            }
        }
        [JsonPropertyName(D.X)]
        [JsonInclude]
        [DataMember(Name = D.X)]
        public double[] X { get; }
        [JsonPropertyName(D.CyclesRequired)]
        [JsonInclude]
        [DataMember(Name = D.CyclesRequired)]
        public int CyclesRequired { get; set; }
        [JsonPropertyName(D.ReachedMaxIterations)]
        [JsonInclude]
        [DataMember(Name = D.ReachedMaxIterations)]
        public bool ReachedMaxIterations { get; set; }
        [JsonPropertyName(D.Dxs)]
        [JsonInclude]
        [DataMember(Name = D.Dxs)]
        public double[] Dxs { get; }
        [JsonPropertyName(D.AbsoluteMaxErrors)]
        [JsonInclude]
        [DataMember(Name = D.AbsoluteMaxErrors)]
        public double[] AbsoluteMaxErrors { get; }
        [JsonPropertyName(D.AbsoluteTargetErrors)]
        [JsonInclude]
        [DataMember(Name = D.AbsoluteTargetErrors)]
        public double[] AbsoluteTargetErrors { get; }
        [JsonPropertyName(D.RelativeMaxErrors)]
        [JsonInclude]
        [DataMember(Name = D.RelativeMaxErrors)]
        public double[] RelativeMaxErrors { get; }
        [JsonPropertyName(D.RelativeTargetErrors)]
        [JsonInclude]
        [DataMember(Name = D.RelativeTargetErrors)]
        public double[] RelativeTargetErrors { get; }
        [JsonPropertyName(D.ResidualMaxErrors)]
        [JsonInclude]
        [DataMember(Name = D.ResidualMaxErrors)]
        public double[] ResidualMaxErrors { get; }
        [JsonPropertyName(D.ResidualTargetErrors)]
        [JsonInclude]
        [DataMember(Name = D.ResidualTargetErrors)]
        public double[] ResidualTargetErrors { get; }
        [JsonPropertyName(D.Residual)]
        [JsonInclude]
        [DataMember(Name = D.Residual)]
        public double[] Residual { get; }
        [JsonPropertyName(D.FulfilledMaxAbsolute)]
        [JsonInclude]
        [DataMember(Name = D.FulfilledMaxAbsolute)]
        public bool FulfilledMaxAbsolute { get; }
        [JsonPropertyName(D.FulfilledTargetAbsolute)]
        [JsonInclude]
        [DataMember(Name = D.FulfilledTargetAbsolute)]
        public bool FulfilledTargetAbsolute { get; }
        [JsonPropertyName(D.FulfilledMaxRelative)]
        [JsonInclude]
        [DataMember(Name = D.FulfilledMaxRelative)]
        public bool FulfilledMaxRelative { get; }
        [JsonPropertyName(D.FulfilledTargetRelative)]
        [JsonInclude]
        [DataMember(Name = D.FulfilledTargetRelative)]
        public bool FulfilledTargetRelative { get; }
        [JsonPropertyName(D.FulfilledMaxResidual)]
        [JsonInclude]
        [DataMember(Name = D.FulfilledMaxResidual)]
        public bool FulfilledMaxResidual { get; }
        [JsonPropertyName(D.FulfilledTargetResidual)]
        [JsonInclude]
        [DataMember(Name = D.FulfilledTargetResidual)]
        public bool FulfilledTargetResidual { get; }
        [JsonPropertyName(D.FulfilledMaxAll)]
        [JsonInclude]
        [DataMember(Name = D.FulfilledMaxAll)]
        public bool FulfilledMaxAll { get; }
        [JsonPropertyName(D.FulfilledDesiredMaxAll)]
        [JsonInclude]
        [DataMember(Name = D.FulfilledDesiredMaxAll)]
        public bool FulfilledDesiredMaxAll  { get; }
        [JsonPropertyName(D.FulfilledTargetAll)]
        [JsonInclude]
        [DataMember(Name = D.FulfilledTargetAll)]
        public bool FulfilledTargetAll { get; }
        [JsonPropertyName(D.TargetErrorAll)]
        [JsonInclude]
        [DataMember(Name = D.TargetErrorAll)]
        public double TargetErrorAll { get; }
        [JsonPropertyName(D.MaxErrorAll)]
        [JsonInclude]
        [DataMember(Name = D.MaxErrorAll)]
        public double MaxErrorAll { get; }

        [JsonPropertyName(D.MaxRelativeMaxErrors)]
        [JsonInclude]
        [DataMember(Name = D.MaxRelativeMaxErrors)]
        public double MaxRelativeMaxErrors => RelativeMaxErrors.Max();
        [JsonPropertyName(D.MaxAbsoluteMaxError)]
        [JsonInclude]
        [DataMember(Name = D.MaxAbsoluteMaxError)]
        public double MaxAbsoluteMaxError => AbsoluteMaxErrors.Max();
        [JsonPropertyName(D.MaxResidualMaxError)]
        [JsonInclude]
        [DataMember(Name = D.MaxResidualMaxError)]
        public double MaxResidualMaxError => ResidualMaxErrors.Max();
        public NewtonRaphsonMatrixSolutionWithEvaluatedTolerances(
                double[] X,
                double[] dXs,
                double[] absoluteMaxErrors,
                double[] absoluteTargetErrors,
                double[] relativeMaxErrors,
                double[] relativeTargetErrors,
                double[] residualMaxErrors,
                double[] residualTargetErrors,
                double[] residual,
                bool fulfilledMaxAbsolute,
                bool fulfilledTargetAbsolute,
                bool fulfilledMaxRelative,
                bool fulfilledTargetRelative,
                bool fulfilledMaxResidual,
                bool fulfilledTargetResidual,
                bool fulfilledMaxAll,
                bool fulfilledDesiredMaxAll,
                bool fulfilledTargetAll,
                double maxErrorAll,
                double targetErrorAll,
                int cyclesRequired = 0,
                bool reachedMaxIterations = false
            )
        {
            this.X = X;
            Dxs = dXs;
            CyclesRequired = cyclesRequired;
            ReachedMaxIterations = reachedMaxIterations;
            AbsoluteMaxErrors = absoluteMaxErrors;
            AbsoluteTargetErrors = absoluteTargetErrors;
            RelativeMaxErrors = relativeMaxErrors;
            RelativeTargetErrors = relativeTargetErrors;
            ResidualMaxErrors = residualMaxErrors;
            ResidualTargetErrors = residualTargetErrors;
            Residual = residual;
            FulfilledMaxAbsolute = fulfilledMaxAbsolute;
            FulfilledTargetAbsolute = fulfilledTargetAbsolute;
            FulfilledMaxRelative = fulfilledMaxRelative;
            FulfilledTargetRelative = fulfilledTargetRelative;
            FulfilledMaxResidual = fulfilledMaxResidual;
            FulfilledTargetResidual = fulfilledTargetResidual;
            FulfilledMaxAll = fulfilledMaxAll;
            FulfilledDesiredMaxAll = fulfilledDesiredMaxAll;
            FulfilledTargetAll = fulfilledTargetAll;
            MaxErrorAll = maxErrorAll;
            TargetErrorAll = targetErrorAll;
        }
        public override string ToString()
        {
            return Json.Serialize(this);
        }
        public NewtonRaphsonMatrixSolutionWithEvaluatedTolerances Decondition(ConditionedSystem conditionedSystem)
        {
            // Descale the primary solution vector X
            double[] descaledX = conditionedSystem.DescaleConditionedMatrixInverseXConditionedVector(this.X);

            // Descale dXs
            double[] descaledDXs = conditionedSystem.DescaleConditionedMatrixXConditionedVector(this.Dxs);

            // Descale absolute errors
            double[] descaledAbsoluteMaxErrors = conditionedSystem.DescaleConditionedMatrixXConditionedVector(this.AbsoluteMaxErrors);
            double[] descaledAbsoluteTargetErrors = conditionedSystem.DescaleConditionedMatrixXConditionedVector(this.AbsoluteTargetErrors);

            // Descale relative errors
            double[] descaledRelativeMaxErrors = conditionedSystem.DescaleConditionedMatrixXConditionedVector(this.RelativeMaxErrors);
            double[] descaledRelativeTargetErrors = conditionedSystem.DescaleConditionedMatrixXConditionedVector(this.RelativeTargetErrors);

            // Descale residual errors
            double[] descaledResidualMaxErrors = conditionedSystem.DescaleConditionedMatrixXConditionedVector(this.ResidualMaxErrors);
            double[] descaledResidualTargetErrors = conditionedSystem.DescaleConditionedMatrixXConditionedVector(this.ResidualTargetErrors);

            // Descale residual vector
            double[] descaledResidual = conditionedSystem.DescaleConditionedMatrixXConditionedVector(this.Residual);

            // Return a new instance of the solution with descaled values
            return new NewtonRaphsonMatrixSolutionWithEvaluatedTolerances(
                descaledX,
                descaledDXs,
                descaledAbsoluteMaxErrors,
                descaledAbsoluteTargetErrors,
                descaledRelativeMaxErrors,
                descaledRelativeTargetErrors,
                descaledResidualMaxErrors,
                descaledResidualTargetErrors,
                descaledResidual,
                this.FulfilledMaxAbsolute,
                this.FulfilledTargetAbsolute,
                this.FulfilledMaxRelative,
                this.FulfilledTargetRelative,
                this.FulfilledMaxResidual,
                this.FulfilledTargetResidual,
                this.FulfilledMaxAll,
                this.FulfilledDesiredMaxAll,
                this.FulfilledTargetAll,
                this.MaxErrorAll,
                this.TargetErrorAll,
                CyclesRequired,
                ReachedMaxIterations
            );
        }

    }
}
