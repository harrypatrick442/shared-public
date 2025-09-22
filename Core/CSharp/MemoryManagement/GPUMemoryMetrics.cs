using Core.DataMemberNames.Messages;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.MemoryManagement
{
    [DataContract]
    public class GPUMemoryMetrics
    {
        public ulong Total { get; protected set; }
        public ulong Used => Total - Free;
        public ulong Free { get; protected set; }
        public GPUMemoryMetrics(ulong total, ulong free)
        {
            Total = total;
            Free = free;
        }
        protected GPUMemoryMetrics() { }
    }
}
