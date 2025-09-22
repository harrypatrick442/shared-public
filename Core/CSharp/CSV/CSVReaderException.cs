using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
namespace Core.CSV
{
    public class CSVReaderException : Exception
    {
        internal CSVReaderException(string message):base(message) { }
        internal CSVReaderException(string message, Exception innerException) :base(message, innerException){ }    
    }
}