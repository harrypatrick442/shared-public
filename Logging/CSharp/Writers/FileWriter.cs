using Enums;
using System.Reflection;
using Logging;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading;
using System.Linq;

namespace Logging.Writers
{
    public class FileWriter : ILogWriter
    {
        private const int TIMEOUT_ACQUIRE_MUTEX_APPEND_MILLISECONDS = 200, TIMEOUT_ACQUIRE_MUTEX_CLEAR_MILLISECONDS = 500;
        private readonly object _LockObjectThreadSafe = new object();
        private string _ExplicitFilePath, _FileNamesPrefix;
        public FileWriter(string filePath = null, string fileNamesPrefix = null)
        {
            if (filePath != null)
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            //if (filePath == null) throw new ArgumentNullException("If you want to use the default log file location do not parse in a filePath parameter");
            _ExplicitFilePath = filePath;
            _FileNamesPrefix = fileNamesPrefix;
        }
        public void Initialize()
        {
        }

        private Dictionary<LogLevel, string> _MapLogLevelToLogFilePath = new Dictionary<LogLevel, string>();
        private string GetLogFilePath(LogLevel logLevel)
        {
            if (_ExplicitFilePath != null) return _ExplicitFilePath;
            if (_MapLogLevelToLogFilePath.ContainsKey(logLevel)) return _MapLogLevelToLogFilePath[logLevel];
            string fileName = logLevel.GetString() + ".log";
            if (_FileNamesPrefix != null) fileName = $"{_FileNamesPrefix}_{fileName}";
            string path = Path.Combine(Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Logs"), fileName);
            _MapLogLevelToLogFilePath[logLevel] = path;
            return path;
        }
        public virtual string GetHeading(LogLevel logLevel)
        {
            string heading = $"[{DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss")}";
            if (_ExplicitFilePath != null) heading += $" {logLevel.GetString()}]";
            else heading += "]";
            return heading;
        }
        public void Write(object o, LogLevel logLevel)
        {
            if (o == null) return;
            string str = ParseToString(o);
            string heading = GetHeading(logLevel);
            string logFilePath = GetLogFilePath(logLevel);
            using (var mutex = GetMutex(logLevel, logFilePath))
            {
                var mutexAcquired = false;
                try
                {
                    mutexAcquired = mutex.WaitOne(TIMEOUT_ACQUIRE_MUTEX_APPEND_MILLISECONDS);
                }
                catch (AbandonedMutexException)
                {
                    mutexAcquired = true;
                }
                if (!mutexAcquired) return;
                try
                {
                    File.AppendAllLines(logFilePath, new string[] { heading + str, "" });
                }
                catch (Exception ex)
                {
#if DEVELOPMENT
                    Console.WriteLine(ex);
#endif
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
        private Mutex GetMutex(LogLevel logLevel, string logFilePath)
        {
            return new Mutex(false, GetMutexName(logLevel, logFilePath));
        }
        private string GetMutexName(LogLevel logLevel, string logFilePath)
        {
            return logFilePath.Replace("\\", "").Replace("/", "");
        }
        private string ParseToString(object o)
        {
            Type type = o.GetType();
            if (type == typeof(string))
            {
                return (string)o;
            }
            if (typeof(Exception).IsAssignableFrom(type))
            {
                return ((Exception)o).ToString();
            }
            return o.ToString();
        }
        public void Clear()
        {
            lock (_LockObjectThreadSafe)
            {
                foreach (LogLevel logLevel in Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>())
                {
                    string logFilePath = GetLogFilePath(logLevel);
                    using (var mutex = GetMutex(logLevel, logFilePath))
                    {
                        var mutexAcquired = false;
                        try
                        {
                            mutexAcquired = mutex.WaitOne(TIMEOUT_ACQUIRE_MUTEX_CLEAR_MILLISECONDS);
                        }
                        catch (AbandonedMutexException)
                        {
                            mutexAcquired = true;
                        }
                        if (!mutexAcquired) return;
                        try
                        {
                            if (File.Exists(logFilePath)) File.WriteAllText(logFilePath, "");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        finally
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }
            }
        }
        public void Breadcrumb(IBreadcrumb breadcrumb)
        {

        }
    }
}
