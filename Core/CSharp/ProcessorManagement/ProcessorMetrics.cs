using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.DataMemberNames.Messages;
namespace Core.MemoryManagement
{
    [DataContract]
    public class ProcessorMetrics
    {
        [JsonPropertyName(ProcessorMetricsDataMemberNames.PercentTotal)]
        [JsonInclude]
        [DataMember(Name = ProcessorMetricsDataMemberNames.PercentTotal)]
        public double PercentCpuUsageByAllProcesses { get; protected set; }
        [JsonPropertyName(ProcessorMetricsDataMemberNames.PercentByMe)]
        [JsonInclude]
        [DataMember(Name = ProcessorMetricsDataMemberNames.PercentByMe)]
        public int PercentCpuUsageByMe { get; protected set; }
        public ProcessorMetrics(int percentCpuUsageByMe, double percentCpuUsageByAllProcesses)
        {
            PercentCpuUsageByMe = percentCpuUsageByMe;
            PercentCpuUsageByAllProcesses = percentCpuUsageByAllProcesses;
        }
        protected ProcessorMetrics() { }
    }
}
