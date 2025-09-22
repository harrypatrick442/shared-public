using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.Messages.Messages;
using Core.DataMemberNames;
using WebAbstract.DataMemberNames.Responses;

namespace WebAPI.Responses
{
    [DataContract]
    public class IATAuthenticateResponse : TicketedMessageBase
    {
        [JsonPropertyName(IATAuthenticateResponseDataMemberNames.UserId)]
        [JsonInclude]
        [DataMember(Name = IATAuthenticateResponseDataMemberNames.UserId,
            EmitDefaultValue = true)]
        public long? UserId{ get;  protected set; }
        protected IATAuthenticateResponse(long? userId, long ticket):base(TicketedMessageType.Ticketed)
        {
            UserId = userId;
            Ticket = ticket;
        }
        protected IATAuthenticateResponse() : base(TicketedMessageType.Ticketed) { }
        public static IATAuthenticateResponse Successful(long userId, long ticket)
        {
            return new IATAuthenticateResponse(userId, ticket);
        }
        public static IATAuthenticateResponse Failed(long ticket)
        { 
            return new IATAuthenticateResponse(null, ticket);
        }
    }
}
