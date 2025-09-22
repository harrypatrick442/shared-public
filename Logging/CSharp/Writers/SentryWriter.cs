using Enums;
//using Sentry;
namespace Logging.Writers
{
    /*
    public class SentryWriter : ILogWriter
    {
        private static Sentry.SentryClient _SentryClient;
        public SentryWriter(string dsn)
        {
            SentrySdk.Init(dsn);
            _SentryClient = new Sentry.SentryClient(new SentryOptions() { Dsn = dsn });
        }
        public virtual void Initialize()
        {

        }
        public void SetDsn(string dsn)
        {
            _SentryClient = new Sentry.SentryClient(new SentryOptions() { Dsn = dsn });
        }
        public void Write(object o, LogLevel logLevel)
        {
            if (o == null) return;
            switch (logLevel)
            {
                case LogLevel.Debug:
                    _SentryClient.CaptureMessage(o.ToString(), SentryLevel.Debug);
                    break;
                case LogLevel.Warning:
                    _SentryClient.CaptureMessage(o.ToString(), SentryLevel.Warning);
                    break;
                case LogLevel.Info:
                    _SentryClient.CaptureMessage(o.ToString(), SentryLevel.Info);
                    break;
                case LogLevel.Fatal:
                    if (typeof(Exception).IsAssignableFrom(o.GetType()))
                        _SentryClient.CaptureException((Exception)o);
                    _SentryClient.CaptureMessage(o.ToString(), SentryLevel.Fatal);
                    break;
                case LogLevel.Error:
                    try
                    {
                        if (typeof(Exception).IsAssignableFrom(o.GetType()))
                            _SentryClient.CaptureException((Exception)o);
                        else
                            _SentryClient.CaptureMessage(o.ToString(), SentryLevel.Error);
                    }
                    catch (Exception ex)
                    {

#if DEVELOPMENT

                        throw ex;
#endif
#if DEBUG

                        throw ex;
#endif
                    }
                    break;
            }
        }
        public void Clear()
        {

        }

        public void Breadcrumb(IBreadcrumb breadcrumb)
        {

        }
    }*/
}
