using Enums;
using Logging;
using System;

namespace Logging.Writers
{
    public class ConsoleWriter : ILogWriter
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
        public virtual void WriteString(string str, LogLevel logLevel)
        {
#if !DEVELOPMENT
            if (logLevel.Equals(LogLevel.Debug)) return;
#endif
            try
            {
                Console.WriteLine(logLevel.GetString() + ": " + str);
            }
            catch { }
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
