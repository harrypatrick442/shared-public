using Enums;

namespace Logging
{
    public interface ILogWriter
    {
        void Initialize();
        void Write(object o, LogLevel logLevel);
        void Breadcrumb(IBreadcrumb breadcrumb);
        void Clear();
    }
}
