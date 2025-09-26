using MessageTypes.Attributes;
using Core.DataMemberNames;
using System.Net.NetworkInformation;

namespace Native.DataMemberNames.Requests
{
    [MessageType(MessageTypes.NativeDownloadFile)]
    public static class NativeDownloadFileRequestDataMemberNames
    {
        public const string ThroughServerReceiveUrl = "r";
        public const string FileName = "f";
    }
}