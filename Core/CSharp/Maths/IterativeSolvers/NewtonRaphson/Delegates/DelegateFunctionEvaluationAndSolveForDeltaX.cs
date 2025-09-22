using System.Threading;

namespace Core.Maths.IterativeSolvers.NewtonRaphson.Delegates
{
    /// <summary>
    /// residual/function evaluation and solve for delta x
    /// </summary>
    /// <returns></returns>
    public delegate void DelegateFunctionEvaluationAndSolveForDeltaX(
        out double[] residual, out double[] xAtEndOfIteration, 
        CancellationToken cancellationToken);
}
