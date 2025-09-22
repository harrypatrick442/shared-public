using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.ComponentModel;
using Core.Maths;
using MathNet.Numerics;
using Core.Timing;
using Core.Cleanup;
using System.Threading.Tasks;

namespace Core.Pool
{
    public abstract class ProgressHandler
    {
        private const string DEFAULT_ETC_STRING = "not determined";
        private const long MIN_DELAY_UPDATE_ETC = 20000;
        protected List<EventHandler<ProgressEventArgs>> _ProgressedEventHandlers;
        public event EventHandler<ProgressEventArgs> Progressed
        {
            add {
                lock (_LockObject) {
                    if (_ProgressedEventHandlers == null) _ProgressedEventHandlers 
                            = new List<EventHandler<ProgressEventArgs>>{ value};
                    else
                        _ProgressedEventHandlers.Add(value);
                }
            }
            remove
            {
                lock (_LockObject)
                {
                    _ProgressedEventHandlers.Remove(value);
                }
            }
        }
        protected readonly object _LockObject = new object();
        protected double _Proportion;
        public double Proportion { 
            get {
                lock (_LockObject)
                {
                    return _Proportion;
                }
            }
        }
        public ProgressHandler() { 
            
        }
        protected void Dispatch(EventHandler<ProgressEventArgs>[]? eventHandlers, double proportion) 
        {
            if (eventHandlers == null || !eventHandlers.Any()) return;
            ProgressEventArgs eventArgs = new ProgressEventArgs(proportion);
            foreach (var eventHandler in eventHandlers)
            {
                eventHandler.Invoke(this, eventArgs);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callbackPrint"></param>
        /// <returns>Dispose registration</returns>
        public CleanupHandle RegisterPrintProportion(Action<string> print)
        {
            EventHandler<ProgressEventArgs> handle = (o, e) => {
                print((e.Proportion).RoundToMultiple(0.0001).ToString());
            };
            Progressed += handle;
            return new CleanupHandle(() => Progressed -= handle);
        }
        public CleanupHandle RegisterPrintPercent(Action<string> print)
        {
            EventHandler<ProgressEventArgs> handle = (o, e) => {
                print($"{Math.Round(e.Proportion * 100d, 2)}%");
            };
            Progressed += handle;
            return new CleanupHandle(() => Progressed -= handle);
        }
        public CleanupHandle RegisterPrintPercentSameLine(string prefix)
        {
                return RegisterPrintSameLine((e) =>$"{prefix}{Math.Round(e.Proportion * 100d, 2)}%");
        }
        public CleanupHandle RegisterPrintPercentSameLineWithEstimatedCompletionTime(string prefix, int decimalPlaces = 2)
        {
            long? startTime = null;
            long lastUpdatedETC = 0;
            string? lastEtcStr = null;
            return RegisterPrintSameLine((e) =>
            {
                if (e.Proportion >= 1) {
                    return $"{prefix}100%";
                }
                string etcStr;
                if (startTime == null)
                {
                    startTime = TimeHelper.MillisecondsNow;
                    etcStr = DEFAULT_ETC_STRING;
                }
                else { 
                    if (e.Proportion <= 0)
                    {
                        etcStr = DEFAULT_ETC_STRING;
                    }
                    else
                    {
                        long now = TimeHelper.MillisecondsNow;
                        if (lastEtcStr == null || now - lastUpdatedETC >= MIN_DELAY_UPDATE_ETC)
                        {
                            long ellapsed = now - (long)startTime;
                            long etc = ((long)startTime + (long)(ellapsed / e.Proportion));
                            try
                            {
                                lastUpdatedETC = now;
                                DateTime etcDateTime = TimeHelper.GetDateTimeFromMillisecondsUTC(etc);
                                etcStr = etcDateTime.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
                                lastEtcStr = etcStr;
                            }
                            catch
                            {
                                etcStr = DEFAULT_ETC_STRING;
                            }
                        }
                        else {
                            etcStr = lastEtcStr!;
                        }
                    }
                }
                return $"{prefix}{Math.Round(e.Proportion * 100d, decimalPlaces)}%, ETC: {etcStr}";
            });
        }

        private CleanupHandle RegisterPrintSameLine(Func<ProgressEventArgs, string> createString)
        {
            // Store the initial cursor position
            int originalCursorTop = Console.CursorTop;
            int previousLength = 0; // To track the length of the previous output
            object lockObject = new object();
            EventHandler<ProgressEventArgs> handle = (o, e) =>
            {
                lock (lockObject)
                {
                    // If the cursor moved down, something else printed, so move to a new line
                    if (Console.CursorTop != originalCursorTop)
                    {
                        Console.WriteLine(); // Move to the next line
                        originalCursorTop = Console.CursorTop; // Update the original cursor position
                    }

                    // Set the cursor to the beginning of the line
                    Console.SetCursorPosition(0, originalCursorTop);

                    // Prepare the new message to print
                    string message = createString(e);

                    // Print the message, and if necessary, pad with spaces to overwrite any previous text
                    var a = message.PadRight(previousLength);
                    Task.Run(() => Console.Write(a));

                    // Update the previous length with the current message length
                    previousLength = message.Length;
                }
            };

            Progressed += handle;

            return new CleanupHandle(() =>
            {
                // When cleaning up, check if the cursor is still on the line where we printed the progress
                if (Console.CursorTop == originalCursorTop)
                {
                    Console.WriteLine(); // Move to the next line to avoid overwriting in the same line
                }

                // Remove the event handler
                Progressed -= handle;
            });
        }
    }
}
