using MessageTypes.Attributes;

namespace Native.DataMemberNames.Messages
{
    [MessageType(MessageTypes.ConsoleMessage)]
    public static class ConsoleMessageDataMemberNames
    {
        public const string Message = "m";
        public const string IsError = "e";
    }
}
