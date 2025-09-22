using System;
using System.Collections.Generic;
using System.Threading;
namespace Core
{
    public class Result<TResult> {
        public Exception Exception { get; }
        public bool HadException { get; }
        public TResult Value { get; }
        public Result(TResult value) {
            HadException = false;
            Exception = null;
            Value = value;
        }

        public Result(Exception exception)
        {
            HadException = true;
            Exception = exception;
            Value = default(TResult);
        }
    }
}
