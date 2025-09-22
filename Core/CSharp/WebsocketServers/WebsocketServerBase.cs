using JSON;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Net;
using Logging;

namespace Snippets.WebAPI.WebsocketServers
{
    public abstract class WebsocketServerBase : WebSocketBehavior
    {
        protected IPAddress _ClientIPAddress;
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
        }
        protected override void OnOpen()
        {
            _ClientIPAddress = Context?.UserEndPoint?.Address;
            base.OnOpen();
        }
        public virtual void SendJSONString(string jsonString)
        {
            //Logs.Default?.Info("SEND :" + jsonString);
            base.Send(jsonString);
        }
        private void Close()
        {
            try
            {
                Context?.WebSocket?.Close();
            }
            catch { }
        }
        /*
        protected void SendTicketedResponse<TResponsePayload>(TResponsePayload response) where TResponsePayload: TicketedResponseMessageBase
        {
            Send(response);
        }*/
        public virtual void Dispose() {
            Close();
        }
    }
}