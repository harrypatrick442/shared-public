using System;
namespace Core.Maths
{
    public class InvertWithGPUHandle : IDisposable
    {
        private Action _Invert;
        private Action _Dispose;
        public InvertWithGPUHandle(Action invert, Action dispose)
        {
            _Invert = invert;
            _Dispose = dispose;
        }
        public void Invert()
        {
            _Invert();
        }
        public void Dispose()
        {
            _Dispose.Invoke();
        }
    }
}
