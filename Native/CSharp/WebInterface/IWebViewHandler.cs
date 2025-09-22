using Core.Events;
namespace Native.WebViewInterface
{
    public interface IWebViewHandler
    {
        public event EventHandler<MessageEventArgs> MessageReceived;
        public void SendMessage(string message);
    }
}