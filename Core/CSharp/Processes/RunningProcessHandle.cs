using System;
using System.Diagnostics;

namespace Core.Processes
{
    public class RunningProcessHandle : IDisposable
    {
        private Action _Dispose;
        private Action<int?> _Wait;
        private bool _Disposed = false;
        private Process _Process;
        public RunningProcessHandle(Action dispose, Action<int?> wait, Process process)
        {
            _Dispose = dispose;
            _Wait = wait;
            _Process = process;
        }
        public void WriteLine(string str) {
            _Process.StandardInput.WriteLine(str);
        }
        public int Wait(int? millisecondsTimeout = null)
        {
            _Wait(millisecondsTimeout);
            return _Process.ExitCode;
        }

        ~RunningProcessHandle() {
            Dispose();
        }
        public void Dispose()
        {
            lock (_Dispose)
            {
                if (_Disposed) return;
                _Disposed = true;
                _Dispose();
            }
        }
    }
}
