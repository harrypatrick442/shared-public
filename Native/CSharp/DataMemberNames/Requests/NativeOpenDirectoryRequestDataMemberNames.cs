using MessageTypes.Attributes;
using Core.DataMemberNames;
using System.Net.NetworkInformation;

namespace Native.DataMemberNames.Requests
{
    [MessageType(MessageTypes.NativeOpenDirectory)]
    public static class NativeOpenDirectoryRequestDataMemberNames
    {
        public const string DirectoryPath = "p";
    }
}