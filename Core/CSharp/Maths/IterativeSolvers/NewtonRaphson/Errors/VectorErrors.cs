using Core.Maths.IterativeSolvers.NewtonRaphson.Enums;

namespace Core.Maths.IterativeSolvers.NewtonRaphson
{
    public class VectorErrors
    {
        public NRVectorType VectorType { get; private set; }
        public RowError[] RowErrors{ get; private set; }
        public VectorErrors(NRVectorType vectorType, RowError[] rowErrors)
        {
            VectorType = vectorType;
            RowErrors = rowErrors;
        }
    }
}
