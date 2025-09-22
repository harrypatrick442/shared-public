using MessageTypes.Attributes;

namespace Core.DataMemberNames.Requests
{
    [MessageType(global::MessageTypes.MessageTypes.ErrorMessage)]
    public class ErrorMessageDataMemberNames
    {
        public const string Message = "m";
        public const string Stack = "s";
    }
}