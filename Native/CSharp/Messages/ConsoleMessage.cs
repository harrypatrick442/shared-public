using Core.Messages.Messages;
using Native.DataMemberNames.Messages;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Native.Messages
{
    [DataContract]
    public class ConsoleMessage : TypedMessageBase
    {
        [JsonPropertyName(ConsoleMessageDataMemberNames.Message)]
        [JsonInclude]
        [DataMember(Name = ConsoleMessageDataMemberNames.Message)]
        public string Message { get; protected set; }
        [JsonPropertyName(ConsoleMessageDataMemberNames.IsError)]
        [JsonInclude]
        [DataMember(Name = ConsoleMessageDataMemberNames.IsError)]
        public bool IsError { get; protected set; }
        public ConsoleMessage(string message, bool isError = false)
            : base()
        {
            Type = MessageTypes.ConsoleMessage;
            Message = message;
            IsError = isError;
        }
        protected ConsoleMessage()
            : base()
        {
            Type = MessageTypes.ConsoleMessage;
        }
    }
}
