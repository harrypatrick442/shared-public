using SmartHomeCore.Tapo.Events;
namespace SmartHomeCore.Tapo
{
    public interface IDeviceConnectedStatus
    {
        public bool Connected {  get; }
        public event EventHandler<ConnectedChangedEventArgs> ConnectedChanged;
    }
}