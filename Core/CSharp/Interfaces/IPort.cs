
using Core.Port;

namespace Core.Interfaces{
    public interface IPort<TMessageToSend, TMessageToReceive> :ISender<TMessageToSend>, IAddRemoveHandler<TMessageToReceive>
    {

    }
}