using System;
using System.Collections.Generic;
namespace Core.Timing
{
    public class IntervalNTimes
    {
        public int NTimes { get; }
        public int IntervalMilliseconds { get; }
        public IntervalNTimes(int intervalMilliseconds, int nTimes)
        {
            IntervalMilliseconds = intervalMilliseconds;
            NTimes = nTimes;
        }
        public static IntervalNTimes Infinite(int nTimes, int intervalMilliseconds) {
            return new IntervalNTimes(nTimes, intervalMilliseconds);
        }
    }
}