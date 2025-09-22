using System.Text;

namespace Core.Maths.IterativeSolvers.NewtonRaphson
{
    public class NRFailedExplanation
    {
        public VectorErrors? AbsoluteMaxErrors { get; }
        public VectorErrors? RelativeMaxErrors { get; }
        public VectorErrors? ResidualMaxErrors { get; }
        public VectorErrors? AbsoluteTargetErrors { get; }
        public VectorErrors? RelativeTargetErrors { get; }
        public VectorErrors? ResidualTargetErrors { get; }
        public NRFailedExplanation(
            VectorErrors? absoluteMaxErrors,
            VectorErrors? relativeMaxErrors,
            VectorErrors? residualMaxErrors,
            VectorErrors? absoluteTargetErrors,
            VectorErrors? relativeTargetErrors,
            VectorErrors? residualTargetErrors)
        { 
            AbsoluteMaxErrors = absoluteMaxErrors;
            RelativeMaxErrors = relativeMaxErrors;
            ResidualMaxErrors = residualMaxErrors;
            AbsoluteTargetErrors = absoluteTargetErrors;
            RelativeTargetErrors = relativeTargetErrors;
            ResidualTargetErrors = residualTargetErrors;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            CreateString(sb, nameof(AbsoluteMaxErrors), AbsoluteMaxErrors);
            CreateString(sb, nameof(RelativeMaxErrors), RelativeMaxErrors);
            CreateString(sb, nameof(ResidualMaxErrors), ResidualMaxErrors);
            CreateString(sb, nameof(AbsoluteTargetErrors), AbsoluteTargetErrors);
            CreateString(sb, nameof(RelativeTargetErrors), RelativeTargetErrors);
            CreateString(sb, nameof(ResidualTargetErrors), ResidualTargetErrors);
            return sb.ToString();
        }
        private void CreateString(StringBuilder sb, string name, VectorErrors? vectorErrors) {
            if (vectorErrors == null) return;
            sb.Append(name);
            sb.Append(" failed because of [index](error): ");
            bool first = true;
            foreach (var rowError in vectorErrors.RowErrors) {
                if (first) first = false;
                else sb.Append(", ");
                sb.Append("[");
                sb.Append(rowError.Index.ToString());
                sb.Append("](");
                sb.Append(rowError.NormalizedError.ToString());
                sb.Append(")");
            }
            sb.AppendLine(".");
        }
    }
}
