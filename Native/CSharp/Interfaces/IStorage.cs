namespace Native.Interfaces
{
    public interface IStorage
    {
        public string GetString(string key);
        public void SetString(string key, string value);
        public void DeleteAll();
    }
}