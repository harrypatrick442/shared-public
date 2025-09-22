namespace Core.Locks
{
    public interface IWatchableLock
    {
        string StackTrace{get; }
        long CreatedAt { get; }
    }
}