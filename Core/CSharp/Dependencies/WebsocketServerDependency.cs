

using Snippets.WebAPI.WebsocketServers;
using System;

namespace Snippets.WebAPI.Dependencies
{
    public class WebsocketServerDependency<TWebsocketServer>: WebsocketServerDependency where TWebsocketServer: WebsocketServerBase
    {
        public WebsocketServerDependency(string path):base(typeof(TWebsocketServer), path) { 
        
        }
    }
    public abstract class WebsocketServerDependency
    {
        private Type _Type;
        public Type Type
        {
            get { return _Type; }
        }
        private string _Path;
        public string Path
        {
            get { return _Path; }
        }
        protected WebsocketServerDependency(Type type, string path)
        {
            _Type = type;
            _Path = path;
        }
    }
}
