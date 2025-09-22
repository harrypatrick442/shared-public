using Core.MemoryManagement;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.DataMemberNames.Messages;
namespace Core.Machine
{
    [DataContract]
	public class MachineMetrics
    {
        [JsonPropertyName(MachineMetricsDataMemberNames.Memory)]
        [JsonInclude]
        [DataMember(Name = MachineMetricsDataMemberNames.Memory)]
        public MemoryMetrics Memory { get; protected set; }
        [JsonPropertyName(MachineMetricsDataMemberNames.Processor)]
        [JsonInclude]
        [DataMember(Name = MachineMetricsDataMemberNames.Processor)]
        public ProcessorMetrics Processor { get; protected set; }
        public MachineMetrics(MemoryMetrics memoryMetrics, ProcessorMetrics processorMetrics)
        {
            Memory= memoryMetrics;
            Processor = processorMetrics;
        }
        protected MachineMetrics() { }
    }
}
