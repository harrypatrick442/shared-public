using MessageTypes.Attributes;
using Core.DataMemberNames;
using System.Net.NetworkInformation;

namespace Native.DataMemberNames.Messages
{
    [MessageType(MessageTypes.NativePermissionsUpdate)]
    public static class NativePermissionsUpdateMessageDataMemberNames
    {
        public const string HasAllRequired = "a";
    }
}