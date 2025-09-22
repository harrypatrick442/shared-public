namespace Logging
{
    public interface ILog
    {
        void Breadcrumb(IBreadcrumb breadcrumb);
        void Clear();
        void Debug(object o);
        void Error(object o);
        TLogWriter GetLogWriter<TLogWriter>() where TLogWriter : ILogWriter;
        void Info(object o);
        void Warn(object o);
        void AddLogWriter(ILogWriter logWriter);
    }
}
