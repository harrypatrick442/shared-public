using MessageTypes.Attributes;
using Core.DataMemberNames;
using System.Net.NetworkInformation;

namespace Native.DataMemberNames.Requests
{
    [MessageType(global::MessageTypes.MessageTypes.NativeStorageSetString)]
    public static class NativeStorageSetStringRequestDataMemberNames
    {
        public const string Key = "k";
        public const string Value = "v";
    }
}