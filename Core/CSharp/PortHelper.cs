// MIT License - Copyright (c) 2016 Can Güney Aksakalli
// https://aksakalli.github.io/2014/02/24/simple-http-server-with-csparp.html

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Snippets
{
    public class PortHelper
    {
        public static int GetEmptyPort()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);
            int port;
            tcpListener.Start();
            try
            {
                port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            }
            finally
            {
                tcpListener.Stop();
            }
            return port;
        }
    }
}