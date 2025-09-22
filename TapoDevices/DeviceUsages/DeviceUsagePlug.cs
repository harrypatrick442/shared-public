using System;
using System.Text;
using System.Text.Json.Serialization;

namespace TapoDevices.DeviceUsages
{
    public class DeviceUsagePlug : DeviceUsageBase
    {
        [JsonPropertyName("time_usage")]
        public TimeUsage TimeUsage { get; set; }
    }
}
