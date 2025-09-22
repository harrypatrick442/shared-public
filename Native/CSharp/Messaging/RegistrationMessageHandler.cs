namespace Native.Messaging
{
    public class RegistrationMessageHandler: RegistrationMessageHandlerBase
    {

        private readonly object _LockObjectSendRaw = new object();
        private Action<string>? _SendRaw;
        public RegistrationMessageHandler(Action<string>? sendRaw) :base(){ 
            _SendRaw = sendRaw;
        }
        public new void HandleIncomingMessage(string message) { 
            base.HandleIncomingMessage(message);
        }
        public void SetSendRaw(Action<string> setSendRaw) {
            lock (_LockObjectSendRaw) {
                _SendRaw = setSendRaw;
            }
        }
        public override void SendRaw(string message)
        {
            lock (_LockObjectSendRaw)
            {
                _SendRaw?.Invoke(message);
            }
        }
    }
}