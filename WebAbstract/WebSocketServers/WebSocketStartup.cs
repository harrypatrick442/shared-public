using WebSocketSharp.Server;
using SslProtocols = System.Security.Authentication.SslProtocols;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using Shutdown;

namespace WebAbstract
{
    public static class WebSocketStartup
    {
        public static WebSocketServer Run(Func<string, string> getConfigurationValue) {
#if RELEASE
            string fullChainPath = getConfigurationValue("Kestrel:Certificates:Default:Path");
            string privKeyPath = getConfigurationValue("Kestrel:Certificates:Default:KeyPath");

            WebSocketServer webSocketServer = new WebSocketServer(8443, true);
            webSocketServer.SslConfiguration.ServerCertificate = X509Certificate2.CreateFromPemFile(
                fullChainPath, privKeyPath);
            webSocketServer.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
            ShutdownManager.Instance.Add(webSocketServer.Stop, ShutdownOrder.WebSocketServers);
#else

                
            WebSocketServer webSocketServer = new WebSocketServer(8080, false);
#endif
            return webSocketServer;
        }
    }
}