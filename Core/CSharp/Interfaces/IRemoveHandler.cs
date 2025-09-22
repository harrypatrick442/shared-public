namespace Core.Port
{
    public interface IRemoveHandler<TMessageToReceive>{
        void RemoveHandler(string messageType, CallbackMessageDelegate<TMessageToReceive> callbackMessageDelegate);
    }
}
