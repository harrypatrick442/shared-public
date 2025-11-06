using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationCore
{
    public interface IIntervalsConfiguration
    {
        public int SleepBetweenDisposedCheckIntervals { get; }
    }
}
