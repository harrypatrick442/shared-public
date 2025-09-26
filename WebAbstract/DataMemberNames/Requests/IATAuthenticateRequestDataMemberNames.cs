using MessageTypes.Attributes;

namespace WebAbstract.DataMemberNames.Requests
{
    [MessageType(MessageTypes.IATAuthentication)]
    public static class IATAuthenticateRequestDataMemberNames
    {
        public const string NodeId = "n";
        public const string SessionId = "s";
        public const string Token = "t";
    }
}