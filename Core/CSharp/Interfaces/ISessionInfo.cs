namespace Core.Interfaces
{
    public interface ISessionInfo
    {
        string DeviceIdentifier { get; }
        long SessionId { get; }
        string Token { get; }
        long UserId { get; }
        void Dispose();
    }
}