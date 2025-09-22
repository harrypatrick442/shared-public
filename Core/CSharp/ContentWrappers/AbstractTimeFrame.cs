using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.Enums;
using Core.DataMemberNames.Messages;

namespace Core.Content
{
    [DataContract]
    public class AbstractTimeFrame
    {
        private long _MillisecondsUTC;
        [JsonPropertyName(AbstractTimeFrameDataMemberNames.MillisecondsUTC)]
        [JsonInclude]
        [DataMember(Name = AbstractTimeFrameDataMemberNames.MillisecondsUTC)]
        public long MillisecondsUTC { get { return _MillisecondsUTC; } protected set { _MillisecondsUTC = value; } }
        private AbstractTimeSpan _RepeatEveryAbstractTimeSpan;
        [JsonPropertyName(AbstractTimeFrameDataMemberNames.RepeatEveryAbstractTimeSpan)]
        [JsonInclude]
        [DataMember(Name = AbstractTimeFrameDataMemberNames.RepeatEveryAbstractTimeSpan)]
        public AbstractTimeSpan RepeatEveryAbstractTimeSpan { get { return _RepeatEveryAbstractTimeSpan; } protected set { _RepeatEveryAbstractTimeSpan = value; } }
        private AbstractTimeSpan _ForDurationAbstractTimeSpan;
        [JsonPropertyName(AbstractTimeFrameDataMemberNames.ForDurationAbstractTimeSpan)]
        [JsonInclude]
        [DataMember(Name = AbstractTimeFrameDataMemberNames.ForDurationAbstractTimeSpan)]
        public AbstractTimeSpan ForDurationAbstractTimeSpan { get { return _ForDurationAbstractTimeSpan; } protected set { _ForDurationAbstractTimeSpan = value; } }
        private int _NRepititionsNullForInfinite;
        [JsonPropertyName(AbstractTimeFrameDataMemberNames.NRepititionsNullForInfinite)]
        [JsonInclude]
        [DataMember(Name = AbstractTimeFrameDataMemberNames.NRepititionsNullForInfinite)]
        public int NRepititionsNullForInfinite { get { return _NRepititionsNullForInfinite; } protected set { _NRepititionsNullForInfinite = value; } }
        private DaysOfWeek _OnDaysOfWeek;
        [JsonPropertyName(AbstractTimeFrameDataMemberNames.OnDaysOfWeek)]
        [JsonInclude]
        [DataMember(Name = AbstractTimeFrameDataMemberNames.OnDaysOfWeek)]
        public DaysOfWeek OnDaysOfWeek { get { return _OnDaysOfWeek; } protected set { _OnDaysOfWeek = value; } }
        private int[] _OnDaysOfMonth;
        [JsonPropertyName(AbstractTimeFrameDataMemberNames.OnDaysOfMonth)]
        [JsonInclude]
        [DataMember(Name = AbstractTimeFrameDataMemberNames.OnDaysOfMonth)]
        public int[] OnDaysOfMonth { get { return _OnDaysOfMonth; } protected set { _OnDaysOfMonth = value; } }
        private bool _IsAlarm;
        [JsonPropertyName(AbstractTimeFrameDataMemberNames.IsAlarm)]
        [JsonInclude]
        [DataMember(Name =AbstractTimeFrameDataMemberNames.IsAlarm)]
        public bool IsAlarm { get { return _IsAlarm; }protected set { _IsAlarm = value; } }
        protected AbstractTimeFrame() { }
    }
}
