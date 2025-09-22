using Android.Runtime;
using Android.Webkit;
using Java.Interop;
using Logging;
using System;

namespace NativeAndroid
{
    public class JavascriptInterface:Java.Lang.Object
    {
        private Action<string> _CallbackReceivedMessage;
        public JavascriptInterface(Action<string> callbackReceivedMessage)
        {
            _CallbackReceivedMessage = callbackReceivedMessage;
        }
        [JavascriptInterface]
        [Export("send")]
        public void Send(String message)
        {
            _CallbackReceivedMessage(message);
        }

        [JavascriptInterface]
        [Export("getLogSessionId")]
        public long GetLogSessionId()
        {
            return LogServerClient.Instance.SessionId;
        }
    }
}