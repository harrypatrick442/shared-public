
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace WebAbstract
{
    public static class IpHelper
    {
        public static string GetClientIPAddress(HttpRequest httpRequest)
        {
            if (httpRequest.Headers.TryGetValue("X-Forwarded-For", out StringValues value))
                return value.ToString();
            return httpRequest.HttpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}