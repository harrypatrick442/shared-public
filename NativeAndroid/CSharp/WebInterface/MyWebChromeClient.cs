using Android.App;
using Android.Content;
using Android.Webkit;
using Logging;
using NativeAndroid.Interfaces;
using System;

namespace NativeAndroid
{
    public class MyWebChromeClient<TActivity> : WebChromeClient where TActivity:Activity, IActivityResultSource
    {
        private const int FILECHOOSER_RESULTCODE = 1;
        private TActivity _Activity;
        public MyWebChromeClient(TActivity context) {
            _Activity = context;
        }
        public override void OnProgressChanged(WebView? view, int progress)
        {
            base.OnProgressChanged(view, progress);
        }
        //[Android.Runtime.Register("onShowFileChooser", "(Landroid/webkit/WebView;Landroid/webkit/ValueCallback;Landroid/webkit/WebChromeClient$FileChooserParams;)Z", "GetOnShowFileChooser_Landroid_webkit_WebView_Landroid_webkit_ValueCallback_Landroid_webkit_WebChromeClient_FileChooserParams_Handler")]
        public override Boolean OnShowFileChooser(WebView webView,
            IValueCallback filePathCallback, WebChromeClient.FileChooserParams fileChooserParams)
        {
            try
            {
                Intent intent = fileChooserParams.CreateIntent();
                intent.SetType("*/*");
                intent.AddCategory(Intent.CategoryOpenable);
                _Activity.AddHandler(FILECHOOSER_RESULTCODE , (requestCode, resultCode, data) => {
                    filePathCallback.OnReceiveValue(
                        WebChromeClient.FileChooserParams.ParseResult(
                            Convert.ToInt32(resultCode), data
                        )
                    );
                });
                _Activity.StartActivityForResult(
                    Intent.CreateChooser(intent, "File Chooser"),
                    FILECHOOSER_RESULTCODE);
                return true;

            }
            catch (Exception ex)
            {
                Logs.Default.Error(ex);
            }
            return true;
        }
    }
}