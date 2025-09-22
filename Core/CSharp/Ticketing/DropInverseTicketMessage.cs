using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.DataMemberNames;

namespace Core.Ticketing
{
    [DataContract]
    public class DropInverseTicketMessage
    {
        [JsonPropertyName(MessageTypeDataMemberName.Value)]
        [JsonInclude]
        [DataMember(Name = MessageTypeDataMemberName.Value)]
        public string Type { get { return InverseTicketedSender_MessageTypes.DropInverseTicket; } protected set { } }
        private long _InverseTicket;
        [JsonPropertyName(InverseTicketedDataMemberNames.InverseTicket)]
        [JsonInclude]
        [DataMember(Name = InverseTicketedDataMemberNames.InverseTicket)]
        public long InverseTicket
        {
            get { return _InverseTicket; }
            protected set { _InverseTicket = value; }
        }
        public DropInverseTicketMessage(long inverseTicket)
        {
            _InverseTicket = inverseTicket;
        }
        protected DropInverseTicketMessage() { }
    }
}
