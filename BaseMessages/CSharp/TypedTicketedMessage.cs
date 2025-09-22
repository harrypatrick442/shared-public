using Core.DataMemberNames;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Messages.Messages
{
    [DataContract]
    public class TypedTicketedMessage : TicketedMessageBase
    {
        public TypedTicketedMessage() : base(TicketedMessageType.Ticketed)
        {

        }
    }
}
