using Core.Timing;
using Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    public class RepeatExceptionLogHandler
    {
        private class SpecificRepeatException{
            private long _MinDelayMilliseconds;

            private long _LastLoggedAtMilliseconds = 0;
            public int LineNumber { get; }
            private int _NSinceLastLog = 0;
            private ILog _Log;
            private Exception _LastException;
            public SpecificRepeatException(ILog log, int lineNumber, long minDelayMilliseconds) {
                _Log = log;
                LineNumber = lineNumber;
                _MinDelayMilliseconds = minDelayMilliseconds;
            }
            public void Log(Exception ex) {
                lock (this)
                {
                    long now = TimeHelper.MillisecondsNow;
                    if ((now - _LastLoggedAtMilliseconds) < _MinDelayMilliseconds)
                    {
                        _NSinceLastLog++;
                        _LastException = ex;
                        return;
                    }
                    _Log.Error(new RepeatException(_NSinceLastLog+1, LineNumber, now, ex));
                    _LastLoggedAtMilliseconds = now;
                    _NSinceLastLog = 0;
                }
            }
            public void Dispose() {

                lock (this)
                {
                    if (_NSinceLastLog <= 0) return;
                    _Log.Error(new RepeatException(_NSinceLastLog, LineNumber, _LastLoggedAtMilliseconds, _LastException));
                }
            }
        }
        private int _MinDelayMilliseconds;
        private Dictionary<int, SpecificRepeatException> _MapLineNumberToSpecificRepeatException
            = new Dictionary<int, SpecificRepeatException>();
        private ILog _Log;
        public RepeatExceptionLogHandler(int minDelayMilliseconds, ILog log) { 
            _MinDelayMilliseconds = minDelayMilliseconds;
            _Log = log;
        }
        public void Log(Exception ex) {
            try
            {
                int lineNumber = GetLineNumber(ex);
                SpecificRepeatException repeatException;
                lock (_MapLineNumberToSpecificRepeatException)
                {
                    if (!_MapLineNumberToSpecificRepeatException.TryGetValue(lineNumber, out repeatException))
                    {
                        repeatException = new SpecificRepeatException(_Log, lineNumber, _MinDelayMilliseconds);
                        _MapLineNumberToSpecificRepeatException.Add(lineNumber, repeatException);
                    }
                }
                repeatException.Log(ex);
            }
            catch (Exception exInternal) {
                Logs.Default.Error(exInternal);
            }
        }
        private int GetLineNumber(Exception ex) {
            var st = new StackTrace(ex, true);
            var frame = st.GetFrame(0);
            return frame.GetFileLineNumber();
        }
        public void Dispose() {
            SpecificRepeatException[] specificRepeatExceptions;
            lock (_MapLineNumberToSpecificRepeatException) {
                specificRepeatExceptions = _MapLineNumberToSpecificRepeatException.Values.ToArray();
            }
            foreach (SpecificRepeatException s in specificRepeatExceptions) {
                try
                {
                    s.Dispose();
                }
                catch { }
            }
        }
    }
}
