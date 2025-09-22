namespace Core.Interfaces
{
    public interface IClientEndpointLight
    {
        void SendObject<TObject>(TObject obj);
        void SendJSONString(string jsonString);
    }
}