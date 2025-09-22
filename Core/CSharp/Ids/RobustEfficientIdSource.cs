using System;
using System.Collections.Generic;
using Shutdown;
using System.IO;
using System.Linq;
using Core.Exceptions;
using System.Threading;
using Logging;
using Core;

namespace Core.Ids
{
    public abstract class RobustEfficientIdSource
    {
        private int _NFiles,
            N_INCREMENTS_PER_SAVE=100;
        protected bool _ShuttingDown = false;
        protected long _CurrentIdSanityCheck=-1;
        private string[] _FilePaths;
        protected long _CurrentId;
        private int _NIncrementsSinceSave = 0;
        private string _ErrorFilePath, _DidShutdownSaveFilePath;
        private bool _DidShutdownSaveFileExisted;
        protected readonly object _LockObject = new object();
        protected RobustEfficientIdSource(int nFiles, string directoryPath, string fileNamePrefix)
        {
            _NFiles = nFiles;
            _FilePaths = new string[_NFiles];
            _ErrorFilePath = Path.Combine(directoryPath, $"{fileNamePrefix}_error.txt");
            _DidShutdownSaveFilePath = Path.Combine(directoryPath, $"{fileNamePrefix}_didShutdownSave.json");

            _DidShutdownSaveFileExisted = File.Exists(_DidShutdownSaveFilePath);
            try { File.Delete(_DidShutdownSaveFilePath); } catch { }
            for (var i = 0; i < _NFiles; i++)
            {
                _FilePaths[i] = Path.Combine(directoryPath, $"{fileNamePrefix}_{i}.json");
            }
            ShutdownManager.Instance.Add(() =>
            {
                lock (_LockObject)
                {
                    _ShuttingDown = true;
                    Save();
                    File.WriteAllText(_DidShutdownSaveFilePath, "");
                }
            }, ShutdownOrder.IdSource);
        }
        protected abstract long IncrementId(long currentId, long currentIdSanityCheck, out long newCurrentIdSanityCheck);
        public virtual long NextId (){
            lock (_LockObject)
            {
                if (_ShuttingDown)
                    throw new ObjectDisposedException(this.GetType().Name, "Already shutting down!");
                _CurrentId = IncrementId(_CurrentId, _CurrentIdSanityCheck,  out _CurrentIdSanityCheck);
                try
                {
                    CheckSanity();
#if DEBUG
                    Save();
#else
                    _NIncrementsSinceSave++;
                    if (_NIncrementsSinceSave >= N_INCREMENTS_PER_SAVE)
                    {
                        Save();
                        _NIncrementsSinceSave = 0;
                    }
#endif

                    CheckSanity();
                    return _CurrentId;
                }
                catch (Exception ex) {
                    Logs.HighPriority.Error(ex);
                    _ShuttingDown = true;
                    ShutdownManager.Instance.Shutdown(exitCode: 2);
                    throw new FatalException("NextId", ex);
                }
            }
        }
        protected abstract long IncrementIdBy(long currentId, long currentIdSanityCheck, int n, int nSanityCheck, out long newCurrentIdSanityCheck);

        protected void LoadCurrentId() {
            try
            {
                if (File.Exists(_ErrorFilePath))
                    throw new FatalException($"There was previously a fatal exception saved to {_ErrorFilePath} which needs to be dealth with before the system can start");
                _CurrentId = LoadCurrentIdFromFiles();
                _CurrentIdSanityCheck = LoadCurrentIdFromFiles();
#if !DEBUG
                if(!_DidShutdownSaveFileExisted)
                    _CurrentId = IncrementIdBy(_CurrentId, _CurrentIdSanityCheck, N_INCREMENTS_PER_SAVE - 1, N_INCREMENTS_PER_SAVE - 1, out _CurrentIdSanityCheck);
#endif
                CheckSanity();
                Save();
            }
            catch (Exception ex) {
                Logs.HighPriority.Error(ex);
                _ShuttingDown = true;
                ShutdownManager.Instance.Shutdown(exitCode: 2);
                throw new FatalException("A serious error occured while loading", ex);
            }
        }
        protected void CheckSanity() {
            if (_CurrentId != _CurrentIdSanityCheck) throw new FatalException("Sanity check failed");
        }
        private long LoadCurrentIdFromFiles()
        {
            List<long> ids = new List<long>();
            foreach (string filePath in _FilePaths)
            {
                string content = File.ReadAllText(filePath);
                long value = long.Parse(content);
                ids.Add(value);
            }
            if (ids.Count < 1)
            {
                throw new Exception("no files existed!");
            }
            if(ids.Min()!=ids.Max())
                throw new ParseException("Failed to establish id from the saved files. They did not all match");
            return ids[0];
        }
        private long Save() {
            CountdownLatch countdownLatch = new CountdownLatch(_NFiles-1);
            long currentId = _CurrentId;
            string content = currentId.ToString();
            bool failedAWrite = false;
            Action callbackFailedAWrite = () => failedAWrite = true;
            for (int i = 1; i < _NFiles; i++) {
                SaveFileOnNewThread(i, content, callbackFailedAWrite, countdownLatch);
            }
            try
            {
                File.WriteAllText(_FilePaths[0], content);
            }
            catch (Exception ex) { 
                Logs.HighPriority.Error(ex);
                failedAWrite = true;
            }
            countdownLatch.Wait();
            if (failedAWrite)
            {
                File.WriteAllText(_ErrorFilePath, $"Failed a write during save. CurrentId was {_CurrentId} and sanity check was {_CurrentIdSanityCheck}");
                throw new FatalException("Failed a write during save");
            }
            if (currentId != long.Parse(content)||currentId!=_CurrentIdSanityCheck)
            {
                File.WriteAllText(_ErrorFilePath, $"Fatal corruption of currentId during save. CurrentId was {_CurrentId} and sanity check was {_CurrentIdSanityCheck}");
                throw new FatalException("currentId appears to have corrupted during save");
            }
            return currentId;
        }
        private void SaveFileOnNewThread(int i, string content, Action failedAWrite, CountdownLatch countdownLatch)
        {
            new Thread(() => {
                try
                {
                    File.WriteAllText(_FilePaths[i], content);
                }
                catch (Exception ex)
                {
                    Logs.HighPriority.Error(ex);
                    failedAWrite();
                }
                finally
                {
                    countdownLatch.Signal();
                }
            }).Start();
        }
    }
}