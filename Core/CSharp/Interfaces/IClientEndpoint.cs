using System.Net;

namespace Core.Interfaces
{
    public interface IClientEndpoint
    {
        long SessionId { get; }
        long UserId { get; }
        ISessionInfo SessionInfoSafe { get; }
        public bool HasSession { get; }
        void SendJSONString(string jsonString);
        void SendObject<TObject>(TObject obj) where TObject : class;
        void Dispose();
        IPAddress ClientIPAddress { get; }
    }
}