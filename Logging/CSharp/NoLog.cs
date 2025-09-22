using System;

namespace Logging
{
    public class NoLog : ILog
    {
        public TLogWriter GetLogWriter<TLogWriter>() where TLogWriter : ILogWriter
        {
            return default;
        }
        public void Clear()
        {
        }
        public virtual void Breadcrumb(BreadcrumbCategoryName breadcrumbCategoryName, string message, BreadcrumbLevel breadcrumbLevel = BreadcrumbLevel.Info)
        {
        }
        public virtual void Breadcrumb(IBreadcrumb breadcrumb)
        {
        }
        public virtual void Info(object o)
        {
        }
        public virtual void Error(object o)
        {
        }
        public virtual void Warn(object o)
        {
        }
        public virtual void Debug(object o)
        {
        }

        public void AddLogWriter(ILogWriter logWriter)
        {
            throw new NotImplementedException();
        }
    }
}
