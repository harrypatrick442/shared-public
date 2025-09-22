using TapoSharp.Clients;
namespace SmartHomeCore.Tapo.Events
{
    public class ConnectedChangedEventArgs : EventArgs
    {
        public bool Connected { get; }
        public ConnectedChangedEventArgs(bool connected)
        {
            Connected = connected;
        }
    }
}
