using System;
using System.Text;
using System.Text.Json.Serialization;

namespace TapoDevices
{
    public class GetDeviceInfo
    {
        public class Params
        {

        }
        internal static TapoRequest<Params> CreateRequest() =>
            Utils.CreateTapoRequest<Params>("get_device_info", null);
    }
}
