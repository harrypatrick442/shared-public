using Core.Interfaces;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Messages
{
    public class TypeTicketedAndWholePayload:ITypedMessage
    {
        public string Type { get; }
        public long Ticket { get; }
        public string JsonString { get; }
        public TypeTicketedAndWholePayload(string type, long ticket, string jsonString) {
            Type = type;
            Ticket = ticket;
            JsonString = jsonString;
        }
        
    }
}
