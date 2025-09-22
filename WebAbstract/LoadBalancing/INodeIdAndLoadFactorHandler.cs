namespace Core.LoadBalancing
{
    public interface INodeIdAndLoadFactorHandler
    {
        public LoadFactorType LoadFactorType { get; }
        public void EndpointWentOffline(int nodeId);
        public void GotNodeIdAndLoadFactor(NodeIdAndLoadFactor nodeIdAndLoadFactor);
    }
}