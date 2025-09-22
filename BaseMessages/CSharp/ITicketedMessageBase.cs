using Core.Interfaces;

namespace Core.Messages.Messages
{
    public interface ITicketedMessageBase: ITypedMessage
    {
        long Ticket { get; set; }
    }
}