using MessageTypes.Attributes;
using Core.DataMemberNames;
using System.Net.NetworkInformation;

namespace Native.DataMemberNames.Messages
{
    [MessageType(global::MessageTypes.MessageTypes.NativePlatform)]
    public static class NativePlatformMessageDataMemberNames
    {
        public const string Platform = "p";
    }
}