using JSON;
namespace Core.Interfaces
{
    public interface IToJson
    {
        string ToJson(IJsonParser jsonParser);
    }
}
