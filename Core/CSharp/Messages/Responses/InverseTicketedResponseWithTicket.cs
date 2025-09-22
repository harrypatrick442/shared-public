using Core.Interfaces;
using JSON;
using Core.DataMemberNames;
using Core.Ticketing;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.Messages.Messages;

namespace Core.Messages.Responses
{
    [DataContract]
    public class InverseTicketedResponseWithTicket : TicketedMessageBase, IInverseTicketed
    {
        private long _InverseTicket;
        [JsonPropertyName(InverseTicketedDataMemberNames.InverseTicket)]
        [JsonInclude]
        [DataMember(Name = InverseTicketedDataMemberNames.InverseTicket)]
        public long InverseTicket { get { return _InverseTicket; } set { _InverseTicket = value; } }
        public InverseTicketedResponseWithTicket(long inverseTicket) : base(InverseTicketedSender_MessageTypes.InverseTicketed)
        {
            _InverseTicket = inverseTicket;
        }
        protected InverseTicketedResponseWithTicket() : base(InverseTicketedSender_MessageTypes.InverseTicketed)
        { }
    }
}
