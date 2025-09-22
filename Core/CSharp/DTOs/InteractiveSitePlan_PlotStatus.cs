using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SnippetsDatabase.DTOs
{
    [DataContract]
    public class InteractiveSitePlan_PlotStatus
    {
        private string _Name;
        [JsonPropertyName("name")]
        [JsonInclude]
        [DataMember(Name="name")]
        public string Name { get { return _Name; } protected set { _Name = value; } }
        private string _PlotMarkerColor;
        [JsonPropertyName("plotMarkerColor")]
        [JsonInclude]
        [DataMember(Name = "plotMarkerColor")]
        public string PlotMarkerColor { get { return _PlotMarkerColor; } protected set { _PlotMarkerColor = value; } }
        protected InteractiveSitePlan_PlotStatus() { }
    }
}
