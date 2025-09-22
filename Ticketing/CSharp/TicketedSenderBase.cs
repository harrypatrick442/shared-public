using Core.Interfaces;
using JSON;
using System;
using Core.Timing;
using System.Collections.Generic;
using Shutdown;
using System.Linq;
using Timer = System.Timers.Timer;
using System.Timers;

namespace Core.Ticketing
{
    public abstract class TicketedSenderBase: IDisposable
    {
        protected static bool _ShuttingDown = false;
        protected static Timer _TimerCheckTimedOutAllThreads;
        protected static HashSet<TicketedSenderBase> _Instances = new HashSet<TicketedSenderBase>();
        protected bool _Disposed = false;
        protected Dictionary<long, TicketedSender_Handle> _MapTicketToHandle = new Dictionary<long, TicketedSender_Handle>();
        protected HashSet<TicketedSender_Handle> _Handles = new HashSet<TicketedSender_Handle>();

        public virtual void Dispose()
        {
            lock (_MapTicketToHandle)
            {
                if (_Disposed) return;
                _Disposed = true;//Possible lockup condition due to lock while calling fail?
                foreach (TicketedSender_Handle handle in _Handles)
                {
                    handle.Fail(new ObjectDisposedException(nameof(TicketedSenderBase)));
                }
                lock (_Instances)
                {
                    _Instances.Remove(this);
                }
            }
        }
        public void CheckTimedOut()
        {
            TicketedSender_Handle[] handles = null;
            lock (_MapTicketToHandle)
            {
                handles = _Handles.ToArray();
            }
            if (handles.Length < 1) return;
            long millisecondsNow = TimeHelper.MillisecondsNow;
            foreach (TicketedSender_Handle handle in handles)
            {
                handle.CheckTimedOut(millisecondsNow);
            }
        }

    }
    public abstract class TicketedSenderBase<TMessageBaseBeingSent, TMessageBaseBeingReceived> : TicketedSenderBase where TMessageBaseBeingReceived : ITypedMessage
    {


        protected delegate long DelegateTicketOutgoingMessage<TMessage>(TMessage message)
            where TMessage : TMessageBaseBeingSent;
        static TicketedSenderBase()
        {
            _TimerCheckTimedOutAllThreads = new Timer(500);
            _TimerCheckTimedOutAllThreads.AutoReset = true;
            _TimerCheckTimedOutAllThreads.Enabled = true;
            _TimerCheckTimedOutAllThreads.Elapsed += CheckTimedOutOnAllInstances;
            _TimerCheckTimedOutAllThreads.Start();

            ShutdownManager.Instance.Add(DoShutdown, ShutdownOrder.TicketedSender);
        }
        private static void DoShutdown()
        {
            TicketedSenderBase[] instances;
            lock (_Instances)
            {
                _ShuttingDown = true;
                instances = _Instances.ToArray();
                _TimerCheckTimedOutAllThreads.Stop();
                _TimerCheckTimedOutAllThreads.Dispose();
            }
            foreach (TicketedSenderBase instance in instances)
            {
                instance.Dispose();
            }
        }
        private static void CheckTimedOutOnAllInstances(object sender, ElapsedEventArgs e)
        {

            TicketedSenderBase[] instances;
            lock (_Instances)
            {
                instances = _Instances.ToArray();
                if (instances.Length < 1)
                {
                    _TimerCheckTimedOutAllThreads.Stop();
                    return;
                }
            }
            foreach (TicketedSenderBase instance in instances)
            {
                instance.CheckTimedOut();
            }
        }
        protected TicketedSenderBase()
        {
            lock (_Instances)
            {
                if (_ShuttingDown) throw new ObjectDisposedException($"Shutting down {nameof(TicketedSenderBase)}");
                _Instances.Add(this);
                _TimerCheckTimedOutAllThreads.Start();
            }
        }
        protected abstract bool GetTicketForHandleMessage(TMessageBaseBeingReceived message, out long ticketed);
        /// <summary>
        /// returns true if message had ticket.
        /// This is useful for the outer message handler to know to do nothing else with the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual bool _HandleMessage(TMessageBaseBeingReceived message, string rawMessage)
        {
            if (!GetTicketForHandleMessage(message, out long ticket)) return false;
            lock (_MapTicketToHandle)
            {
                if (!_MapTicketToHandle.ContainsKey(ticket)) return true;
                TicketedSender_Handle handle = _MapTicketToHandle[ticket];
                handle.Complete(rawMessage);
                _Handles.Remove(handle);
                _MapTicketToHandle.Remove(handle.Ticket);
            }
            return true;
        }
        protected virtual TResponseMessage Send<TMessage, TResponseMessage>(TMessage message,
            int timeoutMilliseconds, System.Threading.CancellationToken? cancellationToken,
            DelegateTicketOutgoingMessage<TMessage> ticketOutgoingMessage, Action<string> send)
            where TMessage : TMessageBaseBeingSent where TResponseMessage : TMessageBaseBeingReceived
        {
            long ticket = ticketOutgoingMessage(message);
            TResponseMessage responseMessage = default;
            TicketedSender_Handle handle = new TicketedSender_Handle(ticket,
                TimeHelper.MillisecondsNow + timeoutMilliseconds);
            AddHandleAndCheckNotDisposed(handle);
            try
            {
                send(Json.Serialize(message));
                handle.Wait(cancellationToken);
                responseMessage = Json.Deserialize<TResponseMessage>(handle.ResponseMessage);
            }
            finally
            {
                RemoveHandle(handle);
            }
            return responseMessage;
        }
        private void AddHandleAndCheckNotDisposed(TicketedSender_Handle handle)
        {
            lock (_MapTicketToHandle)
            {
                if (_Disposed) throw new ObjectDisposedException(nameof(TicketedSenderBase));
                _Handles.Add(handle);
                _MapTicketToHandle.Add(handle.Ticket, handle);
            }
        }
        private void RemoveHandle(TicketedSender_Handle handle)
        {

            lock (_MapTicketToHandle)
            {
                if (!_MapTicketToHandle.Remove(handle.Ticket)) return;
                _Handles.Remove(handle);
            }
        }
    }
}
