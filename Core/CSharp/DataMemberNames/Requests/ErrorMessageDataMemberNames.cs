using MessageTypes.Attributes;

namespace Core.DataMemberNames.Requests
{
    [MessageType(MessageTypes.ErrorMessage)]
    public class ErrorMessageDataMemberNames
    {
        public const string Message = "m";
        public const string Stack = "s";
    }
}