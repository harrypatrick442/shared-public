using Core.Enums;
using System;
namespace Core.Exceptions{
	public class EmailInvalidException : Exception
	{
		private EmailInvalidReason _Reason;
		public EmailInvalidReason Reason { get { return _Reason;  } }
		public EmailInvalidException(EmailInvalidReason reason) : base() {
			_Reason = reason;
		}
	}
}