using Core.Exceptions;
using Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Shutdown
{
    public class Firewall:IShutdownable
    {
        private const int 
            TIMEOUT_ALLOW_PORT = 3000, 
            TIMEOUT_DENY_PORT=3000;
        private readonly object _LockObjectDispose = new object();
        private bool _Disposed = false;
        HashSet<int> _PortsOpened = new HashSet<int>();
        public ShutdownOrder ShutdownOrder => ShutdownOrder.ClosePorts;
        private static Firewall _Instance;
        protected Firewall()
        {
            ShutdownManager.Instance.Add(this);
        }
        public static Firewall Initialize()
        {
            if (_Instance != null) return _Instance;
            _Instance = new Firewall();
            return _Instance;
        }
        public static Firewall Instance
        {
            get
            {
                if (_Instance == null) throw new NotInitializedException(nameof(Firewall));
                return _Instance;
            }
        }


        public void OpenPortsUntilShutdown(params int[] ports)
        {
            Logs.Default.Info($"Opening ports [{ string.Join(',', ports)}]");
            lock (_LockObjectDispose)
            {
                if (_Disposed) return;
            }
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;
            Logs.Default.Info("Was linux so proceeding to open ports");
            foreach (int port in ports)
            {
                if (port == 22) continue;
                try {
                    lock (_PortsOpened)
                    {
                        if (!_PortsOpened.Contains(port))
                            _PortsOpened.Add(port);
                    }
                    AllowPort(port);
                }
                catch (Exception ex)
                {
                    Logs.Default.Error(ex);
                }
            }
        }
        private static void AllowPort(int port)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            { FileName = "/bin/bash", Arguments = $"-c \"sudo ufw allow {port}\"", };
            Process proc = new Process() { StartInfo = startInfo, };
            proc.Start();
            proc.WaitForExit(TIMEOUT_ALLOW_PORT);
        }
        private static void DenyPort(int port)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            { FileName = "/bin/bash", Arguments = $"-c \"sudo ufw deny {port}\"", };
            Process proc = new Process() { StartInfo = startInfo, };
            proc.Start();
            proc.WaitForExit(TIMEOUT_DENY_PORT);
        }

        public void Dispose()
        {
            lock (_LockObjectDispose)
            {
                if (_Disposed) return;
                _Disposed = true;
            }
            Logs.Default.Info($"Closing ports {string.Join(',', _PortsOpened)}");
            lock (_PortsOpened) {
                foreach (int port in _PortsOpened)
                {
                    try
                    {
                        DenyPort(port);
                    }
                    catch (Exception ex) {
                        Logs.Default.Error(ex);
                    }
                }
            }
        }
    }
}