using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    public class InterserverException :Exception
    {
        public InterserverException(string message) : base(message) { 
            
        }
    }
}
