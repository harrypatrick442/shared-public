using Android.App;
using Android.Webkit;
using Core.Events;
using Native.WebViewInterface;
using NativeAndroid.Interfaces;
using System;
using System.Reflection.Metadata;

namespace NativeAndroid
{
    public class AndroidWebViewHandler<TActivity> : IWebViewHandler where TActivity:Activity, IActivityResultSource
    {
        public event EventHandler<MessageEventArgs>? MessageReceived;
        public event EventHandler<EventArgs>? FinishedLoading;
        private WebView _WebView;
        private Activity _Activity;
        public AndroidWebViewHandler(TActivity activity, WebView webView)
        {
            _WebView = webView;
            _Activity = activity;
            WebSettings webSettings = _WebView.Settings;
            webSettings.JavaScriptEnabled = true;
            webSettings.AllowFileAccess = true;
            //webSettings.AllowFileAccessFromFileURLs =true;
            //webSettings.AllowUniversalAccessFromFileURLs =true;
            webView.SetWebChromeClient(new MyWebChromeClient<TActivity>(activity));
            webView.AddJavascriptInterface(new JavascriptInterface(ReceivedMessage), "NativeToJavaScriptInterface");
        }
        public void SendMessage(string message)
        {
            string methodParams = message == null ? string.Empty : message;
            _Activity.RunOnUiThread(() => {
                _WebView.EvaluateJavascript($"_incomingNative({methodParams})", null);
            });
        }
        private void ReceivedMessage(string message)
        {
            MessageReceived?.Invoke(this, new MessageEventArgs(message));
        }
    }
}