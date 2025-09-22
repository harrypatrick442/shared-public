using System;
using System.Threading.Tasks;
using TapoDevices.Credentials;
using TapoDevices.DeviceUsages;

namespace TapoDevices
{
    /// <summary>
    /// Represents connection to Tapo Smart Wi-Fi Socket P100/P110/P300 device.
    /// </summary>
    /// <remarks>SMART.TAPOPLUG</remarks>
    public class TapoPlug : TapoDevice
    {
        public TapoPlug(
            string ipAddress,
            TapoCredentials credentials) : base(ipAddress, credentials)
        {

        }

        public TapoPlug(
            string ipAddress,
            TapoCredentials credentials,
            TimeSpan defaultTimeout) : base(ipAddress, credentials, defaultTimeout)
        {

        }

        public async Task<DeviceUsagePlug> GetDeviceUsageAsync()
        {
            var request = GetDeviceUsage.CreateRequest();
            return await PostSecuredAsync<TapoRequest<GetDeviceUsage.Params>, DeviceUsagePlug>(request);
        }

        public async Task<GetEnergyUsage.ResultPlug> GetEnergyUsageAsync()
        {
            var request = GetEnergyUsage.CreateRequest();
            return await PostSecuredAsync<TapoRequest<GetEnergyUsage.Params>, GetEnergyUsage.ResultPlug>(request);
        }

        public async Task<GetLedInfo.ResultPlug> GetLedInfoAsync()
        {
            var request = GetLedInfo.CreateRequest();
            return await PostSecuredAsync<TapoRequest<GetLedInfo.Params>, GetLedInfo.ResultPlug>(request);
        }
    }
}
