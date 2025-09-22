namespace Core.Maths.IterativeSolvers.NewtonRaphson.Delegates
{
    internal delegate NewtonRaphsonMatrixSolutionWithEvaluatedTolerances DelegateNewtonRaphsonEvaluateTolerances(double[] currentX, double[] lastX, double[] residual);
}
