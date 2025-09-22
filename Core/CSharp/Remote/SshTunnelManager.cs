using Core.Exceptions;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Remote
{
    public class SshTunnelManager
    {/*
        private SshClient _SshClient;
        private ForwardedPortLocal _ForwardedPortLocal;
        /// <summary>
        /// The local one
        /// </summary>
        public uint BoundPort { get { return _ForwardedPortLocal.BoundPort; } }
        /// <summary>
        /// The local one
        /// </summary>
        public string BoundHost
        {
            get
            {
                return _ForwardedPortLocal.BoundHost;
            }
        }
        public SshTunnelManager(string host, string username, string password, uint port, string localHost, uint? localPort = null)
        {
            _SshClient = new SshClient(host, username, password);
            _SshClient.Connect();
            _ForwardedPortLocal = new ForwardedPortLocal(localHost, host, port);
            _SshClient.AddForwardedPort(_ForwardedPortLocal);
            if (!_SshClient.IsConnected)
            {
                throw new OperationFailedException("Failed to connect");
            }
            _ForwardedPortLocal.Exception += (object sender, ExceptionEventArgs e) =>
            {
                Console.WriteLine(e.Exception.ToString());
            };
            _ForwardedPortLocal.Start();
        }
        public void Dispose()
        {
            try
            {
                _ForwardedPortLocal?.Stop();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
            try
            {
                _SshClient.Disconnect();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
            try
            {
                _SshClient.Dispose();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
        }
        */
    }
}
