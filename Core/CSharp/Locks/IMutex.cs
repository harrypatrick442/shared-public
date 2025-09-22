using System;
using System.Threading;

namespace Core.Locks
{
    public interface IMutex<TObjectCurrenlyTaking> :IDisposable
    {
        bool Enter(out TObjectCurrenlyTaking objectCurrentlyTaking, CancellationToken? canncellationToken, int? timeoutMilliseconds);
        void Exit();
    }
}