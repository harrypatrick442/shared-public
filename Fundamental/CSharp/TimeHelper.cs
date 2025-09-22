using System;
using System.Collections.Generic;
namespace Core.Timing
{
    public static class TimeHelper
    {
        public static long MillisecondsNow
        {
            get
            {
                return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }
        public static string NowMillisecondsQueryStringProperty
        {
            get
            {
                return $"t={(MillisecondsNow).ToString()}";
            }
        }
        public static string NowMillisecondsQueryStringPropertyWithQuestionMark
        {
            get
            {
                return $"?{NowMillisecondsQueryStringProperty}";
            }
        }
        public static DateTime GetDateTimeFromMillisecondsUTC(long millisecondsUTC) {
            TimeSpan time = TimeSpan.FromMilliseconds(millisecondsUTC);
            return new DateTime(1970, 1, 1) + time;
        }
    }
}