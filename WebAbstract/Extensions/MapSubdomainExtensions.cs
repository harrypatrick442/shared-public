using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace WebAbstract.Extensions
{
    public static class MapSubdomainExtensions
    {
        public static IApplicationBuilder MapSubdomain(this IApplicationBuilder app,
            string subdomain, Action<IApplicationBuilder> configuration)
        {
            return app.UseWhen(GetSubdomainPredicate(subdomain), configuration);
        }

        private static Func<HttpContext, bool> GetSubdomainPredicate(string subdomain)
        {
            return (HttpContext context) =>
            {
                string hostSubdomain = context.Request.Host.Host.Split('.')[0];

                return (subdomain == hostSubdomain);
            };
        }
    }
}