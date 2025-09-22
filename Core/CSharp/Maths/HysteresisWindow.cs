using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Maths
{
	public struct HysteresisWindowLong
	{

		private long _LowerBound, _UpperBound;
		public long UpperBound { get { return _UpperBound; } }
		public long LowerBound { get { return _LowerBound; } }
		public long Size { get { return _UpperBound - _LowerBound; } }
		public HysteresisWindowLong(long lowerBound, long upperBound) {
			_LowerBound = lowerBound;
			_UpperBound = upperBound;
		}
    }
}
