
using System;

public class CleanupHandle : IDisposable
{
    private readonly Action _CallbackCleanup;
    private readonly object _LockObjectDispose = new object();
    private bool _Disposed = false;

    public CleanupHandle(Action callbackCleanup)
    {
        _CallbackCleanup = callbackCleanup ?? throw new ArgumentNullException(nameof(callbackCleanup));
    }

    public CleanupHandle(IDisposable disposable)
    {
        if(disposable==null) throw new ArgumentNullException(nameof(disposable));
        _CallbackCleanup = disposable.Dispose;
    }

    public void Dispose()
    {
        lock (_LockObjectDispose)
        {
            if (_Disposed) return;
            _Disposed = true;
            _CallbackCleanup();
        }
        GC.SuppressFinalize(this);
    }
}