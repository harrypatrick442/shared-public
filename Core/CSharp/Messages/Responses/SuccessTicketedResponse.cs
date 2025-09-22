using Core.DataMemberNames;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.Messages.Messages;

namespace Core.Messages.Responses
{
    [DataContract]
    public class SuccessTicketedResponse : TicketedMessageBase
    {
        private bool _Success;
        [JsonPropertyName(SuccessTicketedResponseDataMemberNames.Success)]
        [JsonInclude]
        [DataMember(Name = SuccessTicketedResponseDataMemberNames.Success)]
        public bool Success
        {
            get { return _Success; }
            protected set { _Success = value; }
        }
        public SuccessTicketedResponse(bool success, long ticket)
            : base(TicketedMessageType.Ticketed)
        {
            _Success = success;
            _Ticket = ticket;
        }
        protected SuccessTicketedResponse()
            : base(TicketedMessageType.Ticketed) { }
    }
}
