using System.Threading;
using System;
using Core;

namespace Core.Ticketing
{

    public class TicketedSender_Handle
    {
        private readonly object _LockObjectModify = new object();
        private CountdownLatch _CountdownLatchWait = new CountdownLatch();
        private long _Ticket;
        public long Ticket { get { return _Ticket; } }
        private long _TimeoutAtThresholdMilliseconds;
        private Exception _Exception;
        private string _ResponseMessage;
        public string ResponseMessage { get { return _ResponseMessage; } }
        public void Complete(string responseMessage)
        {
            lock (_LockObjectModify)
            {
                _ResponseMessage = responseMessage;
            }
            _CountdownLatchWait.Signal();
        }
        public void Fail(Exception exception)
        {
            lock (_LockObjectModify)
            {
                _Exception = exception;
            }
            _CountdownLatchWait.Signal();
        }
        public string Wait(CancellationToken? cancellationToken)
        {
            if (cancellationToken == null)
                _CountdownLatchWait.Wait();
            else
                _CountdownLatchWait.Wait((CancellationToken)cancellationToken);
            try
            {
                if (cancellationToken != null
                    && ((CancellationToken)cancellationToken).IsCancellationRequested)
                    throw new OperationCanceledException();
                lock (_LockObjectModify)
                {
                    if (_Exception != null)
                        throw _Exception;
                    return _ResponseMessage;
                }
            }
            finally
            {

            }
        }
        public bool CheckTimedOut(long millisecondsNow)
        {//TODO remove timers from every handle. shared timers.
            if (millisecondsNow < _TimeoutAtThresholdMilliseconds) return false;
            Fail(new TimeoutException());
            return true;
        }
        public TicketedSender_Handle(long ticket, long timeoutAtThresholdMilliseconds)
        {
            _Ticket = ticket;
            _TimeoutAtThresholdMilliseconds = timeoutAtThresholdMilliseconds;
        }
    }
}
