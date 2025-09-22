using Core.Enums;
using System;
namespace Core.Exceptions{
	public class PasswordInvalidException : Exception
	{
		private PasswordInvalidReason _Reason;
		public PasswordInvalidReason Reason { get { return _Reason; } }
		private int? _MinLength;
		private int? _MaxLength;
		public int? MinLength { get { return _MinLength; } }
		public int? MaxLength { get { return _MaxLength; } }
		public PasswordInvalidException(PasswordInvalidReason reason, int? minLength, int? maxLength) : base() {
			_Reason = reason;
			_MinLength = minLength;
			_MaxLength = maxLength;
		}
	}
}