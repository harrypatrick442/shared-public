using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.Messages.Messages;
using Core.DataMemberNames;
using Core.Machine;
using WebAbstract.DataMemberNames.Interserver.Responses;

namespace WebAPI.Responses
{
    [DataContract]
    public class GetLoadFactorResponse : TicketedMessageBase
    {
        [JsonPropertyName(GetLoadFactorResponseDataMemberNames.LoadFactor)]
        [JsonInclude]
        [DataMember(Name = GetLoadFactorResponseDataMemberNames.LoadFactor,
            EmitDefaultValue = true)]
        public double? LoadFactor{ get;  protected set; }
        protected GetLoadFactorResponse(double? loadFactor, long ticket):base(TicketedMessageType.Ticketed)
        {
            LoadFactor = loadFactor;
            Ticket = ticket;
        }
        protected GetLoadFactorResponse() : base(TicketedMessageType.Ticketed)
        { }
        public static GetLoadFactorResponse Successful(double loadFactor, long ticket)
        {
            return new GetLoadFactorResponse(loadFactor, ticket);
        }
        public static GetLoadFactorResponse Failed(long ticket)
        { 
            return new GetLoadFactorResponse(null, ticket);
        }
    }
}
