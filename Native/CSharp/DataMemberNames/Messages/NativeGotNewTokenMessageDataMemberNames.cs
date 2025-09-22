using MessageTypes.Attributes;
using Core.DataMemberNames;
using System.Net.NetworkInformation;

namespace Native.DataMemberNames.Messages
{
    [MessageType(global::MessageTypes.MessageTypes.NativeGotNewToken)]
    public static class NativeGotNewTokenMessageDataMemberNames
    {
        public const string Token = "t";
    }
}