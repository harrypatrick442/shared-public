using Core.Interfaces;
using System.Threading;
using System;
using Core.Exceptions;
using Core.Messages.Messages;

namespace Core.Ticketing
{
    public class InverseTicketedSender : TicketedSenderBase<IInverseTicketed, IInverseTicketed>
    {
        public delegate bool DelegateHandleParsedAndRawMessage<TMessageToHandle>(TMessageToHandle message);
        public InverseTicketedSender()
            : base()
        {

        }
        public  TResponseMessage Send<TMessage, TResponseMessage>(TMessage message,
           long ticketForResponse, int timeoutMilliseconds, CancellationToken? cancellationToken,
           Action<string> send)
            where TMessage : TicketedMessageBase, IInverseTicketed where TResponseMessage : IInverseTicketed
        {
            return base.Send<TMessage, TResponseMessage>(message,
            timeoutMilliseconds, cancellationToken, Get_TicketOutgoingMessage<TMessage>(ticketForResponse), send);
        }
        public bool HandleMessage<TMessage>(TMessage message, string rawMessage) where TMessage:IInverseTicketed, ITypedMessage {

            if (message.Type == InverseTicketedSender_MessageTypes.DropInverseTicket) {
                DropInverseTicket(message.InverseTicket);
                return true;
            }
            return base._HandleMessage(message, rawMessage);
        }
        private void DropInverseTicket(long inverseTicket)
        {
            //TODO check this over
            TicketedSender_Handle handle;
            lock (_MapTicketToHandle) {
                if (!_MapTicketToHandle.TryGetValue(inverseTicket, out handle)) return;
                _MapTicketToHandle.Remove(inverseTicket);
                _Handles.Remove(handle);
            }
            handle.Fail(new OperationFailedException("Invert ticket was dropped"));
        }
        protected override bool GetTicketForHandleMessage(IInverseTicketed message, out long ticket)
        {
            ticket = -1;
            if (message.Type != InverseTicketedSender_MessageTypes.InverseTicketed) {
                return false;
            }
            ticket = message.InverseTicket;
            return true;
        }

        protected DelegateTicketOutgoingMessage<TMessage> Get_TicketOutgoingMessage<TMessage>(long ticketForResponse)
           where TMessage : TicketedMessageBase, IInverseTicketed 
        {
            return (message) =>
            {
                long ticket = TicketSource.GetNextTicket();
                message.InverseTicket = ticket;
                message.Ticket = ticketForResponse;
                return ticket;
            };
        }
    }
}
