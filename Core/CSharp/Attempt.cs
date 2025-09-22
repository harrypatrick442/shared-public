using System;
using System.Linq;
using Core.Exceptions;
namespace Core
{
    public static class Attempt
    {
        public static void UpToNTimes(Action toRun, int nTimes)
        {
            Exception lastException = null;
            int i = 0;
            while (i < nTimes)
            {
                try
                {
                    toRun();
                    return;
                }
                catch (Exception exception)
                {
                    lastException = exception;
                }
                i++;
            }
            throw new OperationFailedException($"Attempted {nTimes} but failed", lastException);
        }
        public static TResult UpToNTimes<TResult>(Func<TResult> toRun, int nTimes)
        {
            Exception lastException = null;
            int i = 0;
            while (i < nTimes)
            {
                try
                {
                    return toRun();
                }
                catch (Exception exception)
                {
                    lastException = exception;
                }
                i++;
            }
            throw new OperationFailedException($"Attempted {nTimes} but failed", lastException);
        }
    }
}