using Enums;

namespace Logging.Writers
{
    public class LogServerWriter : ILogWriter
    {
        private ILogServerClient _LogServerClient;
        public LogServerWriter(ILogServerClient logServerClient) { 
            _LogServerClient = logServerClient; 
        }
        public void Initialize()
        {

        }
        public void Write(object o, LogLevel logLevel)
        {

            if (o == null) return;
            switch (logLevel)
            {
                case LogLevel.Fatal:
                    if (typeof(Exception).IsAssignableFrom(o.GetType()))
                    {
                        _LogServerClient.Error((Exception)o);
                        return;
                    }
                    _LogServerClient.Error(o.ToString());
                    return;
                case LogLevel.Error:

                    if (typeof(Exception).IsAssignableFrom(o.GetType())){
                        _LogServerClient.Error((Exception)o);
                        return;
                    }
                    _LogServerClient.Error(o.ToString());
                    return;
                case LogLevel.Warning:
                    _LogServerClient.Breadcrumb(Core.Enums.BreadcrumbType.Warning, "Warning", o.ToString());
                    return;
                case LogLevel.Info:
                    _LogServerClient.Breadcrumb(Core.Enums.BreadcrumbType.Info, "Info", o.ToString());
                    return;
            }
        }
        public void Clear()
        {

        }
        public void Breadcrumb(IBreadcrumb breadcrumb)
        {

        }
    }
}
