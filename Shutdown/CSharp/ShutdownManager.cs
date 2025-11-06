using Initialization.Exceptions;
using Logging;

namespace Shutdown
{
    public class ShutdownManager
    {
        private Func<ILog> _GetLog;
        private ILog _Log { get { return _GetLog(); } }
        private static ShutdownManager _Instance;
        private static object _InitializeLockObject = new object();
        private List<IShutdownable> _IShutdownables = new List<IShutdownable>();
        private volatile bool _DidShutdown = false;
        private object _DidShutdownLockObject = new object();
        private static Action<int> _ApplicationShutdown;
        private CancellationTokenSource _CancellationTokenSource = new CancellationTokenSource();
        public CancellationToken CancellationToken { get { return _CancellationTokenSource.Token; } }
        public bool ShuttingDown { get { return _DidShutdown; } }
        public static ShutdownManager Instance { get {
                if (_Instance == null)
                    throw NotInitializedException.ForThis();
                return _Instance;
            }
        }
        private ShutdownManager(Func<ILog> getLog)
        {
            _GetLog = getLog;
        }
        public static ShutdownManager Initialize(Action<int> applicationShutdown, Func<ILog> getLog, bool throwErrorOnAlreadyInitialized=true) {
            _ApplicationShutdown = applicationShutdown;
            lock (_InitializeLockObject){
                if (_Instance != null)
                {
                    if (!throwErrorOnAlreadyInitialized) return _Instance;
                    throw AlreadyInitializedException.ForThis();
                }
                _Instance = new ShutdownManager(getLog);
                return _Instance;
            }
        }
        public void AddRange(params IShutdownable[] iShutdownables)
        {
            foreach (IShutdownable iShutdownable in iShutdownables) Add(iShutdownable);
        }
        public void AddRange(ShutdownOrder shutdownOrder, params IDisposable[] iDisposables) {
            foreach (IDisposable iDisposable in iDisposables) Add(iDisposable, shutdownOrder);
        }
        public void Add(IDisposable iDisposable, ShutdownOrder shutdownOrder) 
        {
            Add(new Disposer(iDisposable, shutdownOrder));
        }
        public void Add(Action disposeAction, ShutdownOrder shutdownOrder) {
            Add(new Disposer(disposeAction, shutdownOrder));
        }
        public void Add(IShutdownable iShutdownable)
        {
            lock (_DidShutdownLockObject)
            {
                if (_DidShutdown)
                {
                    Dispose(iShutdownable);
                    return;
                }
                if (_IShutdownables.Contains(iShutdownable)) return;
                _IShutdownables.Add(iShutdownable);
            }
        }
        public void Shutdown(int exitCode = 0) {
            new Thread(() =>
            {
                _Log.Info("Called Shutdown");
                lock (_DidShutdownLockObject)
                {
                    if (_DidShutdown) return;
                    _DidShutdown = true;
                    _CancellationTokenSource.Cancel();
                    foreach (IDisposable iDisposable in _IShutdownables.OrderBy(iShutdownable => iShutdownable.ShutdownOrder).ToArray())
                    {
                        Dispose(iDisposable);
                    }
                }
                if (_ApplicationShutdown != null)
                    _ApplicationShutdown(exitCode);
                else
                    Environment.Exit(exitCode);
            }).Start();
        }   
        private void Dispose(IDisposable iDisposable) {

            try
            {
                iDisposable.Dispose();
            }
            catch (Exception ex)
            {
                _Log.Error(ex);
            }
        }
    }
}