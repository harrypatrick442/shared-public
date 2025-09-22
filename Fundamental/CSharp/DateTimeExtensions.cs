using System;
using System.Collections.Generic;
namespace Core.Timing
{
    public static class DateTimeExtensions
    {
        public static long ToMillisecondsUTC(this DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(
                    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    ).TotalMilliseconds;
        }
    }
}