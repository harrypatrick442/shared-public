using System;
using System.IO;
using System.Net;

namespace Core.Interfaces{
    public interface IExclusiveAccess
    {
        bool HaveExclusiveAccess { get; }
    }
}