using System;
namespace Core.Interfaces
{
    public interface IInverseTicketed:ITypedMessage{
        long InverseTicket { get; set; }
    }
}
