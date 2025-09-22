
namespace Shutdown
{
    public class Disposer : IShutdownable
    {
        private Action _Dispose;
        private ShutdownOrder _ShutdownOrder;
        public ShutdownOrder ShutdownOrder { get { return _ShutdownOrder; } }
        public Disposer(IDisposable iDisposable, ShutdownOrder shutdownOrder) {
            _Dispose = iDisposable.Dispose;
            _ShutdownOrder = shutdownOrder;
        }
        public Disposer(Action dispose, ShutdownOrder shutdownOrder)
        {
            _Dispose = dispose;
            _ShutdownOrder = shutdownOrder;
        }
        public void Dispose() { _Dispose(); }

        public override bool Equals(object obj)
        {
            return obj is Disposer disposer &&
                   EqualityComparer<Action>.Default.Equals(_Dispose, disposer._Dispose);
        }

        public override int GetHashCode()
        {
            return 304537551 + EqualityComparer<Action>.Default.GetHashCode(_Dispose);
        }
    }
}