using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.Messages.Messages;
using VirtualSockets.DataMemberNames;

namespace VirtualSockets
{
    [DataContract]
    public class NewVirtualSocket2:TypedMessageBase
    {
        [JsonPropertyName(NewVirtualSocket2DataMemberNames.Secret)]
        [JsonInclude]
        [DataMember(Name = NewVirtualSocket2DataMemberNames.Secret)]
        public string Secret { get; protected set; }
        [JsonPropertyName(NewVirtualSocket2DataMemberNames.TheirNodeId)]
        [JsonInclude]
        [DataMember(Name = NewVirtualSocket2DataMemberNames.TheirNodeId)]
        public long TheirNodeId { get; protected set; }
        [JsonPropertyName(NewVirtualSocket2DataMemberNames.EndpointId)]
        [JsonInclude]
        [DataMember(Name = NewVirtualSocket2DataMemberNames.EndpointId)]
        public long EndpointId { get; protected set; }
        public NewVirtualSocket2(string secret, long theirNodeId, long endpointId) {
            Secret = secret;
            TheirNodeId = theirNodeId;
            EndpointId = endpointId;
        }
        protected NewVirtualSocket2() { }
    }
}
