using System.Text.Json.Serialization;

namespace TapoDevices.DeviceInfos
{

    public class DeviceInfoBulb : DeviceInfo
    {
        [JsonPropertyName("brightness")]
        public int Brightness { get; set; }

        [JsonPropertyName("hue")]
        public int Hue { get; set; }

        [JsonPropertyName("saturation")]
        public int Saturation { get; set; }

        [JsonPropertyName("color_temp")]
        public int ColorTemperature { get; set; }

        [JsonPropertyName("color_temp_range")]
        public int[] ColorTemperatureRange { get; set; }
    }
}
