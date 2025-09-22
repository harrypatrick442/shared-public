using System;
using InfernoDispatcher.Tasks;

namespace Core.Pool
{
    internal class ObjectPool_TaskAndCallback<TObject, TResult> : ObjectPool_TaskAndCallbackBase<TObject>
    {
        private Func<TObject, TResult> _Callback;
        private InfernoTaskWithResultBase<TResult> _Task;
        public ObjectPool_TaskAndCallback(Func<TObject, TResult> callback, InfernoTaskWithResultBase<TResult> task)
        {
            _Callback = callback;
            _Task = task;
        }

        public override void Run(TObject freeObject)
        {
            try
            {
                var result = _Callback(freeObject);
                _Task.Success(result);
            }
            catch (Exception ex)
            {
                _Task.Fail(ex);
            }
        }
    }
    internal class ObjectPool_TaskAndCallback<TObject> : ObjectPool_TaskAndCallbackBase<TObject>
    {
        private Action<TObject> _Callback;
        private InfernoTaskNoResultBase _Task;
        public ObjectPool_TaskAndCallback(Action<TObject> callback, InfernoTaskNoResultBase task)
        {
            _Callback = callback;
            _Task = task;
        }

        public override void Run(TObject freeObject)
        {
            try
            {
                _Callback(freeObject);
                _Task.Success();
            }
            catch (Exception ex)
            {
                _Task.Fail(ex);
            }
        }
    }
    internal class ObjectPool_TaskAndCallbackCreatingTask<TObject> : ObjectPool_TaskAndCallbackBase<TObject>
    {
        private Func<TObject, InfernoTaskNoResultBase> _Callback;
        private InfernoTaskNoResultBase _Task;
        public ObjectPool_TaskAndCallbackCreatingTask(Func<TObject, InfernoTaskNoResultBase> callback, InfernoTaskNoResultBase task)
        {
            _Callback = callback;
            _Task = task;
        }

        public override void Run(TObject freeObject)
        {
            try
            {
                var returnedTask = _Callback(freeObject);
                _Task.AddFrom(returnedTask);
                returnedTask.ThenWhatever(doneState =>
                {
                    if (doneState.Exception != null)
                    {
                        _Task.Fail(doneState.Exception);
                        return;
                    }
                    if (doneState.Canceled)
                    {
                        _Task.Cancel();
                        return;
                    }
                    _Task.Success();
                });
            }
            catch (Exception ex)
            {
                _Task.Fail(ex);
            }
        }
    }
    internal class ObjectPool_TaskAndCallbackCreatingTask<TObject, TResult> : ObjectPool_TaskAndCallbackBase<TObject>
    {
        private Func<TObject, InfernoTaskWithResultBase<TResult>> _Callback;
        private InfernoTaskWithResultBase<TResult> _Task;
        public ObjectPool_TaskAndCallbackCreatingTask(Func<TObject, InfernoTaskWithResultBase<TResult>> callback, InfernoTaskWithResultBase<TResult> task)
        {
            _Callback = callback;
            _Task = task;
        }

        public override void Run(TObject freeObject)
        {
            try
            {
                var returnedTask = _Callback(freeObject);
                _Task.AddFrom(returnedTask);
                returnedTask.ThenWhatever(doneState =>
                {
                    if (doneState.Exception != null)
                    {
                        _Task.Fail(doneState.Exception);
                        return;
                    }
                    if (doneState.Canceled)
                    {
                        _Task.Cancel();
                        return;
                    }
                    _Task.Success((TResult)doneState.Result![0]);
                });
            }
            catch (Exception ex)
            {
                _Task.Fail(ex);
            }
        }
    }
    internal abstract class ObjectPool_TaskAndCallbackBase<TObject>
    {
        public abstract void Run(TObject obj);

    }
}
