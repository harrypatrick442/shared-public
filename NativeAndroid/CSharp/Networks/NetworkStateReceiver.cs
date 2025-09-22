
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Core.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace NativeAndroid.Networks
{
    public sealed class NetworkStateReceiver : BroadcastReceiver
    {
        private static NetworkStateReceiver _Instance;
        public static NetworkStateReceiver Initialize() {
            if (_Instance != null) throw new AlreadyInitializedException(nameof(NetworkStateReceiver));
            _Instance = new NetworkStateReceiver();
            return _Instance;
        }
        public static NetworkStateReceiver Instance
        {
            get
            {
                if (_Instance == null)
                    throw new NotInitializedException(nameof(NetworkStateReceiver));
                return _Instance;
            }
        }
        private HashSet<INetworkStateListener> _Listeners = new HashSet<INetworkStateListener>();
        private bool? _Connected = null;

        private NetworkStateReceiver()
        {

        }

        private void NotifyStateToAll()
        {
            foreach(INetworkStateListener listener in _Listeners)
                NotifyState(listener);
        }

        private void NotifyState(INetworkStateListener listener)
        {
            if (_Connected == null || listener == null)
                return;
            if (_Connected == true)
                listener.NetworkAvailable();
            else
                listener.NetworkUnavailable();
        }

        public void AddListener(INetworkStateListener l)
        {
            if (_Listeners.Contains(l)) 
                return;
            _Listeners.Add(l);
            NotifyState(l);
        }

        public void RemoveListener(INetworkStateListener l)
        {
            _Listeners.Remove(l);
        }

        public override void OnReceive(Context context, Intent intent)
        {

            _Connected = _GetConnected(context);
            NotifyStateToAll();
        }
        private bool _GetConnected(Context context) {
            ConnectivityManager connectivityManager = (ConnectivityManager)context
                    .GetSystemService(Context.ConnectivityService);
            if (connectivityManager == null)
                return false;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                Network activeNetwork = connectivityManager.ActiveNetwork;
                if(activeNetwork == null) return false; 
                NetworkCapabilities networkCapabilities = connectivityManager.GetNetworkCapabilities(activeNetwork);
                    return networkCapabilities != null;
            }
            NetworkInfo? activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
            if (activeNetworkInfo == null)
                return false;
            if (activeNetworkInfo.IsConnectedOrConnecting == true && activeNetworkInfo.IsAvailable)
            {
                return true;
            }
            return false;
            /*
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Network[] networks = connectivityManager.GetAllNetworks();
                return networks.Select(network=>connectivityManager.GetNetworkInfo(network))
                        .Where(networkInfo=>networkInfo.GetState().Equals(NetworkInfo.State.Connected)).Any();
            }
            //noinspection deprecation
            NetworkInfo[] networkInfos = connectivityManager.GetAllNetworkInfo();
            if (networkInfos == null) return false;
            return networkInfos.Where(i => i.GetState() == NetworkInfo.State.Connected).Any();*/
        }

    }
}