using Enums;
using Logging;
using System;

namespace Logging.Writers
{
    public class DiagnosticsConsoleWriter : ILogWriter
    {
        public void Initialize()
        {

        }
        public void Write(object o, LogLevel logLevel)
        {
            if (o == null) return;
            string str = ParseToString(o);
            WriteString(str, logLevel);
        }
        protected virtual void WriteString(string str, LogLevel logLevel)
        {
#if !DEVELOPMENT
            if (logLevel.Equals(LogLevel.Debug)) return;
#endif
            System.Diagnostics.Debug.WriteLine(logLevel.GetString() + ": " + str);
        }
        private string ParseToString(object o)
        {
            Type type = o.GetType();
            if (type == typeof(string))
            {
                return (string)o;
            }
            if (typeof(Exception).IsAssignableFrom(type))
            {
                return ((Exception)o).ToString();
            }
            return o.ToString();
        }
        public void Clear()
        {

        }
        public void Breadcrumb(IBreadcrumb breadcrumb)
        {

        }
    }
}
