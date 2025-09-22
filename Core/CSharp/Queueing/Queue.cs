using System;
using System.Collections.Generic;
using Core.Threading;
using InfernoDispatcher;
using Logging;

namespace Core.Queueing
{
    public class Queue
    {
        public delegate void DelegateEntryHandler(object entry);
        public delegate object PickNextEntryToProcess(List<object> entries);
        protected List<object> _Entries = new List<object>();
        private int _MaxNEntriesToProcessAtOnce;
        private Dictionary<Type, DelegateEntryHandler> _MapTypeToHandler = new Dictionary<Type, DelegateEntryHandler>();
        private LimitedParallelDispatcher _LimitedParallelDispatcher;
        private PickNextEntryToProcess _PickNextEntryToProcess;
        public int NEntries { 
            get 
            {
                lock (_Entries) {
                    return _Entries.Count;
                }
            }
        }
        public Queue(int maxNEntriesToProcessAtOnce, PickNextEntryToProcess pickNextEntryToProcess) {
            if (maxNEntriesToProcessAtOnce < 1) throw new ArgumentException($"{nameof(maxNEntriesToProcessAtOnce)} must be greater than zero");
            _MaxNEntriesToProcessAtOnce = maxNEntriesToProcessAtOnce; 
            _LimitedParallelDispatcher = new LimitedParallelDispatcher(_MaxNEntriesToProcessAtOnce);
            _PickNextEntryToProcess = pickNextEntryToProcess != null ? pickNextEntryToProcess : PickNextQueueEntryToProcess;
        }
        private bool _ProcessingNewEntries = false;
        public void AddHandler(Type type, DelegateEntryHandler handler) {
            if (handler == null) throw new ArgumentNullException($"{nameof(handler)} was null");
            lock (_MapTypeToHandler) {
                if (_MapTypeToHandler.ContainsKey(type)) {
                    throw new ArgumentException($"Already has a handler registered to the entry {nameof(Type)} {type.Name}");
                }
                _MapTypeToHandler[type] = handler;
            }
        }
        public void Add(object entry) {
            if (entry == null)
                throw new ArgumentNullException($"{nameof(entry)} cannot be null");
            CheckHasHandlerForEntryType(entry.GetType());
            lock (_Entries)
            {
                _Entries.Add(entry);
                if (!_ProcessingNewEntries) {
                    StartProcessing();
                    _ProcessingNewEntries = true;
                }
            }
        }
        private void CheckHasHandlerForEntryType(Type entryType) {
            lock (_MapTypeToHandler) {
                if (!_MapTypeToHandler.ContainsKey(entryType))
                    throw new ArgumentException($"There is no handler registered for the entry Type {entryType.Name}");
            }
        }
        protected virtual object PickNextQueueEntryToProcess(List<object> queueEntries) {
            return queueEntries[0];
        }
        private object GetNextEntry() {

            lock (_Entries)
            {
                if (_Entries.Count < 1) {
                    _ProcessingNewEntries = false;
                    return null;
                }
                object next = _PickNextEntryToProcess(_Entries);
                _Entries.Remove(next);
                if (next == null) {
                    throw new NullReferenceException("Next should never be returned as null");
                }
                return next;
            }
        }
        private DelegateEntryHandler GetHandlerForEntryType(Type entryType) {
            lock (_MapTypeToHandler) {
                if (!_MapTypeToHandler.ContainsKey(entryType))
                {
                    Logs.Default.Warn($"Somehow an entry of type {entryType.Name} without a handler when it came to being processed. The handler must have been removed");
                    return null;
                }
                return _MapTypeToHandler[entryType];
            }            
        }
        private void ProcessEntry(object entry) {
            DelegateEntryHandler entryHandler = GetHandlerForEntryType(entry.GetType());
            if (entryHandler==null) return;
            entryHandler(entry);
        }
        private void StartProcessing() {
            Dispatcher.Instance.Run(() => {
                while (true) {

                    object nextEntry= GetNextEntry();
                    if (nextEntry == null) return;
                    _LimitedParallelDispatcher.Run(() => ProcessEntry(nextEntry));
                }
            });
        }
        
    }
}
