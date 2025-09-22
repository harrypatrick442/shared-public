using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Maths
{
	public class Average
	{

        private long _MaxNEntries;
        private List<long> _CurrentEntries = new List<long>();
        public void AddValue(long value) {
            lock (_CurrentEntries)
            {
                if (_CurrentEntries.Count >= _MaxNEntries)
                {
                    _CurrentEntries.RemoveAt(0);
                }
                _CurrentEntries.Add(value);
            }
        }
        public long Value
        {
            get
            {
                lock (_CurrentEntries)
                {
                    return _CurrentEntries.Count==0?0:_CurrentEntries.Sum(entry => entry) / _CurrentEntries.Count;
                }
            }
        }

        public Average(int maxNEntries)
        {
            _MaxNEntries = maxNEntries;
        }
    }
}
