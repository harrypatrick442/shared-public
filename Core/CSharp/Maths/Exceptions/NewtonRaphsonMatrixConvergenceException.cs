using Core.Maths.Enums;
using Core.Maths.IterativeSolvers.NewtonRaphson;
using System;
namespace Core.Maths.Exceptions
{
    public class NewtonRaphsonMatrixConvergenceException : ConvergenceException
    {
        public NewtonRaphsonMatrixSolutionWithEvaluatedTolerances Solution { get; }
        public NewtonRaphsonMatrixConvergenceException(
            ConvergenceFailureType failureType,
            Func<string> getMessage,
            NewtonRaphsonMatrixSolutionWithEvaluatedTolerances solution)
            :base(failureType, getMessage)
        {
            Solution = solution;
        }
        public NewtonRaphsonMatrixConvergenceException(
            ConvergenceFailureType failureType,
            Func<string> getMessage,
            NewtonRaphsonMatrixSolutionWithEvaluatedTolerances solution,
            Exception innerException)
            : base(failureType, getMessage, innerException)
        {
            Solution = solution;
        }
    }
}