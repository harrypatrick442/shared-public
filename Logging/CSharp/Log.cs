using Enums;
using Logging.Writers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logging
{
    public class Log : ILog
    {

        private object _LockObjectLogWriters = new object();
        public TLogWriter GetLogWriter<TLogWriter>() where TLogWriter : ILogWriter
        {
            return (TLogWriter)_ILogWriters.Where(iLogWriter => typeof(TLogWriter).IsAssignableFrom(iLogWriter.GetType())).FirstOrDefault();
        }
        private static Log _None;
        public static Log None
        {
            get
            {
                if (_None == null)
                    _None = new Log(new NoneWriter());
                return _None;
            }
        }
        private List<ILogWriter> _ILogWriters;
        public Log(params ILogWriter[] iLogWriters)
        {
            _ILogWriters = iLogWriters.ToList();
            foreach (ILogWriter iLogWriter in iLogWriters) iLogWriter.Initialize();
        }
        public void AddLogWriter(ILogWriter iLogWriter)
        {

            lock (_LockObjectLogWriters)
            {
                if (_ILogWriters.Contains(iLogWriter)) return;
                iLogWriter.Initialize();
                _ILogWriters.Add(iLogWriter);
            }
        }
        public void Clear()
        {
            ILogWriter[] iLogWriters = null;
            lock (_LockObjectLogWriters)
            {
                iLogWriters = _ILogWriters.ToArray();
            }
            foreach (ILogWriter iLogWriter in iLogWriters)
            {
                iLogWriter.Clear();
            }
        }
        public virtual void Breadcrumb(IBreadcrumb breadcrumb)
        {
            _Breadcrumb(breadcrumb);
        }
        public virtual void Info(object o)
        {
            _Log(o, LogLevel.Info);
        }
        public virtual void Error(object o)
        {
            _Log(o, LogLevel.Error);
        }
        public virtual void Warn(object o)
        {
            _Log(o, LogLevel.Warning);
        }
        public virtual void Debug(object o)
        {
            try
            {
                _Log(o, LogLevel.Debug);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
        private void _Log(object o, LogLevel logLevel)
        {
            _Write(o, logLevel);
        }
        private void _Breadcrumb(IBreadcrumb breadcrumb)
        {
            ILogWriter[] iLogWriters = null;
            lock (_LockObjectLogWriters)
            {
                iLogWriters = _ILogWriters.ToArray();
            }
            foreach (ILogWriter iLogWriter in iLogWriters) iLogWriter.Breadcrumb(breadcrumb);
        }
        private void _Write(object o, LogLevel logLevel)
        {
            ILogWriter[] iLogWriters = null;
            lock (_LockObjectLogWriters)
            {
                iLogWriters = _ILogWriters.ToArray();
            }
            foreach (ILogWriter iLogWriter in iLogWriters) iLogWriter.Write(o, logLevel);
        }
    }
}
