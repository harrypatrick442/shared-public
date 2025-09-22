using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using Logging;

namespace Core.Locks
{
    public class SequentialScheduler : TaskScheduler, IDisposable
    {
        private readonly LinkedList<Task> _Tasks = new LinkedList<Task>();
        private bool _OutOfTasks = false;
        public bool OutOfTasks { get { return _OutOfTasks; } }
        public event EventHandler OnOutOfTasks;
        private readonly Thread _Thread;
        private readonly CancellationTokenSource _CancellationTokenSourceDisposed = new CancellationTokenSource();
        private CountdownLatch _CountdownLatch = new CountdownLatch();
        private volatile bool _Disposed = false; 

        public SequentialScheduler()
        {
            _Thread = new Thread(Run);
            _Thread.Start();
        }

        public void Dispose()
        {
            _Disposed = true;
            _CancellationTokenSourceDisposed.Cancel();
        }

        void Run()
        {
            while (true)
            {
                try
                {
                    if (_Disposed) break;
                    var task = GetNextTaskOrWaitForOne();
                    if (_Disposed) break;
                    TryExecuteTask(task);
                }
                catch (Exception exception)
                {
                    try
                    {
                        Logs.Default.Error(exception);
                    }
                    catch (Exception ex) {
                        Console.WriteLine(exception);
                    }
                }
            }
        }
        private Task GetNextTaskOrWaitForOne()
        {
            lock (_CountdownLatch)
            {
                lock (_Tasks)
                {
                    if (_Tasks.Count > 0)
                    {
                        Task task =  _Tasks.First.Value;
                        _Tasks.RemoveFirst();
                        return task;
                    }
                    _CountdownLatch = new CountdownLatch();
                    _OutOfTasks = true;
                }
            }
            DispatchOutOfTasks();
            _CountdownLatch.Wait(_CancellationTokenSourceDisposed.Token);
            if (_Disposed) return null;
            lock (_CountdownLatch)
            {
                lock (_Tasks)
                {
                    Task task = _Tasks.First.Value;
                    _Tasks.RemoveFirst();
                    return task;
                }
            }
        }
        private void DispatchOutOfTasks() {
            OnOutOfTasks?.Invoke(this, new EventArgs());
        }
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            lock(_Tasks)return _Tasks;
        }

        protected override void QueueTask(Task task)
        {
            if (_Disposed) throw new ObjectDisposedException(nameof(SequentialScheduler));
            lock(_CountdownLatch){
                lock (_Tasks)
                {
                    _Tasks.AddLast(task);
                    _OutOfTasks = false;
                    if (_CountdownLatch != null) _CountdownLatch.Signal();
                }
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (Thread.CurrentThread == _Thread)
            {
                return TryExecuteTask(task);
            }
            return false;
        }
    }
}