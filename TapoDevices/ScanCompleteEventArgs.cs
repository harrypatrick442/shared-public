using System;
using System.Collections.Generic;
using System.Linq;
using TapoDevices.DeviceInfos;
namespace TapoDevices
{
    public class ScanCompleteEventArgs : EventArgs
    {
        private IEnumerable<DeviceInfo> _Clients;
        public ScanCompleteEventArgs(IEnumerable<DeviceInfo> clients)
        {
            _Clients = clients;
        }
        public DeviceInfo? TryGetByDeviceId(string deviceId)
        {
            return _Clients
                .Where(c => c.DeviceId == deviceId)
                .FirstOrDefault();
        }
    }
}
