using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace Core.DTOs
{
    [DataContract]
    [KnownType(typeof(AlarmsAtTime))]
    public class AlarmsAtTime
    {
        private List<int>_AlarmIds;
        [JsonPropertyName("i")]
        [JsonInclude]
        [DataMember(Name ="i")]
        public List<int> AlarmIds { get { return _AlarmIds; } protected set { _AlarmIds = value; } }
        private long _Time;
        [JsonPropertyName("t")]
        [JsonInclude]
        [DataMember(Name = "t")]
        public long Time { get { return _Time; } protected set { _Time = value; } }
        public AlarmsAtTime(int[] alarmIds, long time) {
            _AlarmIds = alarmIds.ToList();
            _Time = time;
        }
        protected AlarmsAtTime() { }
    }
}
