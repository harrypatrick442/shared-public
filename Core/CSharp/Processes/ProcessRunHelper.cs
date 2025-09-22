using Logging;
using System;
using System.Diagnostics;
using System.Threading;

namespace Core.Processes
{
    public static class ProcessRunHelper
    {
        public static string Run(string fileName, string directory, string command,
            Func<string, string, bool> shouldThrowExceptionForError, int? timeout = null)
        {
            throw new NotImplementedException("Needs working on. Look at below");
            /*
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = directory
            };
            int timeoutInt = timeout != null ? (int)timeout : int.MaxValue;
            Process process = new Process();
            process.StartInfo = processStartInfo;
            using (CleanupHandle cleanupRead = ProcessRunHelper.StartReading(process, timeoutInt,
            out StringBuilder outputStringBuilder,
            out StringBuilder errorStringBuilder,
            out Func<bool> waitForReadOutput,
            out Func<bool> waitForReadError))
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.StandardInput.WriteLine(command);
                process.StandardInput.Flush();
                process.StandardInput.Close();
                if (process.WaitForExit(timeoutInt) &&
                waitForReadOutput() &&
                waitForReadError())
                {
                    // Process completed. Check process¬.ExitCode here.
                }
                else
                {
                    // Timed out.
                }
                var output = outputStringBuilder.ToString();
                var error = errorStringBuilder.ToString();
                if ((!string.IsNullOrEmpty(error) && shouldThrowExceptionForError == null) || (
                    shouldThrowExceptionForError != null && shouldThrowExceptionForError(error, output)))
                {
                    throw new OperationFailedException(error + "\n" + output);
                }
                return output;
            }*/
        }
        public static RunningProcessHandle RunAsynchronously(string fileName,
            string directory, string arguments,
            Action<string> onOutput, Action<string> onError,
            ILog? disposeExceptionsLog = null)
        {
            CountdownLatch countdownLatchStarted = new CountdownLatch();
            Exception startException = null;
            RunningProcessHandle handle= null;
            new Thread(() => {
                try
                {
                    var processStartInfo = new ProcessStartInfo()
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        RedirectStandardError = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        WorkingDirectory = directory
                    };
                    Process process = new Process();
                    process.StartInfo = processStartInfo;
                    handle = new RunningProcessHandle(
                        Get_Dispose(process, disposeExceptionsLog),
                        Get_Wait(process),
                        process
                    );
                    ProcessRunHelper.StartReadingToCollectors(
                        process,
                        onOutput,
                        onError
                    );
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }
                catch (Exception ex)
                {
                    handle?.Dispose();
                    startException = ex;
                }
                finally {
                    countdownLatchStarted.Signal();
                }
            }).Start();
            countdownLatchStarted.Wait();
            if (startException != null)
                throw new Exception("Failed to start process", startException);
            return handle;
        }
        private static Action<int?>Get_Wait(Process process) {
            return (timeoutMilliseconds) =>
            {
                if (timeoutMilliseconds != null)
                {
                    process.WaitForExit((int)timeoutMilliseconds);
                    return;
                }
                process.WaitForExit();
            };
        }
        private static Action Get_Dispose(Process process, ILog? disposeExceptionsLog) {
            return () =>
            {
                try
                {
                    process.StandardInput.Close();
                }
                catch (Exception ex)
                {
                    disposeExceptionsLog?.Error(ex);
                }//ProcessHelper.KillRecursively(_Process);
                try
                {
                    process.Close();
                }
                catch (Exception ex)
                {
                    disposeExceptionsLog?.Error(ex);
                }
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    disposeExceptionsLog?.Error(ex);
                }
            };
        }
        public static void StartReadingToCollectors(Process process,
            Action<string> onReadOutput,
            Action<string> onReadError)
        {
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    onReadOutput(e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    onReadError(e.Data);
                }
            };
        }
    }
}
