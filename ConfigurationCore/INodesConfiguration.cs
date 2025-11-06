namespace ConfigurationCore
{
    public interface INodesConfiguration
    {
        public int[] GetNodeIdsAssociatedWithIdType(int idType);
        public int[] GetAssociatedIdTypes(int nodeId);
        public char GetNodeIdentifierCharacter(int nodeId);
        public int GetNodeIdFromIdentifierCharacter(char c);
        public string[] GetDomainsForNode(int nodeId);
        public string FirstUniqueDomainForNode(int nodeId);
        public string[] UniqueDomainsForNode(int nodeId);
        public string GetIpOrDomainForNode(int nodeId);
        public int IdServerNodeIdDebug { get; }
    }
}