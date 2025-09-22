using System.Collections.Concurrent;
using System;
using Core.Timing;
using Core.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace Core.Pool
{
	public class ObjectPool<TObject> where TObject : class
    {
        private readonly HashSet<TObject>
            _FreeObjects = new HashSet<TObject>();
        private Func<TObject> _GenerateObject;
        private bool _Disposed = false;
        private CountdownLatch _CountdownLatchWaitForReturn;
        private int _WaitingCount = 0, _MaxSize, _NEntries=0;
        private readonly int _MaxWaitingBeforeBackedUp;
        private Action<TObject> _DisposeObject;
        public ObjectPool(Func<TObject> generateObject, 
            int maxWaitingBeforeBackedUp,
            int maxSize,
            Action<TObject> disposeObject)
        {
            if (maxSize <= 0)
                throw new ArgumentException(nameof(maxSize));
            _GenerateObject = generateObject;
            _MaxWaitingBeforeBackedUp = maxWaitingBeforeBackedUp;
            _MaxSize = maxSize;
            _DisposeObject = disposeObject;
        }
        public ObjectPoolHandle<TObject> TakeWhenCan(long? timeoutMilliseconds) {
            long? startedWait = null;
            while (true)
            {
                CountdownLatch countdownLatchWaitForReturn;
                lock (_FreeObjects)
                {
                    TObject freeObject;
                    freeObject = _FreeObjects.FirstOrDefault();
                    if (freeObject != null)
                    {
                        _FreeObjects.Remove(freeObject);
                        return new ObjectPoolHandle<TObject>(freeObject, ReturnObject);
                    }
                    if (_Disposed)
                        throw new ObjectDisposedException(nameof(ObjectPool<TObject>)); 
                    if (_GenerateObject != null&& _NEntries < _MaxSize)
                    {
                        freeObject = _GenerateObject();
                        if (freeObject != null)
                        {
                            _NEntries++;
                            return new ObjectPoolHandle<TObject>(freeObject, ReturnObject);
                        }
                    }
                    if (timeoutMilliseconds != null)
                    {
                        if (startedWait == null)
                            startedWait = TimeHelper.MillisecondsNow;
                        else
                        {
                            if (TimeHelper.MillisecondsNow - (long)startedWait > timeoutMilliseconds)
                                throw new TimeoutException();
                        }
                    }
                    if (_CountdownLatchWaitForReturn == null)
                    {
                        _CountdownLatchWaitForReturn = new CountdownLatch();
                        _WaitingCount = 1;
                    }
                    else
                    {
                        _WaitingCount++;
                        if (_WaitingCount > _MaxWaitingBeforeBackedUp)
                        {
                            throw new BackedUpException($"Backed up with {_WaitingCount}");
                        }
                    }
                    countdownLatchWaitForReturn = _CountdownLatchWaitForReturn;
                }
                countdownLatchWaitForReturn.Wait();
            }
        }
        private void ReturnObject(TObject obj) {
            lock(_FreeObjects)
            {
                if (_Disposed) {
                    _DisposeObject?.Invoke(obj);
                }
                else
                {
                    _FreeObjects.Add(obj);
                }
                if (_CountdownLatchWaitForReturn != null) {
                    _CountdownLatchWaitForReturn.Signal();
                    _CountdownLatchWaitForReturn = null;
                    _WaitingCount = 0;
                }
            }
        }
        public void Dispose()
        {
            lock (_FreeObjects)
            {
                if(_Disposed) return;
                _Disposed = true;
                if (_DisposeObject != null)
                {
                    foreach (TObject obj in _FreeObjects)
                    {
                        _DisposeObject(obj);
                    }
                }
                _FreeObjects.Clear();
                _CountdownLatchWaitForReturn?.Signal();
                _CountdownLatchWaitForReturn = null;
            }
        }
    }
}
