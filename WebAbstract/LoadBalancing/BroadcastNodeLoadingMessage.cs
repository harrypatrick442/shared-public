using Core.Messages.Messages;
using MessageTypes.Internal;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using WebAbstract.DataMemberNames.Interserver.Messages;

namespace Core.LoadBalancing
{
    [DataContract]
    public class BroadcastNodeLoadingMessage:TypedMessageBase
    {
        [JsonPropertyName(BroadcastNodeLoadingMessageDataMemberNames.NodeId)]
        [JsonInclude]
        [DataMember(Name =BroadcastNodeLoadingMessageDataMemberNames.NodeId)]
        public  int NodeId { get; protected set; }
        [JsonPropertyName(BroadcastNodeLoadingMessageDataMemberNames.LoadFactor)]
        [JsonInclude]
        [DataMember(Name = BroadcastNodeLoadingMessageDataMemberNames.LoadFactor)]
        public double LoadFactor { get; protected set; }
        [JsonPropertyName(BroadcastNodeLoadingMessageDataMemberNames.LoadFactorType)]
        [JsonInclude]
        [DataMember(Name = BroadcastNodeLoadingMessageDataMemberNames.LoadFactorType)]
        public LoadFactorType LoadFactorType { get; protected set; }
        public BroadcastNodeLoadingMessage(int nodeId, double loadFactor, LoadFactorType loadFactorTYpe) :base(){
            Type = InterserverMessageTypes.BroadcastLoadFactor;
            NodeId = nodeId;
            LoadFactor = loadFactor;
            LoadFactorType = loadFactorTYpe;
        }
        protected BroadcastNodeLoadingMessage() { }
    }
}
