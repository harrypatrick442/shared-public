using MessageTypes.Attributes;
using MessageTypes.Internal;

namespace WebAbstract.DataMemberNames.Interserver.Messages
{
    [MessageType(InterserverMessageTypes.BroadcastLoadFactor)]
    public static class BroadcastNodeLoadingMessageDataMemberNames
    {
        public const string NodeId = "n";
        public const string LoadFactor = "l";
        public const string LoadFactorType = "t";
    }
}