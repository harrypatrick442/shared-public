using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading;
using Core.Exceptions;
using Logging;
using Core;

namespace Snippets
{
    public class SimpleHTTPServer : IDisposable
    {
        private object _LockObjectDispose = new object();
        private CancellationTokenSource _CancellationTokenSourceDisposed = new CancellationTokenSource();
        private readonly string[] _IndexFiles = {
            "index.html",
            "index.htm",
            "default.html",
            "default.htm"
        };

        private static IDictionary<string, string> _MimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
            #region extension to MIME type list
            {".asf", "video/x-ms-asf"},
            {".asx", "video/x-ms-asf"},
            {".avi", "video/x-msvideo"},
            {".bin", "application/octet-stream"},
            {".cco", "application/x-cocoa"},
            {".crt", "application/x-x509-ca-cert"},
            {".css", "text/css"},
            {".deb", "application/octet-stream"},
            {".der", "application/x-x509-ca-cert"},
            {".dll", "application/octet-stream"},
            {".dmg", "application/octet-stream"},
            {".ear", "application/java-archive"},
            {".eot", "application/octet-stream"},
            {".exe", "application/octet-stream"},
            {".flv", "video/x-flv"},
            {".gif", "image/gif"},
            {".hqx", "application/mac-binhex40"},
            {".htc", "text/x-component"},
            {".htm", "text/html"},
            {".html", "text/html"},
            {".ico", "image/x-icon"},
            {".img", "application/octet-stream"},
            {".iso", "application/octet-stream"},
            {".jar", "application/java-archive"},
            {".jardiff", "application/x-java-archive-diff"},
            {".jng", "image/x-jng"},
            {".jnlp", "application/x-java-jnlp-file"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".js", "application/x-javascript"},
            {".mml", "text/mathml"},
            {".mng", "video/x-mng"},
            {".mov", "video/quicktime"},
            {".mp3", "audio/mpeg"},
            {".mpeg", "video/mpeg"},
            {".mpg", "video/mpeg"},
            {".msi", "application/octet-stream"},
            {".msm", "application/octet-stream"},
            {".msp", "application/octet-stream"},
            {".pdb", "application/x-pilot"},
            {".pdf", "application/pdf"},
            {".pem", "application/x-x509-ca-cert"},
            {".pl", "application/x-perl"},
            {".pm", "application/x-perl"},
            {".png", "image/png"},
            {".prc", "application/x-pilot"},
            {".ra", "audio/x-realaudio"},
            {".rar", "application/x-rar-compressed"},
            {".rpm", "application/x-redhat-package-manager"},
            {".rss", "text/xml"},
            {".run", "application/x-makeself"},
            {".sea", "application/x-sea"},
            {".shtml", "text/html"},
            {".sit", "application/x-stuffit"},
            {".swf", "application/x-shockwave-flash"},
            {".tcl", "application/x-tcl"},
            {".tk", "application/x-tcl"},
            {".txt", "text/plain"},
            {".war", "application/java-archive"},
            {".wbmp", "image/vnd.wap.wbmp"},
            {".wmv", "video/x-ms-wmv"},
            {".xml", "text/xml"},
            {".xpi", "application/x-xpinstall"},
            {".zip", "application/zip"},
            #endregion
        };
        private Thread _ServerThread;
        private string _RootDirectory;
        private bool _AllowCors;
        private int _Port;
        private Action<Exception> _HandleException;

        public int Port { get { return _Port; } }
        public string RootDirectory { get { return _RootDirectory; } }
        public SimpleHTTPServer(string directoryPath, int port, bool allowCors = true, Action<Exception> handleException = null)
        {
            Initialize(directoryPath, port, allowCors, handleException);
        }
        public SimpleHTTPServer(string directoryPath, bool allowCors = true, Action<Exception> handleException = null)
        {
            Initialize(directoryPath, PortHelper.GetEmptyPort(), allowCors, handleException);
        }
        private void Initialize(string path, int port, bool allowCors, Action<Exception> handleException)
        {
            _RootDirectory = path;
            _Port = port;
            _AllowCors = allowCors;
            _HandleException = handleException;
            CountdownLatch countdownLatchListeningOrFailed = new CountdownLatch();
            new Thread(() => Listen(countdownLatchListeningOrFailed)).Start();
            countdownLatchListeningOrFailed.Wait();
        }
        ~SimpleHTTPServer()
        {
            Dispose();
        }
        public void Dispose()
        {
            lock (_LockObjectDispose)
            {
                if (_CancellationTokenSourceDisposed.IsCancellationRequested) return;
                _CancellationTokenSourceDisposed.Cancel();
            }
        }

