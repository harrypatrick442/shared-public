using Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
namespace Core.Threading
{
    public static class ParallelOperationHelper
    {
        public static ParallelOperationResult<TArgument, TReturn>
            [] RunInParallel<TArgument, TReturn>(IEnumerable<TArgument> arguments, 
            Func<TArgument, TReturn> callback, int maxNThreads)
        {
            List<ParallelOperationResult<TArgument, TReturn>> returns = new List<ParallelOperationResult<TArgument, TReturn>>();
            CountdownLatch countdownLatch = 
                new CountdownLatch(arguments.Count());
            foreach (TArgument argument in arguments)
            {
                //TODO maxNThreads
                new Thread(() =>
                {
                    try
                    {
                        TReturn ret = callback(argument);
                        lock (returns)
                        {
                            returns.Add(new ParallelOperationResult<TArgument,
                                TReturn>(argument, ret));
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (returns)
                        {
                            returns.Add(new ParallelOperationResult<TArgument,
                                TReturn>(ex, argument));
                        }
                    }
                    finally
                    {
                        countdownLatch.Signal();
                    }
                }).Start();
            }
            countdownLatch.Wait();
            return returns.ToArray();
        }

        public static ParallelOperationResult<TArgument>
            [] RunInParallel<TArgument>(TArgument[] arguments,
            Action<TArgument> callback, int maxNThreads)
        {
            List<ParallelOperationResult<TArgument>> returns = new List<ParallelOperationResult<TArgument>>();
            CountdownLatch countdownLatch =
                new CountdownLatch(arguments.Length);
            foreach (TArgument argument in arguments)
            {
                //TODO maxNThreads
                new Thread(() =>
                {
                    try
                    {
                        callback(argument);
                        lock (returns)
                        {
                            returns.Add(new ParallelOperationResult<TArgument>
                                (argument));
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (returns)
                        {
                            returns.Add(new ParallelOperationResult<TArgument>(ex, argument));
                        }
                    }
                    finally
                    {
                        countdownLatch.Signal();
                    }
                }).Start();
            }
            countdownLatch.Wait();
            return returns.ToArray();
        }

        public static void RunInParallelNoReturn<TArgument>(IEnumerable<TArgument> arguments,
            Action<TArgument> callback, int maxNThreads, bool throwExceptions=false)
        {
            CountdownLatch countdownLatch =
                new CountdownLatch(arguments.Count());
            List<Exception> exceptions = throwExceptions ? new List<Exception>() : null;
            foreach (TArgument argument in arguments)
            {
                //TODO maxNThreads
                new Thread(() =>
                {
                    try
                    {
                        callback(argument);
                    }
                    catch (Exception ex)
                    {
                        if (throwExceptions)
                        {
                            lock (exceptions)
                            {
                                exceptions.Add(ex);
                            }
                        }
                        else
                        {
                            Logs.Default.Error(new Exception("Should have been handled internally", ex));
                        }
                    }
                    finally
                    {
                        countdownLatch.Signal();
                    }
                }).Start();
            }
            countdownLatch.Wait();
            if (throwExceptions&&exceptions.Count>0)
                throw new AggregateException(exceptions);
        }
    }
}
