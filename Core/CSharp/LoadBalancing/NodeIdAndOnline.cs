using Core.Machine;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.DataMemberNames.Messages;

namespace Core.LoadBalancing
{
    [DataContract]
    public class NodeIdAndOnline
    {
        [JsonPropertyName(NodeIdAndOnlineDataMemberNames.NodeId)]
        [JsonInclude]
        [DataMember(Name =NodeIdAndOnlineDataMemberNames.NodeId)]
        public int NodeId { get; protected set; }
        [JsonPropertyName(NodeIdAndOnlineDataMemberNames.Online)]
        [JsonInclude]
        [DataMember(Name = NodeIdAndOnlineDataMemberNames.Online)]
        public bool Online { get; protected set; }
        [JsonPropertyName(NodeIdAndOnlineDataMemberNames.MachineMetrics)]
        [JsonInclude]
        [DataMember(Name = NodeIdAndOnlineDataMemberNames.MachineMetrics)]
        public MachineMetrics MachineMetrics { get; protected set; }
        public NodeIdAndOnline(int nodeId, bool online, MachineMetrics machineMetrics) {
            NodeId = nodeId;
            Online = online;
            MachineMetrics = machineMetrics;
        }
        protected NodeIdAndOnline() { }
    }
}
