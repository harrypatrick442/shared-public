using MessageTypes.Attributes;
using Core.DataMemberNames;
using System.Net.NetworkInformation;

namespace Native.DataMemberNames.Requests
{
    [MessageType(MessageTypes.NativeStorageGetString)]
    public static class NativeStorageGetStringRequestDataMemberNames
    {
        public const string Key = "k";
    }
}