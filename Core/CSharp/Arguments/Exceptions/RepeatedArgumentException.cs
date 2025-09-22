using System;
using Core.NativeExtensions;

namespace Core.Arguments
{
    public class RepeatedArgumentException :ArgumentException{
        private Arg _Arg1, _Arg2;
        public Arg Arg1 { get { return _Arg1; } }
        public  Arg Arg2{get{return _Arg2;} }
        public RepeatedArgumentException(Arg arg1, Arg arg2) :base(GetMessage(arg1, arg2)){
            _Arg1 = arg1;
            _Arg2 = arg2;
        }
        private static string GetMessage(Arg arg1, Arg arg2) {
            string rawKey1 = arg1.RawKey;
            string rawKey2 = arg2.RawKey;
            bool hadDifferentPoll = rawKey1 != rawKey2;
            string message = "Repeated flag ";
            if (hadDifferentPoll)
                message += $"with different poll \"{rawKey1}\" and \"{rawKey2}\"";
            else message += $"\"{rawKey1}\"";
            return message;
        }
    }
}