using System.Collections.Generic;
using System;
using System.Linq;

namespace Core.Pool
{
	public class BinaryProgressHandler : ProgressHandler
    {
        private bool _CurrentValue;
        public void Set(bool value)
        {
            EventHandler<ProgressEventArgs>[]? eventHandlers;
            double proportion;
            lock (_LockObject)
            {
                if (_CurrentValue == value) return;
                _CurrentValue = value;
                proportion = value ? 1d : 0d;
                _Proportion = proportion;
                eventHandlers = _ProgressedEventHandlers?.ToArray();
            }
            Dispatch(eventHandlers, proportion);
        }
        public BinaryProgressHandler() { 
            
        }
    }
}
