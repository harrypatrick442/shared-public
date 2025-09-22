
using Logging.Writers;
using System.Collections.Generic;

namespace Logging
{
    public class Logs
    {
        
        public static ILog HighPriority { get => Default; }
        protected static ILog _Default;
        public static ILog Default { get { return _Default; } }
        public static void Initialize(string logFilePath/*, ILogServerClient logServerClient*/
            )
        {
            System.Diagnostics.Debug.WriteLine(logFilePath);
            List<ILogWriter> writers = new List<ILogWriter> { };
            /*
            if (logServerClient != null)    
                writers.Add(new LogServerWriter(logServerClient));*/
            if (logFilePath != null)
                writers.Add(new FileWriter(logFilePath));
#if DEBUG
                writers.Add(new ConsoleWriter());
#endif
            _Default = new Log(writers.ToArray());
            //ILog noLog = new NoLog();

            writers = new List<ILogWriter> {};
            if (logFilePath != null)
                writers.Add(new FileWriter(logFilePath));
#if DEBUG
            writers.Add(new ConsoleWriter());
#endif
            LogggingLogger = new Log(writers.ToArray());

            ConsoleOnly = new Log(new ConsoleWriter());

            //MachineMetrics = Default;
            //LoadBalancer = Default;
            //Firewall = Default;
            /*WebSocketsDebugging = webSocketsDebugging ?
                Default : noLog;
            AuthenticationDebugging = authenticationDebugging ?
                Default : noLog;
            UserRoutingTableDebugging = userRoutingTableDebugging ?
                Default : noLog;
            AssociateUpdate = GlobalConstants.LogsEnabled.AssociateUpdate ? Default : noLog;*/

        }
        public static void Add(ILogServerClient logServerClient) {
            _Default.AddLogWriter(new LogServerWriter(logServerClient));
        }
        public static ILog ConsoleOnly { get; private set; }
        public static ILog LogggingLogger { get; private set; }
        //public static ILog WebSocketsDebugging { get; private set; }
        //public static ILog AssociateUpdate { get; private set; }
        //public static ILog MachineMetrics { get; private set; }
        //public static ILog LoadBalancer { get; private set; }
        //public static ILog Firewall { get; private set; }
       // public static ILog AuthenticationDebugging { get; private set; }
        //public static ILog UserRoutingTableDebugging { get; private set; }
        /*
        private static Log _Unity;
        public static Log Unity
        {
            get
            {
                if (_Unity == null)
                    _Unity = new LogWithBreadcrumbDiversion(true, new ConsoleWriter(),
                        new FileWriter("C:\\exporterforinteractivesiteplanv1_logs\\exporter.log")
                        //new CoreSentryWriter(SentryDsns.UISentryDSN),
                        );
                return _Unity;
            }
        }*/
        }
}
