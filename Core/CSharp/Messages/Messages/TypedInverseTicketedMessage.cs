using Core.Interfaces;
using Core.Ticketing;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Messages.Messages
{
    [DataContract]
    [KnownType(typeof(TypedInverseTicketedMessage))]
    public class TypedInverseTicketedMessage : TicketedMessageBase, IInverseTicketed
    {
        protected long _InverseTicket;
        [JsonPropertyName(InverseTicketedDataMemberNames.InverseTicket)]
        [JsonInclude]
        [DataMember(Name = InverseTicketedDataMemberNames.InverseTicket)]
        public long InverseTicket { get { return _InverseTicket; } set { _InverseTicket = value; } }
        public TypedInverseTicketedMessage(string type) : base(type)
        {

        }
        protected TypedInverseTicketedMessage() : base(null)
        { }
    }
}
