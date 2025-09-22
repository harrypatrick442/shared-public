using Core.Events;
using Native.WebViewInterface;
using System;
using System.Reflection.Metadata;
using static System.Runtime.InteropServices.JavaScript.JSType;
using WebKit;

namespace NativeIOS
{
    public class IOSWebViewHandler: WKScriptMessageHandler {
        public event EventHandler<MessageEventArgs>? MessageReceived;
        public event EventHandler<EventArgs>? FinishedLoading;
        private Func<WKWebView> _GetWKWebView;
        private UIView _UIView;
        public override void DidReceiveScriptMessage(
            WKUserContentController userContentController, 
            WKScriptMessage wkScriptMessage)
        {
            string message = (string)wkScriptMessage.Body.ToString();
            if (message == null) return;
            ReceivedMessage(message);
        }
        public IOSWebViewHandler(UIView uiView, Func<WKWebView> getWebView):base()
        {
            _UIView = uiView;
            _GetWKWebView = getWebView;
        }
        public void SendMessage(string message)
        {
            string methodParams = message == null ? string.Empty : message;
            _UIView.InvokeOnMainThread(() => {
                _GetWKWebView().EvaluateJavaScript($"_incomingNative({methodParams})", null);
            });
        }
        private void ReceivedMessage(string message)
        {
            MessageReceived?.Invoke(this, new MessageEventArgs(message));
        }
    }
}