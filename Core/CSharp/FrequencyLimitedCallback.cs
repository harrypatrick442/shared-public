using System;
using System.Threading;
using InfernoDispatcher;
using Initialization.Exceptions;
using Shutdown;

namespace Core
{
    public class FrequencyLimitedCallback
    {
        private volatile static bool _Initialized = false, _Disposed = false;
        private volatile bool _Waiting = false;
        public bool Waiting { get { return _Waiting; } }

        private const int 
            DEFAULT_MIN_DELAY_MILLISECONDS=1000;
        private static IShutdownable _IShutdownable= new ShutdownHandler();
        private static CancellationTokenSource _CancellationTokenSourceDisposed = new CancellationTokenSource();
        private int _MinDelayMilliseconds;
        private long _LastDidCallback = 0;
        private Action _Callback;
        private object
            _LockObjectWaiting = new object();
        private static object
            _LockObjectDisposed = new object();
        public FrequencyLimitedCallback(Action callback, int minDelayMilliseconds= DEFAULT_MIN_DELAY_MILLISECONDS) {
            _Callback = callback;
            _MinDelayMilliseconds = minDelayMilliseconds;
        }
        public static void Initialize(ShutdownManager shutdownManager) {
            if (_Initialized) throw AlreadyInitializedException.ForThis();
            _Initialized = true;
            shutdownManager.Add(_IShutdownable);
        }
        private static void CheckInitialized() {
            if (!_Initialized) throw NotInitializedException.ForThis();
        }
        public void Trigger() {
            CheckInitialized();
            if (_Disposed) return;
            lock (_LockObjectWaiting)
            {
                if (_Disposed) return;
                if (_Waiting) return;
                _Waiting = true;
                Dispatcher.Instance.Run(() =>
                {
                    long now = DateTime.Now.ToFileTimeUtc();
                    long millisecondsToWait = _MinDelayMilliseconds - (now - _LastDidCallback);
                    if (millisecondsToWait <= 0) millisecondsToWait = 0;
                    else _CancellationTokenSourceDisposed.Token.WaitHandle.WaitOne((int)millisecondsToWait);
                    lock (_LockObjectWaiting)
                    {
                        _LastDidCallback = now + millisecondsToWait;
                        _Callback();
                        _Waiting = false;
                    }
                });
            }
        }
        private class ShutdownHandler:IShutdownable
        {
            public ShutdownOrder ShutdownOrder => ShutdownOrder.FrequencyLimitedCallback;
            public void Dispose()
            {
                lock (_LockObjectDisposed)
                {
                    if (_Disposed) return;
                    _Disposed = true;
                }
                _CancellationTokenSourceDisposed.Cancel();
            }
        }
    }
}
