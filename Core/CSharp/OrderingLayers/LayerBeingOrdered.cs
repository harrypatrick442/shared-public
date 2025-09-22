using Core.Interfaces;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System;
namespace Core.Geometry
{
    [DataContract]
    public class LayerBeingOrdered<TPayload>
    {
        private TPayload _Payload;
        public TPayload Payload { get { return _Payload; } }
        private Func<string> _GetName;
        public string GetName() {
            return _GetName();
        }
        public LayerBeingOrdered(TPayload payload, Func<string> getName)
        {
            if (payload == null) throw new ArgumentException("The payload was null");
            _Payload = payload;
            _GetName = getName;
        }
        protected LayerBeingOrdered() { }

    }
}
