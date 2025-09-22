using System;
using System.Collections.Generic;
using Shutdown;
using System.IO;

namespace Core.Ids
{
    public class BasicIdSource
    {
        protected bool _ShuttingDown = false;
        private string _JsonFilePath;
        protected long _NextId;
        protected readonly object _LockObject = new object();
        public virtual long NextId()
        {
            lock (_LockObject)
            {
                if (_ShuttingDown)
                    throw new InvalidOperationException("Already shutting down!");
                long nextId = _NextId++;
                SaveToFile();
                return nextId;
            }
        }
        //private Timer _TimerSaveToFile;
        protected BasicIdSource(string jsonFilePath, Func<long> getStartIdIfDoesntExist = null)
        {
            _JsonFilePath = jsonFilePath;
            ShutdownManager.Instance.Add(_Shutdown, ShutdownOrder.ImportantInMemoryFile);
            if (!File.Exists(jsonFilePath))
                _NextId = getStartIdIfDoesntExist != null ? getStartIdIfDoesntExist() : 1;
            else
                _NextId = int.Parse(File.ReadAllText(jsonFilePath));
            //_TimerSaveToFile = new Timer(DELAY_MILLISECONDS_SAVE_TO_FILE, -1, SaveToFile);
            // _TimerSaveToFile.Start();
        }
        private void _Shutdown()
        {
            //_TimerSaveToFile.Stop();
            SaveToFile();
        }
        protected void SaveToFile()
        {
            /*lock (_JsonFilePath)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_JsonFilePath));
                File.WriteAllText(_JsonFilePath, _NextId.ToString());
            }*/
        }
    }
}