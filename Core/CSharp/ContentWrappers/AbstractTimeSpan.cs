using Core.DataMemberNames.Messages;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Content
{
    [DataContract]
    public class AbstractTimeSpan
    {
        private long _MillisecondsUTC;
        [JsonPropertyName(AbstractTimeSpanDataMemberNames.MillisecondsUTC)]
        [JsonInclude]
        [DataMember(Name = AbstractTimeSpanDataMemberNames.MillisecondsUTC)]
        public long MillisecondsUTC { get { return _MillisecondsUTC; } protected set { _MillisecondsUTC = value; } }
        private int _Days;
        [JsonPropertyName(AbstractTimeSpanDataMemberNames.Days)]
        [JsonInclude]
        [DataMember(Name = AbstractTimeSpanDataMemberNames.Days)]
        public int Days { get { return _Days; } protected set { _Days = value; } }
        private int _Months;
        [JsonPropertyName(AbstractTimeSpanDataMemberNames.Months)]
        [JsonInclude]
        [DataMember(Name = AbstractTimeSpanDataMemberNames.Months)]
        public int Months{ get { return _Months; } protected set { _Months = value; } }
        private int _Years;
        [JsonPropertyName(AbstractTimeSpanDataMemberNames.Years)]
        [JsonInclude]
        [DataMember(Name = AbstractTimeSpanDataMemberNames.Years)]
        public int Years { get { return _Years; } protected set { _Years = value; } }
        private int _Hours;
        [JsonPropertyName(AbstractTimeSpanDataMemberNames.Hours)]
        [JsonInclude]
        [DataMember(Name = AbstractTimeSpanDataMemberNames.Hours)]
        public int Hours{ get { return _Hours; } protected set { _Hours = value; } }
        private int _Minutes;
        [JsonPropertyName(AbstractTimeSpanDataMemberNames.Minutes)]
        [JsonInclude]
        [DataMember(Name = AbstractTimeSpanDataMemberNames.Minutes)]
        public int Minutes { get { return _Minutes; } protected set { _Minutes= value; } }
        private int _Seconds;
        [JsonPropertyName(AbstractTimeSpanDataMemberNames.Seconds)]
        [JsonInclude]
        [DataMember(Name = AbstractTimeSpanDataMemberNames.Seconds)]
        public int Seconds { get { return _Seconds; } protected set { _Seconds = value; } }
        protected AbstractTimeSpan() { }
    }
}
