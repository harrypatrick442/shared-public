using System;

namespace Core.Processes
{
    public static class CmdHelper
    {
        public static string Run(string directory, string command,
            Func<string, string, bool> shouldThrowExceptionForError, int? timeout=null)
        {

            return ProcessRunHelper.Run("cmd", directory, command, shouldThrowExceptionForError, timeout);
        }
        public static RunningProcessHandle RunAsynchronously(string directory, string command, bool readEverythingElseOnlyLines)
        {
            throw new NotImplementedException();/*
            RunningProcessHandle runningCmdHandle =  ProcessRunHelper.RunAsynchronously("cmd", directory, null, );
            runningCmdHandle.WriteLine(command);
            return runningCmdHandle;*/
        }
    }
}
