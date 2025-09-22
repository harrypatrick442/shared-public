using System.Collections.Generic;
using System;
using System.Linq;

namespace Core.Pool
{
	public class StandardProgressHandler : ProgressHandler
    {
        public void Set(double value) {
            if (value < 0)
                value = 0;
            else
            {
                if (value > 1)
                    value = 1;
            }
            EventHandler<ProgressEventArgs>[]? eventHandlers;
            lock (_LockObject)
            {
                if (_Proportion == value) return;
                _Proportion = value;
                eventHandlers = _ProgressedEventHandlers?.ToArray();
            }
            Dispatch(eventHandlers, value);
        }
        public StandardProgressHandler(double startProportion = 0) {
            _Proportion = startProportion;
        }

        public Action GetUpdateProgress(int total, int nIntervals)
        {

            int nThisProgressUpdate = 0;
            int nPerSet = total / nIntervals;
            int nDone = 0;
            return () =>
            {
                nDone++;
                if (++nThisProgressUpdate >= nPerSet)
                {
                    nThisProgressUpdate = 0;
                    Set((double)nDone / (double)total);
                }
            };
        }
    }
}
