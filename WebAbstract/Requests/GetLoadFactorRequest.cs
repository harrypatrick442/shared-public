using Core.LoadBalancing;
using Core.Messages.Messages;
using MessageTypes.Internal;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using WebAbstract.DataMemberNames.Interserver.Requests;

namespace WebAPI.Requests
{
    [DataContract]
    public class GetLoadFactorRequest : TicketedMessageBase
    {
        [JsonPropertyName(GetLoadFactorRequestDataMemberNames.LoadFactorType)]
        [JsonInclude]
        [DataMember(Name = GetLoadFactorRequestDataMemberNames.LoadFactorType)]
        public LoadFactorType LoadFactorType { get; protected set; }
        public GetLoadFactorRequest(LoadFactorType loadFactorType) : base(InterserverMessageTypes.GetLoadFactor)
        {
            LoadFactorType = loadFactorType;
        }
        protected GetLoadFactorRequest() : base(InterserverMessageTypes.GetLoadFactor)
        { }
    }
}
