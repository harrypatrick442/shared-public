namespace Core.Maths.IterativeSolvers.NewtonRaphson.Delegates
{
    internal delegate bool DelegateEvaluateSolutionMatrixForm(double[] currentX, double[] residual,
        out NewtonRaphsonMatrixSolutionWithEvaluatedTolerances? solution, out bool reachedMaxIterations);
}
