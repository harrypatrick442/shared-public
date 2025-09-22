using Core.Processes;
using System.Diagnostics;

namespace Setup.Git
{
    public static class NPMHelper
    {
        public static int RunScript(string scriptName,
            string projectDirectoryPath, bool throwExceptionOnErrorCode = true)
        {
            using (RunningProcessHandle runningCmdHandle =
                ProcessRunHelper.RunAsynchronously(
                    "cmd", projectDirectoryPath,
                    null,
                    (e) => Console.WriteLine(e),
                    (e) =>
                    {
                        Console.WriteLine(e);
                    }))
            {
                runningCmdHandle.WriteLine($"npm run {scriptName} & exit");
                int exitCode = runningCmdHandle.Wait();
                if (throwExceptionOnErrorCode && exitCode != 0)
                {
                    throw new Exception($"Running script \"{scriptName}\" in directory \"{projectDirectoryPath}\" failed with exit code {exitCode}");
                }
                return exitCode;
            }
        }
    }
}