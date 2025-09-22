using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System;
using Core.Enums;

namespace Core.Attributes
{
    [DataContract]
    public class PrimitiveAttribute:Attribute
    {
        private PrimitiveFlag _Flags;
        public PrimitiveFlag Flags { get { return _Flags; } }
        public PrimitiveAttribute(PrimitiveFlag flags) {
            _Flags = flags;
        }
    }
}
