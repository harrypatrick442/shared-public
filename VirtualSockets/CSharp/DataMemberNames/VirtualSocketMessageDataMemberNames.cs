using MessageTypes.Attributes;

namespace VirtualSockets.DataMemberNames
{
    [MessageType(global::MessageTypes.MessageTypes.VirtualSocketMessage)]
    public class VirtualSocketMessageDataMemberNames
    {
        public const string Id = "i";
        public const string InternalType = "u";
        public const string Payload = "p";
    }
}