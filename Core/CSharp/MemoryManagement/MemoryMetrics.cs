using Core.DataMemberNames.Messages;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.MemoryManagement
{
    [DataContract]
    public class MemoryMetrics
    {
        [JsonPropertyName(MemoryMetricsDataMemberNames.TotalMb)]
        [JsonInclude]
        [DataMember(Name = MemoryMetricsDataMemberNames.TotalMb)]
        public int TotalMb { get; protected set; }
        public long Total { get { return 1000000L* ((long)TotalMb); } }
        [JsonPropertyName(MemoryMetricsDataMemberNames.UsedMb)]
        [JsonInclude]
        [DataMember(Name = MemoryMetricsDataMemberNames.UsedMb)]
        public int UsedMb { get; protected set; }
        public long Used { get { return 1000000L * ((long)UsedMb); } }
        [JsonPropertyName(MemoryMetricsDataMemberNames.FreeMb)]
        [JsonInclude]
        [DataMember(Name = MemoryMetricsDataMemberNames.FreeMb)]
        public int FreeMb { get; protected set; }
        public long Free { get { return 1000000L * ((long)FreeMb); } }
        public MemoryMetrics(int totalMb, int usedMb, int freeMb)
        {
            TotalMb = totalMb;
            UsedMb = usedMb;
            FreeMb = freeMb;
        }
        protected MemoryMetrics() { }
    }
}
