using Core.Enums;
using System;
namespace Core.Exceptions{
	public class UsernameInvalidException : Exception
	{
		private UsernameInvalidReason _Reason;
		public UsernameInvalidReason Reason { get { return _Reason; } }
		private int? _MinLength;
		private int? _MaxLength;
		public int? MinLength { get { return _MinLength; } }
		public int? MaxLength { get { return _MaxLength; } }
		public UsernameInvalidException(UsernameInvalidReason reason) {
			_Reason = reason;
		}
		public UsernameInvalidException(UsernameInvalidReason reason, int? minLength, int? maxLength) : base() {
			_Reason = reason;
			_MinLength = minLength;
			_MaxLength = maxLength;
		}
	}
}