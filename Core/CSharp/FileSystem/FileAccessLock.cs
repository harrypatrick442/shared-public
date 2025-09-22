using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Core.FileSystem {
    public class FileAccessLock
    {
        private static object _LockObjectGetDefault = new object();
        private static FileAccessLock _Default;
        public static FileAccessLock Default {
            get
            {
                lock (_LockObjectGetDefault)
                {
                    if (_Default != null) return _Default;
                    _Default = new FileAccessLock();
                    return _Default;
                }
            }
        }
        private Dictionary<string, Tuple<FileAccessLockHandle, CountdownLatch>> _MapFilePathToFileAccessLockHandleCountdownLatchPair
            = new Dictionary<string, Tuple<FileAccessLockHandle, CountdownLatch>>();
        public FileAccessLockHandle Lock(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            filePath = PathHelper.NormalizePath(filePath);
            while (true)
            {
                Tuple<FileAccessLockHandle, CountdownLatch> fileAccessLockHandleCountdownLatchPair;
                lock (_MapFilePathToFileAccessLockHandleCountdownLatchPair)
                {
                    if (!_MapFilePathToFileAccessLockHandleCountdownLatchPair.ContainsKey(filePath))
                    {
                        return AddFileAccessLockHandle(filePath);
                    }
                    fileAccessLockHandleCountdownLatchPair = _MapFilePathToFileAccessLockHandleCountdownLatchPair[filePath];
                }
                fileAccessLockHandleCountdownLatchPair.Item2.Wait();
            }
        }
        private FileAccessLockHandle AddFileAccessLockHandle(string filePath)
        {
            CountdownLatch countdownLatch = new CountdownLatch();
            FileAccessLockHandle fileAccessLockHandle = new FileAccessLockHandle(filePath, countdownLatch, Remove);
            _MapFilePathToFileAccessLockHandleCountdownLatchPair[filePath] = new Tuple<FileAccessLockHandle, CountdownLatch>(fileAccessLockHandle, countdownLatch);
            return fileAccessLockHandle;
        }
        private void Remove(FileAccessLockHandle fileAccessLockHandle)
        {
            lock (_MapFilePathToFileAccessLockHandleCountdownLatchPair)
            {
                string filePath = fileAccessLockHandle.FilePath;
                if (!_MapFilePathToFileAccessLockHandleCountdownLatchPair.ContainsKey(filePath)) return;
                CountdownLatch countdownLatch = _MapFilePathToFileAccessLockHandleCountdownLatchPair[filePath].Item2;
                _MapFilePathToFileAccessLockHandleCountdownLatchPair.Remove(filePath);
                countdownLatch.Signal();
            }
        }
    }
}
