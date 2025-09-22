using System.Runtime.Serialization;

namespace Emailing
{
    [DataContract]
    public class SmtpServerConfiguration
    {
        public string Domain { get; }
        public string Username { get; }
        public string Password { get; }
        public int? Port { get;}
        public bool UseSsl { get;}
        public SmtpServerConfiguration(string username, string password, int port, bool useSsl, string domain)
        {
            Username = username;
            Password = password;
            Port = port;
            UseSsl = useSsl;
            Domain = domain;
        }
    }
}