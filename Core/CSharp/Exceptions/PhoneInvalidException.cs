using Core.Enums;
using System;
namespace Core.Exceptions
{
	public class PhoneInvalidException : Exception
	{
		private PhoneInvalidReason _Reason;
		public PhoneInvalidReason Reason { get { return _Reason; } }
		public PhoneInvalidException(PhoneInvalidReason reason) : base()
		{
			_Reason = reason;
		}
	}
}