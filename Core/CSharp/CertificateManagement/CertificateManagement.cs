
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
namespace Core.Configuration
{
    public static class CertificateManagement
    {
        public static void Setup()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
        }
    }
}