using Core.LoadBalancing;
using Core.Messages.Messages;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using WebAbstract.DataMemberNames.Requests;

namespace WebAbstract.Requests
{
    [DataContract]
    public class IATAuthenticateRequest : TicketedMessageBase
    {
        [JsonPropertyName(IATAuthenticateRequestDataMemberNames.NodeId)]
        [JsonInclude]
        [DataMember(Name = IATAuthenticateRequestDataMemberNames.NodeId)]
        public int NodeId { get; protected set; }
        [JsonPropertyName(IATAuthenticateRequestDataMemberNames.SessionId)]
        [JsonInclude]
        [DataMember(Name = IATAuthenticateRequestDataMemberNames.SessionId)]
        public long SessionId { get; protected set; }
        [JsonPropertyName(IATAuthenticateRequestDataMemberNames.Token)]
        [JsonInclude]
        [DataMember(Name = IATAuthenticateRequestDataMemberNames.Token)]
        public string Token { get; protected set; }
        public IATAuthenticateRequest() : base(MessageTypes.IATAuthentication)
        {

        }
    }
}
