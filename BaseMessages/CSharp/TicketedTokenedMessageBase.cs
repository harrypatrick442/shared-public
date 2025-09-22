using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using BaseMessages.DataMemberNames;

namespace Core.Messages.Messages
{
    [DataContract]
    [KnownType(typeof(TicketedMessageBase))]
    public abstract class TicketedTokenedMessageBase : TicketedMessageBase
    {
        [JsonPropertyName(TokenDataMemberName.Token)]
        [JsonInclude]
        [DataMember(Name = TokenDataMemberName.Token)]
        public string Token { get ; set; }
        public TicketedTokenedMessageBase(string type):base(type)
        {

        }
    }
}
