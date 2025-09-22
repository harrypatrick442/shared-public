using Core.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Core.FileSystem {

    public class TemporaryFilesCollection<TDerivedTemporaryFile> : TemporaryFilesCollection where TDerivedTemporaryFile : TemporaryFile
    {
        public void Add(TemporaryFilesCollection<TDerivedTemporaryFile> temporaryFilesCollection)
        {
            base.Add(temporaryFilesCollection);
        }
        public void Add(TDerivedTemporaryFile temporaryFile)
        {
            base.Add(temporaryFile);
        }
        public new TDerivedTemporaryFile[] ToArray()
        {
            return base.ToArray().Cast<TDerivedTemporaryFile>().ToArray();
        }
        public TemporaryFilesCollection(params TemporaryFilesCollection<TDerivedTemporaryFile>[] temporaryFileCollections):base(temporaryFileCollections.Cast<TemporaryFilesCollection>().ToArray())
        {

        }
    }
    public class TemporaryFilesCollection : IDisposable
    {
        private object _LockObjectDispose = new object();
        private bool _Disposed = false;
        protected List<TemporaryFile> _TemporaryFiles = new List<TemporaryFile>();
        protected List<TemporaryFilesCollection> _ChildCollections = new List<TemporaryFilesCollection>();
        public TemporaryFilesCollection(params TemporaryFilesCollection[] temporaryFileCollections) {
            _ChildCollections = temporaryFileCollections.ToList();
        }
        private void CheckNotDisposed()
        {
            if (_Disposed) throw new ObjectDisposedException(nameof(TemporaryFilesCollection));
        }
        public void Add(TemporaryFile temporaryFile)
        {
            CheckNotDisposed();
            if (temporaryFile == null) throw new ArgumentNullException($"{nameof(temporaryFile)} cannot be null");
            lock (_TemporaryFiles)
            {
                if (_TemporaryFiles.Contains(temporaryFile)) return;
                _TemporaryFiles.Add(temporaryFile);
            }
        }
        public void Add(TemporaryFilesCollection temporaryFilesCollection)
        {
            CheckNotDisposed();
            if (temporaryFilesCollection == null) throw new ArgumentNullException($"{nameof(temporaryFilesCollection)} cannot be null");
            lock (_ChildCollections)
            {
                _ChildCollections.Add(temporaryFilesCollection);
            }
        }
        ~TemporaryFilesCollection() {
            Dispose();
        }
        public TemporaryFile[] ToArray()
        {
            lock (_TemporaryFiles)
            {
                lock (_ChildCollections)
                {
                    return _TemporaryFiles.Concat(
                        _ChildCollections.SelectMany(childCollection => childCollection.ToArray())
                    ).ToArray();
                }
            }
        }
        public void Dispose() {
            lock (_LockObjectDispose) {
                if (_Disposed) return;
                _Disposed = true;
                lock (_TemporaryFiles)
                {
                    _TemporaryFiles.ForEach(temporaryFile => temporaryFile.Dispose());
                    _ChildCollections.ForEach(temporaryFilesCollection => temporaryFilesCollection.Dispose());
                }
            }
        }
    }
}
