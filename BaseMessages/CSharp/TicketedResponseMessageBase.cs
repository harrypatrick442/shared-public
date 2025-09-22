using Core.Messages.Messages;
using Core.DataMemberNames;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Messages.Responses
{
    /*
    [DataContract]
    public class TicketedResponseMessage<TResponse>: TicketedResponseMessageBase
    {
        private TResponse _Payload;
        [JsonPropertyName("msg")]
        [JsonInclude]
        [DataMember(Name = "msg")]
        public TResponse Payload { get { return _Payload; } protected set { _Payload = value; } }
        public TicketedResponseMessage(long ticket, TResponse payload):base(ticket){
            _Payload = payload;
        }
    }*/

    [DataContract]
    [KnownType(typeof(TicketedResponseMessageBase))]
    public abstract class TicketedResponseMessageBase : TicketedMessageBase
    {
        public TicketedResponseMessageBase(long ticket) : base(TicketedMessageType.Ticketed)
        {
            _Ticket = ticket;
        }
    }
}
