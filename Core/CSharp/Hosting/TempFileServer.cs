using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Snippets;

namespace Core.Hosting
{// MIT License - Copyright (c) 2016 Can Güney Aksakalli
 // https://aksakalli.github.io/2014/02/24/simple-http-server-with-csparp.html


    public class TempFileServer:IDisposable
    {
        private volatile bool _Disposed = false;
        private object _LockObjectDispose = new object();
        private SimpleHTTPServer _SimpleHttpServer;
        private string _DirectoryPath;
        public TempFileServer() {
            _DirectoryPath = GetNewTempDirectory();
            _SimpleHttpServer = new SimpleHTTPServer(_DirectoryPath);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>Url to file</returns>
        public string AddFile(string filePath) {
            CheckNotDisposed();
            string tempFilePath = GetNewTempFilePath(Path.GetExtension(filePath));
            File.Copy(filePath, tempFilePath);
            return GetFileUrl(tempFilePath);
        }
        private void CheckNotDisposed()
        {
            if (_Disposed) throw new ObjectDisposedException(nameof(TempFileServer));
        }
        private string GetFileUrl(string filePath) {

            return $"http://localhost:{_SimpleHttpServer.Port}/{Path.GetFileName(filePath)}";
        }
        public string WriteToFile(string content, string extension)
        {
            string tempFilePath = GetNewTempFilePath(extension);
            File.WriteAllText(tempFilePath, content);
            return GetFileUrl(tempFilePath);
        }
        private string GetNewTempFilePath(string extension) {
            string filePath;
            do
            {
                string fileName = Guid.NewGuid().ToString("D") + extension;
                filePath = Path.Combine( _DirectoryPath, fileName);
            } while (File.Exists(filePath));
            return filePath;
        }
        private static string GetNewTempDirectory() {
            string tempDirectoryPath;
            do
            {
                tempDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("D"));
            } while (Directory.Exists(tempDirectoryPath));
            Directory.CreateDirectory(tempDirectoryPath);
            return tempDirectoryPath;
        }
        public void Dispose() {
            lock (_LockObjectDispose)
            {
                if (_Disposed) return;
                _SimpleHttpServer.Dispose();
              
                try
                {
                    Directory.Delete(_DirectoryPath, true);
                }
                catch{ }
                _Disposed = true;
            }
        }
    }
}
