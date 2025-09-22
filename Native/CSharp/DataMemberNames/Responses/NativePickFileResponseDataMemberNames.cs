using MessageTypes.Internal;
using Core.DataMemberNames;
using System.Net.NetworkInformation;

namespace Native.DataMemberNames.Responses
{
    public static class NativePickFileResponseDataMemberNames
    {
        public const string Success = "s",
            FileIdentifier = "fi",
            FileName = "fn",
            FileSize = "fs",
            FileType = "ft";
    }
}