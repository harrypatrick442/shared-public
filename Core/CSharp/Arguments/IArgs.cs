namespace Core.Arguments
{
    public interface IArgs//No not implement IEnumerable here as then it wont serialize properly.
    {
        bool HasArg(string key);
        string GetArgString(string key);
        T GetArgValue<T>(string key);
    }
}