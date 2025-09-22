using MessageTypes.Attributes;
using Core.DataMemberNames;
using System.Net.NetworkInformation;

namespace Native.DataMemberNames.Messages
{
    [MessageType(global::MessageTypes.MessageTypes.NativeCloseFile)]
    public static class NativeCloseFileMessageDataMemberNames
    {
        public const string Identifier = "i";
    }
}