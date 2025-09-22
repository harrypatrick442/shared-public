
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Handlers
{
    public class MessageTypeMappingsHandler<TPayload> where TPayload:ITypedMessage
    {
        private Dictionary<string, DelegateHandleMessageOfType<TPayload>> _MapTypeToHandleMessage;
        public MessageTypeMappingsHandler(TupleList<string, DelegateHandleMessageOfType<TPayload>> mappings)
        {
            AddRange(mappings);
        }
        public MessageTypeMappingsHandler(List<Tuple<string, DelegateHandleMessageOfType<TPayload>>> mappings)
        {
            AddRange(mappings);
        }
        public MessageTypeMappingsHandler()
        {
            _MapTypeToHandleMessage = new Dictionary<string, DelegateHandleMessageOfType<TPayload>>();
        }
        public bool HandleMessage(TPayload payload) {
            DelegateHandleMessageOfType<TPayload> handleMessage = null;
            lock (_MapTypeToHandleMessage) {
                if (!_MapTypeToHandleMessage.TryGetValue(payload.Type, 
                    out handleMessage))
                    return false;
            }
            handleMessage(payload);
            return true;
        }
        public Action Add(string type, DelegateHandleMessageOfType<TPayload> handler) {
            lock (_MapTypeToHandleMessage)
            {
                if (_MapTypeToHandleMessage.ContainsKey(type))
                    throw new ArgumentException($"Contested type \"{type}\"");
                _MapTypeToHandleMessage[type] = handler;
                bool doneRemove = false;
                return () =>
                {
                    lock (_MapTypeToHandleMessage)
                    {
                        if (doneRemove) return;
                        doneRemove = true;
                        _MapTypeToHandleMessage.Remove(type);
                    }
                };
            }
        }
        public Action AddRange(List<Tuple<string, DelegateHandleMessageOfType<TPayload>>> mappings) {
            List<string> mappingsAdded = new List<string>();
            lock (_MapTypeToHandleMessage)
            {
                foreach (Tuple<string, DelegateHandleMessageOfType<TPayload>> mapping in mappings.ToList())

                {
                    string type = mapping.Item1;
                    if (_MapTypeToHandleMessage.ContainsKey(type))
                        throw new ArgumentException($"Contested type \"{type}\"");
                    _MapTypeToHandleMessage[type] = mapping.Item2;
                    mappingsAdded.Add(mapping.Item1);
                }
            }
            return Get_RemoveRange(mappingsAdded.ToArray());
        }
        public Action Get_RemoveRange(string[]mappings)
        {
            bool done = false;
            return () =>
            {
                lock (_MapTypeToHandleMessage)
                {
                    if (done) return;
                    done = true;
                    foreach (string mapping in mappings)
                    {
                        _MapTypeToHandleMessage.Remove(mapping);
                    }
                }
            };
        }
    }
}