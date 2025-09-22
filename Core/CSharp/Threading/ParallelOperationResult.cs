using Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
namespace Core.Threading
{
    public class ParallelOperationResult<TArgument, TReturn>: ParallelOperationResult<TArgument>
    {
        private TReturn _Return;
        public TReturn Return { get { return _Return; } }
        public ParallelOperationResult(TArgument argument, TReturn ret):base(argument)
        {
            _Return = ret;
        }
        public ParallelOperationResult(Exception exception, TArgument argument):base(exception, argument)
        {

        }
    }
    public class ParallelOperationResult<TArgument>
    {
        private TArgument _Argument;
        public TArgument Argument { get { return _Argument; } }
        private Exception _Exception;
        public Exception Exception { get { return _Exception; } }
        private bool _Success;
        public bool Success
        {
            get { return _Success; }
        }
        public ParallelOperationResult(TArgument argument)
        {
            _Argument = argument;
            _Success = true;
        }
        public ParallelOperationResult(Exception exception, TArgument argument)
        {
            _Exception = exception;
            _Argument = argument;
        }
    }
}
