using JSON;
namespace Native.Messaging
{
    public class RegisteredHandler<TPayload, TResponse> : RegisteredHandler
    {
        public override bool HasResponse => true;
        public Func<TPayload, TResponse> _Callback;
        public RegisteredHandler(Func<TPayload, TResponse> callback) : base()
        {
            _Callback = callback;
        }
        public override string Call(string payload)
        {
            TResponse response = _Callback(Json.Deserialize<TPayload>(payload));
            return response == null ? null : Json.Serialize(response);
        }
    }
    public class RegisteredHandler<TMessage> : RegisteredHandler
    {
        public override bool HasResponse => false;
        public Action<TMessage> _Callback;
        public RegisteredHandler(Action<TMessage> callback) : base()
        {
            _Callback = callback;
        }
        public override string Call(string payload)
        {
            _Callback(Json.Deserialize<TMessage>(payload));
            return null;
        }
    }
    public abstract class RegisteredHandler
    {
        public abstract bool HasResponse { get; }
        public RegisteredHandler()
        {

        }
        public abstract string Call(string payload);
    }
}