using MessageTypes.Attributes;
using Core.DataMemberNames;
using System.Net.NetworkInformation;

namespace Native.DataMemberNames.Messages
{
    [MessageType(global::MessageTypes.MessageTypes.NativeDisposeClientToClientChannel)]
    public static class NativeDisposeClientToClientChannelMessageDataMemberNames
    {
        public const string VirtualSocketId = "v";
    }
}