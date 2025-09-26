using MessageTypes.Attributes;

namespace VirtualSockets.DataMemberNames
{
    [MessageType(MessageTypes.VirtualSocket2Message)]
    public static class VirtualSocket2MessageDataMemberNames
    {
        public const string Secret = "s";
        public const string EndpointId = "i";
        public const string TheirNodeId = "n";
        public const string Payload = "p";
    }
}