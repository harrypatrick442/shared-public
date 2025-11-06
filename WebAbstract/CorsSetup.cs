
using Logging;
using Microsoft.Extensions.DependencyInjection;
using Nodes;

namespace WebAbstract
{
    public static class CorsSetup
    {
        public static void FilesRelay(IServiceCollection services)
        {
            _FilesRelayCors(services);
        }
        public static void TransferServer(IServiceCollection services)
        {
            _TransferServerCors(services);
        }
        public static void LogServer(IServiceCollection services)
        {
            _LogServerCors(services);
        }
        public static void ClientToClientFilesRelay(IServiceCollection services)
        {
            _FilesRelayCors(services);
            _TransferServerCors(services);
            _LogServerCors(services);
        }
        public static void FileServerCors(IServiceCollection services)
        {
            _FileServerCors(services);
        }
        public static void RetrocauseQuantus(IServiceCollection services)
        {
            _Retrocause(services);
            _LogServerCors(services);
        }
        public static void RetrocauseModerator(IServiceCollection services)
        {
            _Retrocause(services);
            _LogServerCors(services);
        }
        public static void EChatServer(IServiceCollection services)
        {
            
            //The ones this has access to.
            List<string> origins = new List<string> { 
                "https://filesrelay.com",
                "https://e-chat.live",
                "https://dev.e-chat.live",
                "https://ms.e-chat.live",
                "https://e-chat.store",
                "https://e-chat.world",
                "https://echat.ltd",
                "https://objmesh.com",
                "https://log.objmesh.com"
            };
#if DEBUG
            origins.Add("http://localhost:5299");
            origins.Add("https://localhost:5401");
            origins.Add("http://localhost:3000");
#endif
            
            _Add(services, "EChatServerCors", origins.ToArray());
        }
        public static void MultimediaServer(IServiceCollection services)
        {
            //The ones this has access to.
            List<string> origins = new List<string> {
                   "https://filesrelay.com",
                   "https://android.filesrelay.com",
                   "https://ios.filesrelay.com",
                   "https://www.filesrelay.com",
                   "https://fs.filesrelay.com",
                   "https://fs2.filesrelay.com",
                   "https://ws.filesrelay.com",
                   "https://ws2.filesrelay.com",
                   "https://objmesh.com",
                   "https://ts.objmesh.com",
                   "https://ts2.objmesh.com",
                   "https://ts3.objmesh.com",
                   "https://log.objmesh.com",
                   "https://e-chat.live",
                   "https://dev.e-chat.live",
                   "https://e-chat.store",
                   "https://e-chat.world",
                   "https://echat.ltd"
               };
#if DEBUG
            origins.Add("http://localhost:5299");
            origins.Add("https://localhost:5401");
            origins.Add("http://localhost:3000");
#endif

            _Add(services, "MultimediaServerCors", origins.ToArray());
        }
        public static void Authentication(IServiceCollection services)
        {
            //The ones this has access to.
            List<string> origins = new List<string> {
                   "https://filesrelay.com",
                   "https://android.filesrelay.com",
                   "https://ios.filesrelay.com",
                   "https://www.filesrelay.com",
                   "https://fs.filesrelay.com",
                   "https://fs2.filesrelay.com",
                   "https://ws.filesrelay.com",
                   "https://ws2.filesrelay.com",
                   "https://objmesh.com",
                   "https://ts.objmesh.com",
                   "https://ts2.objmesh.com",
                   "https://ts3.objmesh.com",
                   "https://log.objmesh.com",
                   "https://e-chat.live",
                   "https://dev.e-chat.live",
                   "https://e-chat.store",
                   "https://e-chat.world",
                   "https://echat.ltd"
               };
#if DEBUG
            origins.Add("http://localhost:5299");
            origins.Add("https://localhost:5401");
            origins.Add("http://localhost:3000");
#endif

            _Add(services, "AuthenticationCors", origins.ToArray());
        }
        public static void IdServerCors(IServiceCollection services)
        {
            //The ones this has access to.
            List<string> origins = new List<string> { 
                "https://filesrelay.com",
                "https://android.filesrelay.com",
                "https://ios.filesrelay.com",
                "https://www.filesrelay.com",
                "https://fs.filesrelay.com",
                "https://fs2.filesrelay.com",
                "https://ws.filesrelay.com",
                "https://ws2.filesrelay.com",
                "https://objmesh.com",
                "https://ts.objmesh.com",
                "https://ts2.objmesh.com",
                "https://ts3.objmesh.com",
                "https://log.objmesh.com"
            };
#if DEBUG
            origins.Add("http://localhost:5299");
            origins.Add("https://localhost:5401");
            origins.Add("http://localhost:3000");
#endif
            _Add(services, "IdServerCors", origins.ToArray());
        }
        private static void _FilesRelayCors(IServiceCollection services)
        {
            //The ones this has access to.
            List<string> origins = new List<string> { 
                "https://filesrelay.com",
                "https://android.filesrelay.com",
                "https://ios.filesrelay.com",
                "https://www.filesrelay.com",
                "https://fs.filesrelay.com",
                "https://fs2.filesrelay.com",
                "https://ws.filesrelay.com",
                "https://ws2.filesrelay.com",
                "https://objmesh.com",
                "https://ts.objmesh.com",
                "https://ts2.objmesh.com",
                "https://ts3.objmesh.com",
                "https://log.objmesh.com"
            };
#if DEBUG
            origins.Add("http://localhost:5299");
            origins.Add("https://localhost:5401");
            origins.Add("http://localhost:3000");
#endif
            _Add(services, "FilesRelayCors", origins.ToArray());
        }
        private static void _FileServerCors(IServiceCollection services)
        {
            //The ones this has access to.
            List<string> origins = new List<string> { 
                "https://filesrelay.com",
                "https://android.filesrelay.com",
                "https://ios.filesrelay.com",
                "https://www.filesrelay.com",
                "https://fs.filesrelay.com",
                "https://fs2.filesrelay.com",
                "https://ws.filesrelay.com",
                "https://ws2.filesrelay.com",
                "https://objmesh.com",
                "https://ts.objmesh.com",
                "https://ts2.objmesh.com",
                "https://ts3.objmesh.com",
                "https://log.objmesh.com"
            };
#if DEBUG
            origins.Add("http://localhost:5299");
            origins.Add("https://localhost:5401");
            origins.Add("http://localhost:3000");
#endif
        }
        private static void _TransferServerCors(IServiceCollection services)
        {
            List<string> origins = new List<string> { 
                "https://filesrelay.com",
                "https://android.filesrelay.com",
                "https://ios.filesrelay.com",
                "https://www.filesrelay.com",
                "https://fs.filesrelay.com",
                "https://fs2.filesrelay.com",
                "https://ws.filesrelay.com",
                "https://ws2.filesrelay.com",
                "https://objmesh.com",
                "https://ts.objmesh.com",
                "https://ts2.objmesh.com",
                "https://ts3.objmesh.com",
                "https://log.objmesh.com"};
#if DEBUG
            origins.Add("http://localhost:5299");
            origins.Add("https://localhost:5401");
            origins.Add("http://localhost:3000");
#endif
            _Add(services, "TransferServerCors", origins.ToArray());
        }
        private static void _LogServerCors(IServiceCollection services)
        {
            List<string> origins = new List<string> { 
                "https://filesrelay.com",
                "https://android.filesrelay.com",
                "https://ios.filesrelay.com",
                "https://www.filesrelay.com",
                "https://ws.filesrelay.com",
                "https://ws2.filesrelay.com",
                "https://objmesh.com",
                "https://ts.objmesh.com",
                "https://ts2.objmesh.com",
                "https://log.objmesh.com",
                "https://ts3.objmesh.com",};
#if DEBUG
            origins.Add("http://localhost:5299/*");
            origins.Add("https://localhost:5401");
            origins.Add("http://localhost:3000");
#endif
            _Add(services, "LogServerCors", origins.ToArray()) ;
        }
        private static void _Retrocause(IServiceCollection services)
        {
            //The ones this has access to.
            List<string> origins = new List<string> {
                "https://ms.e-chat.live",
                "https://ws.e-chat.live",
                "https://objmesh.com",
                "https://log.objmesh.com"
            };
#if DEBUG
            origins.Add("http://localhost:5299");
            origins.Add("https://localhost:5401");
            origins.Add("http://localhost:3000");
#endif
        }
        private static void _Add(IServiceCollection services, string name, string[] origins)
        {
            services.AddCors(options =>
            {

                options.AddPolicy(name,
                    builder =>
                    {
                        builder.WithOrigins(origins.ToArray())
                        .AllowAnyMethod()
                        .AllowAnyHeader();
#if DEBUG
                        builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
#endif
                    });

            }
            );

        }
        private static string[] AutomaticallyDetermineOrigins() {
            var origins =  
                Nodes.Nodes.Instance.GetNodesConnectedToNode(Nodes.Nodes.Instance.MyId)
                .Select(n=>n.Id).Concat(new int[] { Nodes.Nodes.Instance.MyId })
                .SelectMany(n =>
                    new Configurations.Nodes().GetDomainsForNode(n))
                .Select(d => $"https://{d}")
#if DEBUG
                .Concat(new string[] { 
                    "http://localhost:5299",
                    "https://localhost:5401",
                    "http://localhost:3000"
                })
                
#endif
                .ToArray();
            foreach (string origin in origins) {
                Logs.Default.Info(origin);
            }
            return origins;
            
        }
    }
}