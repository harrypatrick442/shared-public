namespace Core.Maths.Enums
{
    public enum ConvergenceFailureType
    {
        Unspecified,
        CouldNotBacktrack,
        DerivativeTooSmall,
        ExceededMaxIterations,
        UnableToShrinkTimestep,
        ErrorIncreasingWithShrinkingTimestep,
        NewtonRaphson,
        Validation
    }
}
