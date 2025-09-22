using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Core.Pool
{
	public class ProgressEventArgs : EventArgs
    {
        public double Proportion { get; }
        public ProgressEventArgs(double proportion)
        {
            Proportion = proportion;
        }

    }
}