        private void Listen(CountdownLatch countdownLatchListeningOrFailed)
        {
            try
            {
                using (HttpListener httpListener = new HttpListener())
                {
                    using (CancellationTokenRegistration cancellationTokenRegistration =
                        _CancellationTokenSourceDisposed.Token.Register(httpListener.Abort))
                    {
                        httpListener.Prefixes.Add($"http://*:{_Port.ToString()}/");
                        httpListener.Start();
                        countdownLatchListeningOrFailed.Signal();
                        while (!_CancellationTokenSourceDisposed.IsCancellationRequested)
                        {
                            try
                            {
                                IAsyncResult result = httpListener.BeginGetContext(ListenerCallback, httpListener);
                                result.AsyncWaitHandle.WaitOne();
                            }
                            catch (Exception ex)
                            {
                                Logs.Default.Error(ex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                countdownLatchListeningOrFailed.Signal();
                Logs.Default.Error(ex);
            }
        }
        public void ListenerCallback(IAsyncResult result)
        {
            if (_CancellationTokenSourceDisposed.IsCancellationRequested)
            {
                return;
            }
            try
            {
                HttpListener listener = (HttpListener)result.AsyncState;
                HttpListenerContext context = listener.EndGetContext(result);
                Process(context);
            }
            catch (ObjectDisposedException ex) {
                Logs.Default.Error(ex);
            }
        }
        private void Process(HttpListenerContext httpListenerContext)
        {
            string fileName = null;
            try
            {
                fileName = GetRequestedFileName(httpListenerContext.Request);
                string filePath = fileName == null ? null : Path.Combine(_RootDirectory, fileName);
                if (filePath == null)
                {
                    httpListenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }
                if (!File.Exists(filePath))
                {
                    httpListenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }
                ReturnFile(filePath, httpListenerContext);
            }
            catch (Exception ex)
            {
                Logs.Default.Error(ex);
                httpListenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                string errorMessage = $"Value processing request";
                if (fileName != null) errorMessage += $" for file \"{fileName}\"";
                throw new OperationFailedException(errorMessage, ex);
            }
            finally
            {
                httpListenerContext.Response.OutputStream.Close();
            }
        }
        private void ReturnFile(string filePath, HttpListenerContext httpListenerContext)
        {

            using (Stream input = new FileStream(filePath, FileMode.Open))
            {
                HttpListenerResponse httpListenerResponse = httpListenerContext.Response;
                httpListenerResponse.ContentType = GetContentType(filePath);
                httpListenerResponse.ContentLength64 = input.Length;
                httpListenerResponse.AddHeader("Date", DateTime.UtcNow.ToString("r"));
                httpListenerResponse.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filePath).ToString("r"));
                if (_AllowCors)
                    AddCorsHeaders(httpListenerResponse);
                WriteInputStreamToResponse(input, httpListenerResponse.OutputStream);
                httpListenerResponse.StatusCode = (int)HttpStatusCode.OK;
                httpListenerResponse.OutputStream.Flush();
            }
        }
        private void AddCorsHeaders(HttpListenerResponse httpListenerResponse)
        {
            httpListenerResponse.AddHeader("Access-Control-Allow-Origin", "*");
        }
        private string GetRequestedFileName(HttpListenerRequest httpListenerRequest)
        {

            string fileName = httpListenerRequest.Url.AbsolutePath.Substring(1);
            if (string.IsNullOrEmpty(fileName))
                fileName = GetExistingIndexFileName();
            return fileName;
        }
        private void WriteInputStreamToResponse(Stream inputStream, Stream outputStream)
        {

            byte[] buffer = new byte[1024 * 16];
            int nbytes;
            while ((nbytes = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                outputStream.Write(buffer, 0, nbytes);
        }
        private string GetContentType(string filePath)
        {
            string mime;
            if (_MimeTypeMappings.TryGetValue(Path.GetExtension(filePath), out mime))
                return mime;
            return "application/octet-stream";
        }
        private string GetExistingIndexFileName()
        {
            foreach (string indexFile in _IndexFiles)
            {
                if (File.Exists(Path.Combine(_RootDirectory, indexFile)))
                {
                    return indexFile;
                }
            }
            return null;
        }
    }
}