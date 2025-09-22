using System;
using System.Collections.Generic;
using System.Threading;
namespace Core
{
    public class Once
    {
        private List<int> _RunningThreadIds = new List<int>();
        private object _LockObjectAddThreadId = new List<int>();
        public void Run(Action callback) {
            int threadId = Thread.CurrentThread.GetHashCode();
            lock (_LockObjectAddThreadId) {
                if (_RunningThreadIds.Contains(threadId)) return;
                _RunningThreadIds.Add(threadId);
            }
            try
            {
                callback();
            }
            finally
            {
                _RunningThreadIds.Remove(threadId);
            }
        }
    }
}
