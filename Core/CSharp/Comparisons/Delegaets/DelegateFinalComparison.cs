namespace Core.Comparisons.Delegaets
{
    public delegate TBeingCompared DelegateFinalComparison<TBeingCompared>(TBeingCompared a, TBeingCompared b, DelegateTest<TBeingCompared> highestLevelTestPassed);
}