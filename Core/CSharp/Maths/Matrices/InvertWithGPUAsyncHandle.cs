using System;
namespace Core.Maths
{
    public class InvertWithGPUAsyncHandle : IDisposable
    {
        private Action _Invert, _Synchornize, _Dispose;
        public InvertWithGPUAsyncHandle(Action invert, Action synchronize, Action dispose)
        {
            _Invert = invert;
            _Synchornize = synchronize;
            _Dispose = dispose;
        }
        public void Invert()
        {
            _Invert();
        }
        public void Synchronize()
        {
            _Synchornize();
        }
        public void Dispose()
        {
            _Dispose.Invoke();
        }
    }
}
