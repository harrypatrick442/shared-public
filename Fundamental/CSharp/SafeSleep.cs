using System;
using System.Threading;
using ConfigurationCore;
using Core.Timing;
using DependencyManagement;
namespace Core.Threading {
    public static class SafeSleep
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="intervalNTimes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Func<Done></Done></returns>
        public static Func<bool>GetSleep(CancellationToken cancellationToken, params IntervalNTimes[] intervalNTimes)
        {
            int nTimes=0;
            int intervalIndex = 0;
            Func<IntervalNTimes> nextInterval = () =>
            {
                if (intervalIndex >= intervalNTimes.Length)
                    return null;
                return intervalNTimes[intervalIndex++];
            };
            IntervalNTimes currentIntervalNTimes = nextInterval();
            Action sleep = GetSleep(currentIntervalNTimes.IntervalMilliseconds, cancellationToken);
            return () => {
                sleep();
                if (cancellationToken.IsCancellationRequested)
                    return true;
                nTimes++;
                if (currentIntervalNTimes.NTimes < 0)
                    return false;
                if (nTimes < currentIntervalNTimes.NTimes)
                    return false;
                currentIntervalNTimes = nextInterval();
                if (currentIntervalNTimes == null)
                {
                    sleep = () => throw new InvalidOperationException("Is already done");
                    return true;
                }
                nTimes = 0;
                sleep = GetSleep(currentIntervalNTimes.IntervalMilliseconds, cancellationToken);
                return false;
            };
        }
        public static Action GetSleep(int delayMilliseconds, CancellationToken cancellationToken)
        {
            if (delayMilliseconds <= 0) 
                throw new ArgumentException(nameof(delayMilliseconds));
            int nIntervals = (int)Math.Ceiling((double)delayMilliseconds / DependencyManager.Get<IIntervalsConfiguration>().SleepBetweenDisposedCheckIntervals);
            int intervalDelay = delayMilliseconds / nIntervals;
            return () => {
                for (int i = 0; i < nIntervals; i++) {
                    Thread.Sleep(intervalDelay);
                    if (cancellationToken.IsCancellationRequested)
                        return;
                }
                return;
            };
        }
    }
}
