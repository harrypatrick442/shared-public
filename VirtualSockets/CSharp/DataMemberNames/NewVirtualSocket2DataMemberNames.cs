using MessageTypes.Attributes;

namespace VirtualSockets.DataMemberNames
{
    [MessageType(MessageTypes.VirtualSocket2Message)]
    public static class NewVirtualSocket2DataMemberNames
    {
        public const string Secret = "s";
        public const string EndpointId = "i";
        public const string TheirNodeId = "n";
    }
}