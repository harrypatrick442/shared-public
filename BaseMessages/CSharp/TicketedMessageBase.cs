using Core.DataMemberNames;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using BaseMessages.Constants;

namespace Core.Messages.Messages
{
    [DataContract]
    [KnownType(typeof(TicketedMessageBase))]
    public abstract class TicketedMessageBase :  ITicketedMessageBase
    {
        protected long _Ticket;
        [JsonPropertyName(Ticketing.TICKET)]
        [JsonInclude]
        [DataMember(Name = Ticketing.TICKET)]
        public long Ticket { get { return _Ticket; } set { _Ticket = value; } }
        private string _Type;

        [JsonPropertyName(MessageTypeDataMemberName.Value)]
        [JsonInclude]
        [DataMember(Name = MessageTypeDataMemberName.Value)]
        public string Type { get { return _Type; } protected set { _Type = value; } }
        public TicketedMessageBase(string type)
        {
            _Type = type;
        }
    }
}
