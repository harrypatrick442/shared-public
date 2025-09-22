
using Android.App;
using Android.Content;
using Android.Net;
using System.Collections.Generic;

namespace NativeAndroid.Networks
{
    public interface INetworkStateListener
    {
        public void NetworkAvailable();
        public void NetworkUnavailable();
    }
}