using System;
using System.Diagnostics;
using System.IO;
using Logging;

namespace Core.Processes
{
    public class ProcessWrapper
    {
        private System.Diagnostics.Process _Process;
        private string _SanitisedProcessName;
        public string SanitisedProcessName { get { return _SanitisedProcessName; } }
        private string _FilePath;
        private bool _TriedToGetFIlePath = false;
        public string FilePath { get {
                if (_TriedToGetFIlePath) return _FilePath;
                try
                {
                    ProcessModule processModule = _Process.MainModule;
                    if (processModule != null)
                        _FilePath = processModule.FileName;
                }
                catch (Exception ex)
                {
                    //No reason to print this. It just means you dont have permission with the current elevation of the application. If we print this for every attempt it will fill the console.
                }
                finally {
                    _TriedToGetFIlePath = true;
                }
                return _FilePath; 
            } 
        }
        public string RawName { get { return _Process.ProcessName; } }
        public ProcessWrapper(System.Diagnostics.Process process) {
            _Process = process;
            _SanitisedProcessName = SanitiseProcessName(process.ProcessName);
        }
        public static string SanitiseProcessName(string str)
        {
            return Path.GetFileNameWithoutExtension(str).Replace(" ", "").Replace("_", "").Replace("-", "").ToLower();
        }
        public bool TryKill()
        {
            try
            {
                _Process.Kill();
                return true;
            }
            catch (Exception ex)
            {
                Logs.Default.Debug(ex);
                return false;
            }
        }
    }
}
