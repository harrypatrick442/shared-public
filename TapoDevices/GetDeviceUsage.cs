using System.Text.Json.Serialization;

namespace TapoDevices
{
    public class GetDeviceUsage
    {
        public class Params
        {

        }




        internal static TapoRequest<Params> CreateRequest() =>
            Utils.CreateTapoRequest<Params>("get_device_usage", null);
    }
}
