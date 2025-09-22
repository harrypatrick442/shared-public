

using Core.Timing;
using Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;
namespace Core.Locks
{
    public static class LockWatcher
    {
        //TODO ultimately remove this when confortable no issues.
        private static Timer _TimerCheck;
        static LockWatcher() {
            _TimerCheck = new Timer(Constants.Intervals.CHECK_WATCHED_LOCKS);
            _TimerCheck.AutoReset = true;
            _TimerCheck.Enabled = true;
            _TimerCheck.Elapsed += Check;
            _TimerCheck.Start();
        }
        private static HashSet<IWatchableLock> _WatchedLocks = new HashSet<IWatchableLock>();
        public static void Watch(IWatchableLock watchableLock)
        {
            lock (_WatchedLocks)
            {
                _WatchedLocks.Add(watchableLock);
            }
        }
        public static void StopWatching(IWatchableLock watchableLock)
        {
            lock (_WatchedLocks)
            {
                _WatchedLocks.Remove(watchableLock);
            }
        }
        private static void Check(object sender, ElapsedEventArgs e) {
            IWatchableLock[] watchableLocks;
            lock (_WatchedLocks) {
                watchableLocks = _WatchedLocks.ToArray();
            }
            long now = TimeHelper.MillisecondsNow;
            foreach (IWatchableLock watchedLock in watchableLocks) {
                if (now - watchedLock.CreatedAt > Constants.Intervals.MAXIMUM_INTERVAL_BEFORE_FLAG_WATCHED_LOCK) {
                    Logs.HighPriority.Error(new Exception("Watched lock was alive too long! " + watchedLock.StackTrace));
                }
            }
        }
    }
}
//TODO remove this from being initialized now probably not needed.