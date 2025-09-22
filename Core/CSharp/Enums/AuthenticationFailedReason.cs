using Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;

namespace Core.Enums {
    public enum AuthenticationFailedReason
    {
        Unknown=1,
        BadCredentials=2,
        TooManyAttempts=3,
        Busy=4,
        UserId = 5,
        Phone = 6,
        Email = 7,
        Password = 8,
        Username=9
    }
    public static class AuthenticationFailedReasonExtensions {

        public static string GetDescriptuon(this AuthenticationFailedReason? value)
        {
            if (value != null)
                switch ((AuthenticationFailedReason)value)
                {
                    case AuthenticationFailedReason.BadCredentials:
                        return "bad credentials";
                    case AuthenticationFailedReason.TooManyAttempts:
                        return "too many attempts";
                    case AuthenticationFailedReason.Busy:
                        return "busy";
                    case AuthenticationFailedReason.UserId:
                        return "user id";
                    case AuthenticationFailedReason.Phone:
                        return "phone";
                    case AuthenticationFailedReason.Email:
                        return "email";
                    case AuthenticationFailedReason.Password:
                        return "password";
                    case AuthenticationFailedReason.Unknown:
                    default:
                        break;
                }
            return "unknown";
        }
    }
}

