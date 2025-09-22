using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.Messages.Messages;
using VirtualSockets.DataMemberNames;

namespace Core.VirtualSockets
{
    [DataContract]
    public class VirtualSocket2Message:TypedMessageBase
    {
        [JsonPropertyName(VirtualSocket2MessageDataMemberNames.Secret)]
        [JsonInclude]
        [DataMember(Name = VirtualSocket2MessageDataMemberNames.Secret)]
        public string Secret { get; protected set; }
        [JsonPropertyName(VirtualSocket2MessageDataMemberNames.TheirNodeId)]
        [JsonInclude]
        [DataMember(Name = VirtualSocket2MessageDataMemberNames.TheirNodeId, EmitDefaultValue = false)]
        public int? TheirNodeId { get; protected set; }
        [JsonPropertyName(VirtualSocket2MessageDataMemberNames.EndpointId)]
        [JsonInclude]
        [DataMember(Name = VirtualSocket2MessageDataMemberNames.EndpointId, EmitDefaultValue =false)]
        public long? EndpointId { get; protected set; }
        [JsonPropertyName(VirtualSocketMessageDataMemberNames.Payload)]
        [JsonInclude]
        [DataMember(Name= VirtualSocketMessageDataMemberNames.Payload)]
        public string Payload { get; protected set; }
        public void SetTypeToVirtualSocket2MessageAndDropEndpointIdAndTheirNodeId() {
            Type = global::MessageTypes.MessageTypes.VirtualSocket2Message;
            EndpointId = null;
            TheirNodeId = null;
        }
        public void SetTypeAndDropTheirNodeId(string type)
        {
            Type = type;
            TheirNodeId = null;
        }
    }
}
