using System;
using System.Timers;
using Core.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Port;
using Core;

namespace Core.Ticketing
{
    /// <summary>
    /// Speaks JSON. Pleaes use JSON it's nicer than parameters in a url
    /// </summary>
    public class TicketedSenderOld<TMessageToSend, TMessageToReceive> : IDisposable where TMessageToSend : ITicketed where TMessageToReceive : ITicketed
    {

        private const string TICKETED = "ticketed";
        private const int DEFAULT_TIMEOUT_MILLISECONDS = 10000;
        private static int _NTicket = 0;
        private delegate void DelegateMessageCallback(TMessageToReceive messageReceived);
        private object _MapTicketToSenderCallbackLockObject = new object();

        private Dictionary<long, SendHandle> _MapTicketToSenderCallback = new Dictionary<long, SendHandle>();
        private IPort<TMessageToSend, TMessageToReceive> _IPort;
        private bool _Disposed = false;
        public TicketedSenderOld(IPort<TMessageToSend, TMessageToReceive> iPort)
        {
            _IPort = iPort;
            _IPort.AddHandler(TICKETED, HandleMessage);
        }
        private void HandleMessage(TMessageToReceive messageReceived, IChannel iChannel)
        {
            long ticket = messageReceived.Ticket;
            DelegateMessageCallback delegateMessageCallback;
            SendHandle sendHandle;
            lock (_MapTicketToSenderCallbackLockObject)
            {
                if (!_MapTicketToSenderCallback.ContainsKey(ticket)) return;
                sendHandle = _MapTicketToSenderCallback[ticket];
            }
            if (sendHandle.Cancelled) return;
            sendHandle.DelegateMessageCallback(messageReceived);
        }
        public TMessageToReceive Send(TMessageToSend message, int timeoutMilliseconds = DEFAULT_TIMEOUT_MILLISECONDS, CancellationToken? cancellationToken = null)
        {
            CheckNotDisposed();
            CancellationTokenRegistration? cancellationTokenRegistration = null;
            CountdownLatch countdownLatch = new CountdownLatch();
            TMessageToReceive messageReceived = default;
            if (cancellationToken != null)
            {
                cancellationTokenRegistration = ((CancellationToken)cancellationToken).Register(countdownLatch.Signal);
            }
            try
            {
                SendHandle sendHandle = new SendHandle((messageReceivedIn) =>
                {
                    messageReceived = messageReceivedIn;
                    countdownLatch.Signal();
                }, countdownLatch);
                lock (_MapTicketToSenderCallbackLockObject)
                {
                    CheckNotDisposed();
                    _MapTicketToSenderCallback[message.Ticket] = sendHandle;
                }
                _IPort.Send(message);
                bool timedOutIfNotCancelled = !countdownLatch.Wait(timeoutMilliseconds);
                if (sendHandle.Cancelled || cancellationToken != null && ((CancellationToken)cancellationToken).IsCancellationRequested)
                    throw new TaskCanceledException("Ticketed send was cancelled");
                if (timedOutIfNotCancelled)
                    throw new TimeoutException("Ticketed send timed out waiting for response");
                return messageReceived;
            }
            finally
            {
                lock (_MapTicketToSenderCallbackLockObject)
                {
                    _MapTicketToSenderCallback.Remove(message.Ticket);
                }
                if (cancellationTokenRegistration != null)
                    ((CancellationTokenRegistration)cancellationTokenRegistration).Dispose();
            }
        }
        private void CheckNotDisposed()
        {
            if (_Disposed) throw new ObjectDisposedException(nameof(TicketedSenderOld<TMessageToSend, TMessageToReceive>));
        }
        public void Dispose()
        {
            if (_Disposed) return;
            _Disposed = true;
            _IPort.RemoveHandler(TICKETED, HandleMessage);
            lock (_MapTicketToSenderCallbackLockObject)
            {
                foreach (SendHandle sendHandle in _MapTicketToSenderCallback.Values)
                    sendHandle.Cancel();
            }
        }
        private class SendHandle
        {
            private DelegateMessageCallback _DelegateMessageCallback;
            public DelegateMessageCallback DelegateMessageCallback { get { return _DelegateMessageCallback; } }
            private CountdownLatch _CountdownLatch;
            private bool _Cancelled = false;
            public bool Cancelled { get { return _Cancelled; } }
            public SendHandle(DelegateMessageCallback delegateMessageCallback, CountdownLatch countdownLatch)
            {
                _DelegateMessageCallback = delegateMessageCallback;
                _CountdownLatch = countdownLatch;
            }
            public void Cancel() { _Cancelled = true; _CountdownLatch.Signal(); }
        }
    }
}
