
namespace Shutdown
{
    public interface IShutdownable:IDisposable
    {
        ShutdownOrder ShutdownOrder { get; }
    }
}