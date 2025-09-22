using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MessageTypes.Attributes;
using Core.Exceptions;

namespace Core.Enums
{
    public enum EmailInvalidReason
    {
        Invalid = 1,
        AlreadyInUse = 2
    }
}
