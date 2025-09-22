namespace Core.LoadBalancing
{
    public interface ILoadFactorSource
    {
        LoadFactorType LoadFactorType { get; }
        double GetLoadFactor();
    }
}