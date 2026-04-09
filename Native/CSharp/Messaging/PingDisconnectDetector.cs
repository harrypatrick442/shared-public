using JSON;
using Native.Messages;
namespace Native.Messaging
{
    public class PingDisconnectDetector
    {
        private System.Timers.Timer _TimerSendPing;
        private System.Timers.Timer _TimerCheckReceivedPin;
        private RegistrationMessageHandler _RegistrationMessageHandler;
        private readonly string PING_MESSAGE_STRING;
        private readonly object _LockObject = new object();
        private bool _SeenPing = true;
        private Action _Disconnected;
        public PingDisconnectDetector(
            RegistrationMessageHandler registrationMessageHandler,
            Action disconnected,
            int pingTimeoutMilliseconds,
            int sendPingIntervalMilliseconds) {
            _Disconnected = disconnected;
            PING_MESSAGE_STRING = Json.Serialize(new PingMessage());
            _TimerSendPing = new System.Timers.Timer(interval: sendPingIntervalMilliseconds);
            _TimerSendPing.AutoReset = true;
            _TimerSendPing.Elapsed += SendPing;
            _TimerCheckReceivedPin = new System.Timers.Timer(interval: pingTimeoutMilliseconds);
            _TimerCheckReceivedPin.AutoReset = true;
            _TimerCheckReceivedPin.Elapsed += CheckReceivedPing;
            _RegistrationMessageHandler = registrationMessageHandler;
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
                _TimerCheckReceivedPin.Stop();
                _TimerSendPing.Stop();
            }
        }
        public void StartClean()
        {
            lock (_LockObject)
            {
                _SeenPing = true;
                _TimerCheckReceivedPin.Start();
                _TimerSendPing.Start();
                try { _RegistrationMessageHandler.SendRaw(PING_MESSAGE_STRING); }
                catch
                {

                }
            }
        }
        private void CheckReceivedPing(object sender, System.Timers.ElapsedEventArgs e)
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
        }
        private void SendPing(object sender, System.Timers.ElapsedEventArgs e)
        {
            try { _RegistrationMessageHandler.SendRaw(PING_MESSAGE_STRING); }
            catch
            {

            }
        }
    }
}