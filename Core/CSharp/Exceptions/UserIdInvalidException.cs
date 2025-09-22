using Core.Enums;
using System;
namespace Core.Exceptions
{
	public class UserIdInvalidException : Exception
	{
		private UserIdInvalidReason _Reason;
		public UserIdInvalidReason Reason { get { return _Reason; } }
		public UserIdInvalidException(UserIdInvalidReason reason) : base()
		{
			_Reason = reason;
		}
	}
}