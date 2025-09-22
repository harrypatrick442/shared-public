using System;
using TapoDevices.Credentials;

namespace TapoDevices
{
    public class TapoDeviceFactory
    {
        TapoCredentials _Credentials;

        public TapoDeviceFactory(TapoCredentials credentials)
        {
            _Credentials = credentials;
        }

        public TapoBulb CreateBulb(string ipAddress) =>
            new TapoBulb(ipAddress, _Credentials);

        public TapoBulb CreateBulb(string ipAddress, TimeSpan defaultTimeout) =>
            new TapoBulb(ipAddress, _Credentials, defaultTimeout);

        public TapoPlug CreatePlug(string ipAddress) =>
            new TapoPlug(ipAddress, _Credentials);

        public TapoPlug CreatePlug(string ipAddress, TimeSpan defaultTimeout) =>
            new TapoPlug(ipAddress, _Credentials, defaultTimeout);
    }
}
