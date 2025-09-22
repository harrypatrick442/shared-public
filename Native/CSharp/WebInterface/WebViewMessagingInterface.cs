using Core.Messages.Messages;
using Core.DataMemberNames;
using Core.Events;
using Core.Interfaces;
using JSON;
using Core.Ticketing;
using Native.Events;
using Logging;
using Native.Messaging;

namespace Native.WebViewInterface
{
    public class WebViewMessagingInterface:RegistrationMessageHandlerBase
    {
        private IWebViewHandler _WebViewHandler;
        public event EventHandler<NativeRawMessageEventArgs> OnRawMessage;
        public WebViewMessagingInterface(IWebViewHandler webViewHandler)
        :base(){
            _WebViewHandler = webViewHandler;
            webViewHandler.MessageReceived += _HandleMessageFromJavaScript;
        }

        private void _HandleMessageFromJavaScript(object sender, MessageEventArgs message)
        {
            new Thread(() =>
            {
                try
                {
                    string rawMessage = message.Message;
                    if (rawMessage == null) return;
                    if (rawMessage.Length > 1 && rawMessage[0] == 'R')
                    {
                        EventHandler<NativeRawMessageEventArgs> onRawMessage = OnRawMessage;
                        if (onRawMessage != null)
                            onRawMessage.Invoke(this, new NativeRawMessageEventArgs(rawMessage));
                        return;
                    }
                    base.HandleIncomingMessage(message.Message);
                }
                catch (Exception ex)
                {
                    Logs.Default.Error(ex);
                }
            }).Start();
        }

        public override void SendRaw(string message)
        {
            _WebViewHandler.SendMessage(message);
        }
    }
}