using Core.DataMemberNames;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.Messages.Messages;

namespace Native.Responses
{
    [DataContract]
    public class NativeStorageDeleteAllResponse : TicketedMessageBase
    {
        public NativeStorageDeleteAllResponse(long ticket) : base(TicketedMessageType.Ticketed)
        {
            _Ticket = ticket;
        }
        public NativeStorageDeleteAllResponse() : base(TicketedMessageType.Ticketed)
        {
        }
    }
}
