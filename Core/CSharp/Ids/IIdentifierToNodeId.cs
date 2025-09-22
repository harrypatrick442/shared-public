
namespace Core.Ids
{
    public interface IIdentifierToNodeId<TIdentifier>
    {
        public int[] AllNodesIds { get; }
        public abstract int GetNodeIdFromIdentifier(TIdentifier identifier);
    }
}