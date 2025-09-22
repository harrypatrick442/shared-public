using MessageTypes.Internal;
using Core.DataMemberNames;
using System.Net.NetworkInformation;

namespace Native.DataMemberNames.Responses
{
    public static class NativeShowSaveFilePickerResponseDataMemberNames
    {
        public const string Success = "s",
            FileIdentifier = "fi",
            FileName = "fn",
            FileType = "ft";
    }
}