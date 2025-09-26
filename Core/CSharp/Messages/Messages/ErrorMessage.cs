using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.DataMemberNames;
using Core.DataMemberNames.Requests;

namespace Core.Messages.Messages
{
    [DataContract]
    public class ErrorMessage
    {
        [JsonPropertyName(MessageTypeDataMemberName.Value)]
        [JsonInclude]
        [DataMember(Name = MessageTypeDataMemberName.Value)]

        public string Type { get { return MessageTypes.ErrorMessage; } protected set { } }
        private string _Message;
        [JsonPropertyName(ErrorMessageDataMemberNames.Message)]
        [JsonInclude]
        [DataMember(Name=ErrorMessageDataMemberNames.Message)]
        public string Message { get { return _Message; } protected set { _Message = value; } }
        private string _Stack;
        [JsonPropertyName(ErrorMessageDataMemberNames.Stack)]
        [JsonInclude]
        [DataMember(Name=ErrorMessageDataMemberNames.Stack)]
        public string Stack { get { return _Stack; } protected set { _Stack = value; } }
        protected ErrorMessage() { }
    }
}
