namespace Core.Port
{
    public interface IAddHandler<TMessageToReceive>
    {
        void AddHandler(string messageType, CallbackMessageDelegate<TMessageToReceive> callbackMessageDelegate);
    }
}
