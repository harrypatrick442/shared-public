
namespace Core.Ids
{
    public class FixedNodeIdentifierToNodeId:IIdentifierToNodeId<long>, IIdentifierToNodeId<string>
    {
        private int _NodeId;
        public FixedNodeIdentifierToNodeId(int nodeId) {
            _NodeId = nodeId;
        }
        public int[] AllNodesIds => new int[] { _NodeId };
        public int GetNodeIdFromIdentifier(string identifier) {
            return _NodeId;
        }
        public int GetNodeIdFromIdentifier(long identifier)
        {
            return _NodeId;
        }
    }
}