using Core.DataMemberNames;
using Core.Events;
using Core.Interfaces;
using Core.Messages.Messages;
using Core.Ticketing;
using JSON;

namespace Native.Messaging
{
    public abstract class RegistrationMessageHandlerBase
    {
        private const int DEFAULT_TICKETED_SENDER_TIMEOUT = 2000;
        public event EventHandler<TypedMessageEventArgs>? OnMessage = null;
        private Dictionary<string, RegisteredHandler> _MapTypeToRegisteredHandler = new Dictionary<string, RegisteredHandler>();
        private TicketedSender _TicketedSender;
        public RegistrationMessageHandlerBase()
        {
            _TicketedSender = new TicketedSender(SendRaw);
        }
        public void RegisterMethod<TRequest, TResponse>(string type, Func<TRequest, TResponse> callback)
            where TRequest : TicketedMessageBase where TResponse : TicketedMessageBase
        {
            lock (_MapTypeToRegisteredHandler)
            {
                _MapTypeToRegisteredHandler.Add(type, new RegisteredHandler<TRequest, TResponse>(callback));
            }
        }
        public void RegisterMethod<TMessage>(string type, Action<TMessage> callback)
        {
            lock (_MapTypeToRegisteredHandler)
            {
                _MapTypeToRegisteredHandler.Add(type, new RegisteredHandler<TMessage>(callback));
            }
        }
        public void Send<TRequest>(TRequest request) where TRequest : ITypedMessage
        {
            SendRaw(Json.Serialize(request));
        }
        public TResponse SendMessageWaitForResponse<TRequest, TResponse>(TRequest request,
            int timeoutMilliseconds = DEFAULT_TICKETED_SENDER_TIMEOUT,
            CancellationToken? cancellationToken = null)
            where TRequest : TicketedMessageBase
            where TResponse : TicketedMessageBase
        {
            return _TicketedSender.Send<TRequest, TResponse>(request,
                timeoutMilliseconds, cancellationToken);
        }
        protected void HandleIncomingMessage(string rawMessage)
        {

            TypedTicketedMessage ticketedMessageBase = Json.Instance.Deserialize<TypedTicketedMessage>(rawMessage);
            if (ticketedMessageBase.Type == MessageTypeDataMemberName.Value)
            {
                _TicketedSender.HandleMessage(ticketedMessageBase, rawMessage);
                return;
            }
            else
            {
                lock (_MapTypeToRegisteredHandler)
                {
                    if (_MapTypeToRegisteredHandler.TryGetValue(ticketedMessageBase.Type,
                        out RegisteredHandler registeredHandler))
                    {
                        string response = registeredHandler.Call(rawMessage);
                        if (registeredHandler.HasResponse)
                        {
                            SendRaw(response);
                        }
                    }
                }
            }
            EventHandler<TypedMessageEventArgs> onMessage = OnMessage;
            if (onMessage != null)
                onMessage.Invoke(this, new TypedMessageEventArgs(ticketedMessageBase.Type, rawMessage));
        }
        public abstract void SendRaw(string message);
        public void Dispose() {
            _TicketedSender.Dispose();
        }
    }
}