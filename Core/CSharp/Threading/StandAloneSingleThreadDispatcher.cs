using Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
namespace Core.Threading {
    public class StandAloneSingleThreadDispatcher:IDisposable
    {
        private bool _Disposed = false;
        private LinkedList<Action> _Queue = new LinkedList<Action>();
        private CountdownLatch? _CountdownLatch;
        public StandAloneSingleThreadDispatcher() { 
            new Thread(Loop).Start();
        }
        public void Invoke(Action action) {
            lock (_Queue)
            {
                if (_Disposed) throw new ObjectDisposedException(nameof(StandAloneSingleThreadDispatcher));
                _Queue.AddLast(action);
                _CountdownLatch?.Signal();
                _CountdownLatch = null;
            }
        }
        private void Loop() {
            while (true) {
                Action? next;
                lock (_Queue)
                {
                    if (_Disposed) return;
                    next = _Queue.FirstOrDefault();
                    if (next != null)
                    {
                        _Queue.RemoveFirst();
                    }
                    else {
                        _CountdownLatch = new CountdownLatch();
                    }
                }
                if (next != null) {
                    try
                    {
                        next();
                    }
                    catch (Exception ex) {
                        Logs.Default.Error(ex);
                    }
                    continue;
                }
                _CountdownLatch!.Wait();
            }
        }
        ~StandAloneSingleThreadDispatcher() {
            Dispose();
        }
        public void Dispose()
        {
            lock (_Queue) {
                if (_Disposed) return;
                _Disposed = true;
                _CountdownLatch?.Signal();
                //Currently leaves remaining tasks unfulfilled. May wish to change this behaviour in future.
            }
            GC.SuppressFinalize(this);
        }
    }
}
