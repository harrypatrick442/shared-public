using JSON;
using Native.Messages;
namespace Native.Messaging
{
    public class PingDisconnectDetector
    {
        private System.Timers.Timer _TimerPing;
        private RegistrationMessageHandler _RegistrationMessageHandler;
        private readonly string PING_MESSAGE_STRING;
        private readonly object _LockObject = new object();
        private bool _SeenPing = true;
        private Action _Disconnected;
        public PingDisconnectDetector(
            RegistrationMessageHandler registrationMessageHandler,
            Action disconnected,
            int intervalMilliseconds = 5000) {
            _Disconnected = disconnected;
            PING_MESSAGE_STRING = Json.Serialize(new PingMessage());
            _TimerPing = new System.Timers.Timer(interval: intervalMilliseconds);
            _TimerPing.AutoReset = true;
            _TimerPing.Elapsed += Ping;
            _RegistrationMessageHandler= registrationMessageHandler;
            _RegistrationMessageHandler.RegisterMethod<PingMessage>(MessageTypes.Ping, HandleIncomingPing);
        }
        private void HandleIncomingPing(PingMessage message)
        {
            lock (_LockObject)
            {
                _SeenPing = true;
            }
        }
        public void Received()
        {
            lock (_LockObject)
            {
                _SeenPing = true;
            }
        }
        public void Stop()
        {
            lock (_LockObject)
            {
                _TimerPing.Stop();
            }
        }
        public void StartClean() {
            lock (_LockObject)
            {
                _SeenPing = true;
                _TimerPing.Start();
            }
        }
        private void Ping(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_LockObject)
            {
                if (!_SeenPing)
                {
                    Stop();
                    _Disconnected();
                    return;
                }
                _SeenPing = false;
            }
            try { _RegistrationMessageHandler.SendRaw(PING_MESSAGE_STRING); }
            catch
            {

            }
        }
    }
}