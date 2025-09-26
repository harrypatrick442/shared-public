namespace WebAbstract.LoadBalancing
{
    public class NodeIdAndLoadFactor
    {
        public  int NodeId { get; protected set; }
        public double? LoadFactor { get; protected set; }
        public NodeIdAndLoadFactor(int nodeId, double? loadFactor) {
            NodeId = nodeId;
            LoadFactor = loadFactor;
        }
    }
}
