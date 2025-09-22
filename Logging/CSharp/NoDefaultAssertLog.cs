using System;

namespace Logging
{
    public class NoDefaultAssertLog : ILog
    {
        public TLogWriter GetLogWriter<TLogWriter>() where TLogWriter : ILogWriter
        {
            return default;
        }
        public void Clear()
        {
            AssertOnDevelopment();
        }
        public virtual void Breadcrumb(BreadcrumbCategoryName breadcrumbCategoryName, string message, BreadcrumbLevel breadcrumbLevel = BreadcrumbLevel.Info)
        {

            AssertOnDevelopment();
        }
        public virtual void Breadcrumb(IBreadcrumb breadcrumb)
        {

            AssertOnDevelopment();
        }
        public virtual void Info(object o)
        {

            AssertOnDevelopment();
        }
        public virtual void Error(object o)
        {

            AssertOnDevelopment();
        }
        public virtual void Warn(object o)
        {

            AssertOnDevelopment();
        }
        public virtual void Debug(object o)
        {

            AssertOnDevelopment();
        }
        private void
            AssertOnDevelopment()
        {
            System.Diagnostics.Debug.Assert(false, "Yo0u did not set a default log");
        }

        public void AddLogWriter(ILogWriter logWriter)
        {
            throw new NotImplementedException();
        }
    }
}
