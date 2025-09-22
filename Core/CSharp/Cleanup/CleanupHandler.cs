using System;
using System.Collections.Generic;
using Logging;
namespace Core.Cleanup
{
    public class CleanupHandler:IDisposable
    {
        private bool _Disposed = false;
        private List<Action> _Disposes = new List<Action>();
        public void Add(IDisposable disposable)
        {
            Add(disposable.Dispose);
        }
        public void Add(Action dispose)
        {
            lock (_Disposes)
            {
                if (!_Disposed)
                {
                    _Disposes.Add(dispose);
                    return;
                }
            }
            dispose();
        }
        ~CleanupHandler() {
            Dispose();
        }
        public void Dispose() {
            Action[] disposes;
            lock(_Disposes)
            {
                if (_Disposed) return;
                _Disposed = true;
                disposes = _Disposes.ToArray();
                _Disposes.Clear();
            }
            for(int i=0; i<disposes.Length; i++)
            {
                Action dispose = disposes[i];
                try
                {
                    dispose();
                }
                catch(Exception ex) {
                    Logs.Default.Error(ex);
                }
            }
        }
    }
}
