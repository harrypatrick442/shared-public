namespace Core.Port
{
    public delegate void CallbackMessageDelegate<TMessageReceived>(TMessageReceived messageReceived, IChannel IChannel);
}
