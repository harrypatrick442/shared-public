
using Logging;
using Microsoft.Extensions.DependencyInjection;
using Nodes;
using UAParser;

namespace WebAbstract
{
    public static class UserAgentHelper
    {
        public static void GetFriendlyOperatingSystemAndBrowerName(string userAgentString, out string operatingSystem, out string browserName) {

            ClientInfo clientInfo = UAParser.Parser.GetDefault().Parse(userAgentString);
            operatingSystem = GetFriendlyOperatingSystem(clientInfo);
            browserName = GetFriendlyBrowserName(clientInfo);
        }
        private static string GetFriendlyOperatingSystem(ClientInfo clientInfo)
        {
            return clientInfo?.OS?.Family ?? "Unknown";
        }
        private static string GetFriendlyBrowserName(ClientInfo clientInfo)
        {
            if (clientInfo==null||clientInfo.UA.Family == null) return "Unknown";
            string str = clientInfo.UA.Family;
            if (clientInfo.UA.Major != null)
                str += $" {clientInfo.UA.Major}";
            if (clientInfo.UA.Minor != null)
                str+= $".{clientInfo.UA.Minor}";
            return str;
        }
    }
}