using System;
using System.IO;
using System.Net;

namespace Core.CSV
{

    public interface ICSVBuffer
    {
        bool HasNext
        {
            get;
        }
        char Next
        {
            get;
        }
        char Current
        {
            get;
        }
        char[] Advance(int nIndices);
        char LookAhead(int nIndices);
    }
}
