using System.Threading;
using System;
using JSON;
using Core.DataMemberNames;
using Core.Messages.Messages;

namespace Core.Ticketing
{
    public class TicketedSender : TicketedSenderBase<ITicketedMessageBase, ITicketedMessageBase>
    {
        private Action<string> _CallbackSend;
        public TicketedSender()
            : base()
        {

        }
        public TicketedSender(Action<string> callbackSend)
            : base()
        {
            _CallbackSend = callbackSend;
        }
        public  TResponseMessage Send<TMessage, TResponseMessage>(TMessage message,
            int timeoutMilliseconds, CancellationToken? cancellationToken, Action<string> send)
            where TMessage : ITicketedMessageBase where TResponseMessage : ITicketedMessageBase
        {
            return base.Send<TMessage, TResponseMessage>(message,
            timeoutMilliseconds, cancellationToken, TicketOutgoingMessage, send);
        }
        public bool HandleMessage(TicketedMessageBase message, string rawMessage) {
            return base._HandleMessage(message, rawMessage);
        }
        public TResponseMessage Send<TMessage, TResponseMessage>(
            TMessage message, int timeoutMilliseconds, CancellationToken? cancellationToken)
            where TMessage : TicketedMessageBase where TResponseMessage : TicketedMessageBase
        {
            return base.Send<TMessage, TResponseMessage>(message,
            timeoutMilliseconds, cancellationToken, TicketOutgoingMessage, _CallbackSend);
        }
        protected override bool GetTicketForHandleMessage(ITicketedMessageBase message, out long ticket)
        {
            ticket = -1;
            if (message.Type != TicketedMessageType.Ticketed) {
                return false;
            }
            ticket =  message.Ticket;
            return true;
        }
        protected long TicketOutgoingMessage<TMessage>(TMessage message) where TMessage : ITicketedMessageBase
        {
            long ticket = TicketSource.GetNextTicket();
            message.Ticket = ticket;
            return ticket;

        }
    }
}
