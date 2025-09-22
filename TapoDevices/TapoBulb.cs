using System;
using System.Threading.Tasks;
using TapoDevices.Credentials;
using TapoDevices.DeviceInfos;
using TapoDevices.DeviceUsages;

namespace TapoDevices
{
    /// <summary>
    /// Represents connection to Tapo Smart Light Bulb L510/L530 device.
    /// </summary>
    /// <remarks>SMART.TAPOBULB</remarks>
    public class TapoBulb : TapoDevice
    {
        public TapoBulb(
            string ipAddress,
            TapoCredentials credentials) : base(ipAddress, credentials)
        {

        }

        public TapoBulb(
            string ipAddress,
            TapoCredentials credentials,
            TimeSpan defaultTimeout) : base(ipAddress, credentials, defaultTimeout)
        {

        }

        public new async Task<DeviceInfoBulb> GetInfoAsync()
        {
            var request = GetDeviceInfo.CreateRequest();
            return await PostSecuredAsync<TapoRequest<GetDeviceInfo.Params>, DeviceInfoBulb>(request);
        }

        public async Task<DeviceUsageBulb> GetDeviceUsageAsync()
        {
            var request = GetDeviceUsage.CreateRequest();
            return await PostSecuredAsync<TapoRequest<GetDeviceUsage.Params>, DeviceUsageBulb>(request);
        }

        public async Task SetBrightnessAsync(int brightness)
        {
            var request = SetDeviceInfo.CreateRequest(new SetDeviceInfo.ParamsBulb { Brightness = brightness }); // TODO: check range
            await PostSecuredAsync<TapoRequest<SetDeviceInfo.ParamsBulb>, SetDeviceInfo.Result>(request);
        }

        public async Task SetColorAsync(int hue, int saturation)
        {
            var request = SetDeviceInfo.CreateRequest(new SetDeviceInfo.ParamsBulb
            {
                ColorTemperature = 0,
                Hue = hue,
                Saturation = saturation,
            }); // TODO: ? check range

            await PostSecuredAsync<TapoRequest<SetDeviceInfo.ParamsBulb>, SetDeviceInfo.Result>(request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>L510 model can accept only 0 or 2700 value of color temperature.</remarks>
        /// <param name="colorTemperature"></param>
        /// <returns></returns>
        public async Task SetColorTemperatureAsync(int colorTemperature)
        {
            var request = SetDeviceInfo.CreateRequest(new SetDeviceInfo.ParamsBulb { ColorTemperature = colorTemperature }); // TODO: ? check range
            await PostSecuredAsync<TapoRequest<SetDeviceInfo.ParamsBulb>, SetDeviceInfo.Result>(request);
        }

        public async Task SetParametersAsync(SetDeviceInfo.ParamsBulb parameters)
        {
            var request = SetDeviceInfo.CreateRequest(parameters);
            await PostSecuredAsync<TapoRequest<SetDeviceInfo.ParamsBulb>, SetDeviceInfo.Result>(request);
        }
    }
}
