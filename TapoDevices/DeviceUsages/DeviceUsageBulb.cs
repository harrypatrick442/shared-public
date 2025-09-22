using System;
using System.Text;
using System.Text.Json.Serialization;

namespace TapoDevices.DeviceUsages
{
    public class DeviceUsageBulb : DeviceUsageBase
    {
        [JsonPropertyName("time_usage")]
        public TimeUsage TimeUsage { get; set; }

        [JsonPropertyName("power_usage")]
        public TimeUsage PowerUsage { get; set; }

        [JsonPropertyName("saved_power")]
        public TimeUsage SavedPower { get; set; }
    }
}
