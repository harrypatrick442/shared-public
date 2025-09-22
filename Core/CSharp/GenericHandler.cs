using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JSON;
using Core.Interfaces;
using Shutdown;
using Core.Exceptions;
namespace Core
{
    public class GenericHandler<TRequest, TResponse>:IShutdownable
    {
        private static readonly Json _NativeJsonParser = new Json();
        private object _LockObjectDispose = new object();
        private CancellationTokenSource _CancellationTokenSourceDisposed = new CancellationTokenSource();
        private Thread _ServerThread;
        private int _Port;
        private Action<Exception> _HandleException;
        public delegate TResponse DelegateProcessRequest(TRequest request);
        private DelegateProcessRequest _Callback;

        public int Port {  get { return _Port; } }

        public ShutdownOrder ShutdownOrder => ShutdownOrder.GenericHandler;

        public GenericHandler(int port, DelegateProcessRequest callback, Action<Exception> handleException = null)
        {
            _Callback = callback;
            Initialize(port, handleException);
        }
        private void Initialize(int port, Action<Exception> handleException)
        {
            _Port = port;
            _HandleException = handleException;
            _ServerThread = new Thread(Listen);
            _ServerThread.Start();
        }
        public void Dispose()
        {
            lock (_LockObjectDispose)
            {
                if (_CancellationTokenSourceDisposed.IsCancellationRequested) return;
                _CancellationTokenSourceDisposed.Cancel();
            }
        }

        private void Listen()
        {
            using (HttpListener httpListener = new HttpListener())
            {
                httpListener.Prefixes.Add(GetUrl());
                httpListener.Start();
                using (CancellationTokenRegistration cancellationTokenRegistration = _CancellationTokenSourceDisposed.Token.Register(
                    httpListener.Abort))
                {
                    while (!_CancellationTokenSourceDisposed.IsCancellationRequested)
                    {
                        try
                        {
                            //Using this method because the GetContext method will not exit cleanly even when Stop or Abort are called.
                            Task<HttpListenerContext> task = httpListener.GetContextAsync();
                            task.Wait(_CancellationTokenSourceDisposed.Token);
                            new Thread(() =>
                            {
                                Process(task.Result);
                            }).Start();
                        }
                        catch(Exception ex) {
                            _HandleException?.Invoke(ex);
                        }
                    }
                }
            }
        }
        private string GetUrl() {
            return $"http://*:{ _Port.ToString() }/";
        }
        private void Process(HttpListenerContext httpListenerContext)
        {
            HttpListenerResponse httpListenerResponse = httpListenerContext.Response;
            try
            {
                TRequest request = GetArgumentsFromRequest(httpListenerContext.Request);
                WriteResponse(_Callback(request), httpListenerResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                httpListenerResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                _HandleException?.Invoke(ex);
            }
            finally
            {
                httpListenerResponse.OutputStream.Close();
            }
        }
        private TRequest GetArgumentsFromRequest(HttpListenerRequest httpListenerRequest)
        {
            using (StreamReader streamReader = new StreamReader(httpListenerRequest.InputStream))
            {
                return Json.Deserialize<TRequest>(streamReader.ReadToEnd());
            }
        }
        private static void WriteResponse(TResponse response, HttpListenerResponse httpListenerResponse)
        {
            Stream outputStream = httpListenerResponse.OutputStream;
            using (StreamWriter streamWriter = new StreamWriter(outputStream, System.Text.Encoding.UTF8, bufferSize:8*1024,leaveOpen: true))
                streamWriter.Write(Json.Serialize(response));
            outputStream.Flush();
        }
    }
}