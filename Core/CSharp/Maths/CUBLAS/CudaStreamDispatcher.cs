using ManagedCuda;
using System;
using System.Collections.Generic;
using System.Threading;
namespace Core.Maths.CUBLAS
{

    public class CudaStreamDispatcher
    {
        private readonly object _LockObject = new object();
        private CudaStream _Stream;
        private ICudaContextWithThread _ContextWithThread;

        private bool _Cycling = false;
        private LinkedList<Action<CudaContextHandles>> _Waitings = new LinkedList<Action<CudaContextHandles>>();
        private CountdownLatch? _CountdownLatchSynchronize;
        private Exception? _ExceptionForThisPhase;
        private int _OwningThreadId;
        public CudaStreamDispatcher(CudaStream stream, ICudaContextWithThread contextWithThread) {
            _Stream = stream;
            _ContextWithThread = contextWithThread;
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            /*if (contextWithThread.ThreadId == currentThreadId) {
                throw new InvalidOperationException("You cannot construct on {nameof(contextWithThread)}.{nameof(contextWithThread.ThreadId)}. This can cause {nameof(Synchronize)} to lockup because it blocks the Cuda context assigned thread needed to execute remaining tasks.");
            }*/
            _OwningThreadId = currentThreadId;
        }
        public void Schedule(Action<CudaContextHandles> callback) {
            EnsureSameThread();
            lock (_LockObject)
            {
                if (_CountdownLatchSynchronize != null)
                {
                    throw new Exception($"{nameof(Synchronize)} was already called. This should be called after scheduling all tasks with {nameof(Schedule)}");
                }
                _Waitings.AddLast(callback);
                if (_Cycling) return;
                _Cycling = true;
            }
            Cycle();
        }
        private void Cycle()
        {
            try
            {
                _ContextWithThread.UsingContext(cudaContextHandles =>
                {
                    try
                    {
                        if (!IsStreamSynchronized())
                        {
                            Cycle();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                        return;
                    }
                    Action<CudaContextHandles>? next = null;
                    lock (_LockObject)
                    {
                        var nextNode = _Waitings.First;
                        if (nextNode == null)
                        {
                            _Cycling = false;
                            _CountdownLatchSynchronize?.Signal();
                            _CountdownLatchSynchronize = null;
                            return;
                        }
                        _Waitings.RemoveFirst();
                        next = nextNode.Value;
                    }
                    try
                    {
                        cudaContextHandles.SetStream(_Stream);
                        next(cudaContextHandles);
                        Cycle();
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        private void HandleException(Exception ex) {

            lock (_LockObject)
            {
                _Cycling = false;
                if (_ExceptionForThisPhase == null)
                {
                    _ExceptionForThisPhase = ex;
                }
                if (_CountdownLatchSynchronize != null)
                {
                    _CountdownLatchSynchronize.Signal();
                    _CountdownLatchSynchronize = null;
                }
            }
        }
        public void Synchronize()
        {
            EnsureSameThread();
            CountdownLatch countdownLatchSynchronize;
            lock (_LockObject)
            {
                if (_CountdownLatchSynchronize != null)
                {
                    throw new InvalidOperationException($"You should not call {nameof(Synchronize)} more than once per cycle");
                }
                if (!_Cycling) {
                    if (_ExceptionForThisPhase != null)
                    {
                        throw _ExceptionForThisPhase;
                    }
                    return;
                }
                _CountdownLatchSynchronize = new CountdownLatch();
                countdownLatchSynchronize = _CountdownLatchSynchronize;
            }
            countdownLatchSynchronize.Wait();
            lock (_LockObject)
            {
                if (_ExceptionForThisPhase != null)
                {
                    var exception = _ExceptionForThisPhase;
                    _ExceptionForThisPhase = null;
                    throw exception;
                }
            }
        }
        private bool IsStreamSynchronized()
        {
            int status = CudartInterop.cudaStreamQuery(_Stream.Stream.Pointer);
            if (status == 0) // cudaSuccess
            {
                return true; // Stream is synchronized
            }
            if (status == 1) // cudaErrorNotReady
            {
                return false; // Stream is still executing
            }
            throw new Exception($"CUDA stream error: {status}");
        }
        private void EnsureSameThread()
        {
            if (Thread.CurrentThread.ManagedThreadId != _OwningThreadId)
            {
                throw new InvalidOperationException($"All interactions with {nameof(CudaStreamDispatcher)} must occur on the thread that created it.");
            }
        }

    }
}