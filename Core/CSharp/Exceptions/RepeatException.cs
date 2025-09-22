using System;

namespace Core.Exceptions
{
    public class RepeatException :Exception
    {
        int N { get; }
        int LineNumber { get; }
        long LastLoggedAtMilliseconds { get; }
        public RepeatException(int n, int lineNumber, long lastLoggedAtMilliseconds, Exception ex) : base(null, ex) {
            N = n;
            LineNumber = lineNumber;
            LastLoggedAtMilliseconds = lastLoggedAtMilliseconds;
        }
        public override string ToString()
        {
            return $"{N} thrown from line number {LineNumber} at {LastLoggedAtMilliseconds}";
        }
    }
}
