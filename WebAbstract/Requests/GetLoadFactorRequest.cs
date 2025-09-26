using Core.Messages.Messages;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using WebAbstract.DataMemberNames.Interserver.Requests;
using WebAbstract.LoadBalancing;

namespace WebAbstract.Requests
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
