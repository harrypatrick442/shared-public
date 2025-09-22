namespace JSON
{
    public interface IJsonParser
    {
        string Serialize<TType>(TType instance, bool prettify = false);
        TType Deserialize<TType>(string jsonString);
    }
    public interface IJsonParser<TType>
    {
        string Serialize(TType item, bool prettify = false);
        TType Deserialize(string jsonString);
    }
}