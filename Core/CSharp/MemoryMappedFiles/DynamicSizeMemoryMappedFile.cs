using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Core.Parsing;
using Logging;

namespace Core.MemoryMappedFiles
{
    public class DynamicSizeMemoryMappedFile<TContent>:IDisposable where TContent:class{
        private volatile bool _Disposed = false;
        private object _LockObjectDispose = new object();
        private MemoryMapped<DynamicSizeMetadata> _MemoryMappedDynamicSizeMetadata;
        private List<MemoryMapped> _CurrentMemoryMappedChunks = new List<MemoryMapped>();
        private string _Directory;
        private string _FileNamePrefix;
        private int _ChunkSize;
        private int _AcquireExclusiveAccessTimeoutMilliseconds;
        private string _MetadataFilePath;
        public string MetadataFilePath { get { return _MetadataFilePath; } }
        private Mutex _Mutex;
        private IParser<TContent, byte[]> _Parser;
        public DynamicSizeMemoryMappedFile(int chunkSize, string directory, string fileNamePrefix, IParser<TContent, byte[]> parser, int acquireExclusiveAccessTimeoutMilliseconds=1000) {
            _ChunkSize = chunkSize;
            _Directory = directory;
            _FileNamePrefix = fileNamePrefix;
            _Parser = parser;
            _AcquireExclusiveAccessTimeoutMilliseconds = acquireExclusiveAccessTimeoutMilliseconds;
            _MetadataFilePath = GetMetadataFilePath(_Directory, _FileNamePrefix);
            _Mutex = new Mutex(false, GetMutexName(_MetadataFilePath));
            _MemoryMappedDynamicSizeMetadata = new MemoryMapped<DynamicSizeMetadata>(_MetadataFilePath);
        }
        private string GetMutexName(string filePath)
        {
            return filePath.Replace("\\", "#");
        }
        private static string GetMetadataFilePath(string directory, string fileNamePrefix)
        {
            return Path.Combine(directory, $"{fileNamePrefix}.data");
        }
        private static string GetChunkFileName(int nChunk, string directory, string fileNamePrefix)
        {
            return Path.Combine(directory, $"{fileNamePrefix}_{nChunk}.data");
        }
        public void Update(Func<TContent, Exception,  TContent> update) {
            bool hasHandle = false;
            try
            {
                try
                {
                    hasHandle = _Mutex.WaitOne(_AcquireExclusiveAccessTimeoutMilliseconds, false);
                    if (hasHandle == false)
                        throw new TimeoutException();
                }
                catch (AbandonedMutexException abandonedMutexException)
                {
                    Logs.Default.Error(abandonedMutexException);
                    hasHandle = true;
                }
                _Update(update);
            }
            finally
            {
                if (hasHandle)
                    _Mutex.ReleaseMutex();
            }
        }
        private void _Update(Func<TContent, Exception, TContent> update) {
            DynamicSizeMetadata dynamicSizeMetadata = _MemoryMappedDynamicSizeMetadata.Read();
            Exception exception = null;
            TContent tContent = null;
            try
            {
               tContent = GetContent(dynamicSizeMetadata.CurrentContentLength);
            }
            catch (Exception ex) {
                Logs.Default.Error(exception);
            }
            tContent = update(tContent, exception);
            int currentContentLength = SetContent(tContent);
            _MemoryMappedDynamicSizeMetadata.Write(new DynamicSizeMetadata(currentContentLength));
        }
        private TContent GetContent(int currentContentLength) {
            byte[] bytes = new byte[currentContentLength];
            int nChunk = 0;
            for (int startIndex = 0; startIndex < currentContentLength; startIndex += _ChunkSize)
            {
                MemoryMapped memoryMappedChunk;
                if (nChunk >= _CurrentMemoryMappedChunks.Count)
                {
                    memoryMappedChunk = AddChunk(nChunk);
                } 
                else
                {
                    memoryMappedChunk = _CurrentMemoryMappedChunks[nChunk];
                }
                int nBytesToCopy = GetNBytesToCopy(startIndex, bytes.Length);
                Buffer.BlockCopy(memoryMappedChunk.Read(), 0, bytes, startIndex, nBytesToCopy);
                nChunk++;
            }
            return _Parser.Deserialize(bytes);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tContent"></param>
        /// <returns>currentContentLength</returns>
        private int SetContent(TContent tContent) {
            byte[] bytes = _Parser.Serialize(tContent);
            int requiredLength = bytes.Length;
            int currentLength = _ChunkSize * _CurrentMemoryMappedChunks.Count;
            if (currentLength < requiredLength)
            {
                AddChunksToFit(currentLength, requiredLength);
            }
            else {
                RemoveUnnecessaryChunks(currentLength, requiredLength);
            }
            int currentMemoryMappedChunkIndex = 0;
            for (int startIndex = 0; startIndex < requiredLength; startIndex += _ChunkSize)
            {
                byte[] bytesForMemoryMappedChunk = new byte[_ChunkSize];
                int nBytesToCopy = GetNBytesToCopy(startIndex, bytes.Length);
                Buffer.BlockCopy(bytes, startIndex, bytesForMemoryMappedChunk, 0, nBytesToCopy);
                _CurrentMemoryMappedChunks[currentMemoryMappedChunkIndex++].Write(bytesForMemoryMappedChunk);
            }
            return bytes.Length;
        }
        private void AddChunksToFit(int currentLength, int requiredLength) {
            int requiredExtraLength = requiredLength - currentLength;
            int requiredExtraChunks = (int)Math.Ceiling((double)requiredExtraLength / (double)_ChunkSize);
            int nChunk = _CurrentMemoryMappedChunks.Count;
            for (int i = 0; i < requiredExtraChunks; i++) {
                AddChunk(nChunk++);
            }
        }
        private MemoryMapped AddChunk(int nChunk) {
            MemoryMapped memoryMappedChunk = new MemoryMapped(GetChunkFileName(nChunk, _Directory, _FileNamePrefix), _ChunkSize);
            _CurrentMemoryMappedChunks.Add(memoryMappedChunk);
            return memoryMappedChunk;
        }
        private void RemoveUnnecessaryChunks(int currentLength, int requiredLength) {
            int extraLength = currentLength - requiredLength;
            int extraChunks = (int)Math.Floor((decimal)extraLength / (decimal)_ChunkSize);
            if (extraChunks <= 0) return;
            for (int i = 0; i < extraChunks; i++) {
                MemoryMapped memoryMappedChunk = _CurrentMemoryMappedChunks[_CurrentMemoryMappedChunks.Count - 1];
                memoryMappedChunk.Dispose(true);
                _CurrentMemoryMappedChunks.Remove(memoryMappedChunk);
            }
        }
        private int GetNBytesToCopy(int startIndex, int bytesLength) {

            int nBytesLeftInBytes = bytesLength - startIndex;
            int nBytesToCopy = nBytesLeftInBytes < _ChunkSize ? nBytesLeftInBytes : _ChunkSize;
            return nBytesToCopy;
        }
        public void Dispose() {
            lock (_LockObjectDispose)
            {
                if (_Disposed) return;
                _MemoryMappedDynamicSizeMetadata.Dispose();
                _CurrentMemoryMappedChunks.ForEach(memoryMappedChunk => memoryMappedChunk.Dispose());
                _Mutex.Dispose();
                _Disposed = true;
            }
        }
    }
}
