using Shutdown;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Core.Timing
{
    public class WebsocketReconnectTimer
    {
        Action _Reconnect;
        private readonly object _LockObject = new object();
        private bool _Disposed = false;
        private int _NReconnectsSinceOpened = 0;
        public WebsocketReconnectTimer(Action reconnect) {
            _Reconnect = reconnect;
        }
        public void Opened() {
            lock (_LockObject) {
                _NReconnectsSinceOpened = 0;
            }
        }
        public void Closed() {
            int delayMilliseconds = CalculateReconnectionDelayMilliseconds();
            lock (_LockObject)
            {
                if (_Disposed) return;
                if (_NReconnectsSinceOpened > 19) 
                    _NReconnectsSinceOpened = 0;
                else _NReconnectsSinceOpened++;
            }
            new Thread(() =>//TODO use timer?
            {
                if (delayMilliseconds < 1000)
                {
                    Thread.Sleep(delayMilliseconds);
                    lock (_LockObject)
                    {
                        if (_Disposed) return;
                    }
                }
                else
                {
                    int n = delayMilliseconds / 1000;
                    for (int i = 0; i < n; i++)
                    {
                        Thread.Sleep(delayMilliseconds);
                        lock (_LockObject)
                        {
                            if (_Disposed) return;
                        }
                    }
                }
                _Reconnect();
            }).Start();
        }
        private int CalculateReconnectionDelayMilliseconds()
        {
            lock (_LockObject)
            {
                if (_NReconnectsSinceOpened < 1) {
                    return 100;
                }
                if (_NReconnectsSinceOpened < 2)
                {
                    return 500;
                }
                if (_NReconnectsSinceOpened < 3)
                {
                    return 2000;
                }
                return 5000;
            }
        }
        public void Dispose() {
            lock (_LockObject) {
                _Disposed = true;
            }
        }
    }
}