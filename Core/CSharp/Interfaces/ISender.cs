
namespace Core.Interfaces{
    public interface ISender<TMessage>
    {
        void Send(TMessage message);
    }
}