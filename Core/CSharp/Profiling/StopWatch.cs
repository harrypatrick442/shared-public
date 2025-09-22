using Core.Timing;

namespace Core.Queueing
{
    public class StopWatch
    {
        private static readonly object _InstanceLockObject = new object();
        private static StopWatch? _DebugInstance;
        public static StopWatch DebugInstance
        {
            get
            {
                lock (_InstanceLockObject)
                {
                    if (_DebugInstance == null)
                        _DebugInstance = new StopWatch();
                    return _DebugInstance;
                }
            }
        }
        private static StopWatch? _DebugInstance2;
        public static StopWatch DebugInstance2
        {
            get
            {
                lock (_InstanceLockObject)
                {
                    if (_DebugInstance2 == null)
                        _DebugInstance2 = new StopWatch();
                    return _DebugInstance2;
                }
            }
        }
        private static StopWatch? _DebugInstance4;
        public static StopWatch DebugInstance4
        {
            get
            {
                lock (_InstanceLockObject)
                {
                    if (_DebugInstance4 == null)
                        _DebugInstance4 = new StopWatch();
                    return _DebugInstance4;
                }
            }
        }
        private static StopWatch? _DebugInstance3;
        public static StopWatch DebugInstance3
        {
            get
            {
                lock (_InstanceLockObject)
                {
                    if (_DebugInstance3 == null)
                        _DebugInstance3 = new StopWatch();
                    return _DebugInstance3;
                }
            }
        }
        private long _CreatedAt;
        private long _StartedAt;
        private bool _Running = false;
        public long TotalRunTime { get; protected set; }
        public double ProportionTimeRun => (double)TotalRunTime / (double)(_StartedAt - _CreatedAt);
        public StopWatch() {
            _CreatedAt = TimeHelper.MillisecondsNow;
        }
        public void Start()
        {
            _StartedAt = TimeHelper.MillisecondsNow;
            _Running = true;
        }
        public void Stop() {
            if (!_Running)
                return;
            _Running = false;
            long now = TimeHelper.MillisecondsNow;
            long delay = now- _StartedAt;
            TotalRunTime +=delay;
            _StartedAt = now;
        }
    }
}
