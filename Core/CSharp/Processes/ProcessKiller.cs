
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using Core.FileSystem;
using Core.Processes;
using Logging;

namespace Snippets.Processes
{
    public static class ProcessKiller
    {
        private const string BUILDING_REPRESENTATIVE_STRING_FAILED = "\"Building representative string failed\"";
        public static void KillProcessesUsingDirectoriesOrFilesInThem(System.IO.DirectoryInfo[] directoryInfos, 
            int[] myProcessIds,Func<Process, string> getMainModuleFileName, Func<string, bool> shouldIncludePath = null)
        {                                                                                                                                      
            foreach (Process process in Process.GetProcesses())
            {
                if (myProcessIds.Contains(process.Id)) continue;
                try
                {
                    try
                    {
                        string fileName = getMainModuleFileName(process);
                        if (shouldIncludePath != null && !shouldIncludePath(fileName)) continue;
                    }
                    catch { }
                        foreach (var directoryInfo in directoryInfos)
                    {
                        if (KillProcessesUsingDirectoriesOrFilesInThem_WorkingDirectory(process, directoryInfo)) break;
                        if (KillProcessUsingDirecotriesOrFilesInThem_MainModule(process, directoryInfo, getMainModuleFileName)) break;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            foreach (DirectoryInfo directoryInfo in directoryInfos)
            {
                string[] filePaths = DirectoryHelper.GetFilesRecursively(directoryInfo.FullName);
                if (shouldIncludePath != null) {
                    filePaths = filePaths.Where(filePath => shouldIncludePath(filePath)).ToArray();
                }
                if (directoryInfo.Exists)
                    KillProcessesLockingFiles(filePaths, myProcessIds);
            }
        }
        private static bool KillProcessesUsingDirectoriesOrFilesInThem_WorkingDirectory(Process process, DirectoryInfo directoryInfo) {

            try
            {
                if (process.StartInfo != null && !string.IsNullOrEmpty(process.StartInfo.WorkingDirectory))
                {
                    if (PathHelper.ContainsOrEqualsPath(directoryInfo, new DirectoryInfo(process.StartInfo.WorkingDirectory)))
                    {
                        string processDescriptionString = GetProcessDescription(process);
                        LogAboutToAttemptToKill(processDescriptionString);
                        try
                        {
                            process.Kill();
                            LogSuccessfullyKilled(processDescriptionString);
                        }
                        catch (Exception ex) {
                            LogErrorOccuredAttemptingToKill(processDescriptionString, ex);
                        }
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }
        private static bool KillProcessUsingDirecotriesOrFilesInThem_MainModule(Process process, DirectoryInfo directoryInfo,
            Func<Process, string> getMainModuleFileName)
        {
            try
            {
                if (process.MainModule != null
                && PathHelper.ContainsOrEqualsPath(new FileInfo(getMainModuleFileName(process)), directoryInfo))
                {
                    string processDescriptionString = GetProcessDescription(process);
                    LogAboutToAttemptToKill(processDescriptionString);
                    try
                    {
                        process.Kill();
                        LogSuccessfullyKilled(processDescriptionString);
                    }
                    catch (Exception ex)
                    {
                        LogErrorOccuredAttemptingToKill(processDescriptionString, ex);
                    }
                    return true;
                }
            }
            catch { }
            return false;
        }
        private static string GetProcessDescription(Process process) {
            string workingDirectory = "Couldnt get";
            try
            {
                workingDirectory = process.StartInfo.WorkingDirectory;
            }
            catch { }
            try
            {
                return $"{{{nameof(process.ProcessName)}:\"{process.ProcessName}\", pid:{process.Id}, path:\"{workingDirectory}\"}}";
            }
            catch {

                return BUILDING_REPRESENTATIVE_STRING_FAILED;
            }
        }
        public static void KillProcessesLockingFiles(string[] paths, int[] myProcessesIds) {
            foreach (string path in paths) {
                KillProcessesLockingFile(path, myProcessesIds);
            }
        }
        public static void KillProcessesLockingFile(string path, int[] myProcessIds) {
            try
            {
                Logs.Default.Info($"Attempting to kill any processes locking file with path \"{path}\"");
                foreach (Process process in WhoIsLocking(path))
                {
                    Logs.Default.Info($"File with path \"{path}\" was being locked by pid {process.Id}");
                    if (myProcessIds.Contains(process.Id)) continue;
                    string processDescriptionString = GetProcessDescription(process);
                    try
                    {
                        LogAboutToAttemptToKill(processDescriptionString);
                        process.Kill();
                        LogSuccessfullyKilled(processDescriptionString);
                    }
                    catch(Exception ex) {
                        LogErrorOccuredAttemptingToKill(processDescriptionString, ex);
                    }
                }
            }
            catch(Exception ex) { Logs.Default.Debug(ex); }
        }
        private static void LogSuccessfullyKilled(string processDescriptionString)
        {
            Logs.Default.Info($"Successfully killed process {processDescriptionString}");
        }
        private static void LogAboutToAttemptToKill(string processDescriptionString)
        {
            Logs.Default.Info($"About to attempt to kill killed process {processDescriptionString}");
        }
        private static void LogErrorOccuredAttemptingToKill(string processDescriptionString, Exception ex)
        {
            Logs.Default.Warn($"An error occured attempting to kill process {processDescriptionString} {ex?.ToString()}");
        }
        /// <summary>
        /// Find out what process(es) have a lock on the specified file.
        /// </summary>
        /// <param name="path">Path of the file.</param>
        /// <returns>Processes locking the file</returns>
        /// <remarks>See also:
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa373661(v=vs.85).aspx
        /// http://wyupdate.googlecode.com/svn-history/r401/trunk/frmFilesInUse.cs (no copyright in code at time of viewing)
        /// 
        /// </remarks>
        public static List<Process> WhoIsLocking(string path)
        {
            uint handle;
            string key = Guid.NewGuid().ToString();
            List<Process> processes = new List<Process>();

            int res = RmStartSession(out handle, 0, key);
            if (res != 0) throw new Exception("Could not begin restart session.  Unable to determine file locker.");

            try
            {
                const int ERROR_MORE_DATA = 234;
                uint pnProcInfoNeeded = 0,
                     pnProcInfo = 0,
                     lpdwRebootReasons = RmRebootReasonNone;

                string[] resources = new string[] { path }; // Just checking on one resource.

                res = RmRegisterResources(handle, (uint)resources.Length, resources, 0, null, 0, null);

                if (res != 0) throw new Exception("Could not register resource.");

                //Note: there's a race condition here -- the first call to RmGetList() returns
                //      the total number of process. However, when we call RmGetList() again to get
                //      the actual processes this number may have increased.
                res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);

                if (res == ERROR_MORE_DATA)
                {
                    // Create an array to store the process results
                    RM_PROCESS_INFO[] processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
                    pnProcInfo = pnProcInfoNeeded;

                    // Get the list
                    res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons);
                    if (res == 0)
                    {
                        processes = new List<Process>((int)pnProcInfo);

                        // Enumerate all of the results and add them to the 
                        // list to be returned
                        for (int i = 0; i < pnProcInfo; i++)
                        {
                            try
                            {
                                processes.Add(Process.GetProcessById(processInfo[i].Process.dwProcessId));
                            }
                            // catch the error -- in case the process is no longer running
                            catch (ArgumentException) { }
                        }
                    }
                    else throw new Exception("Could not list processes locking resource.");
                }
                else if (res != 0) throw new Exception("Could not list processes locking resource. Failed to get size of result.");
            }
            finally
            {
                RmEndSession(handle);
            }

            return processes;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RM_UNIQUE_PROCESS
        {
            public int dwProcessId;
            public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
        }

        const int RmRebootReasonNone = 0;
        const int CCH_RM_MAX_APP_NAME = 255;
        const int CCH_RM_MAX_SVC_NAME = 63;

        enum RM_APP_TYPE
        {
            RmUnknownApp = 0,
            RmMainWindow = 1,
            RmOtherWindow = 2,
            RmService = 3,
            RmExplorer = 4,
            RmConsole = 5,
            RmCritical = 1000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct RM_PROCESS_INFO
        {
            public RM_UNIQUE_PROCESS Process;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_APP_NAME + 1)]
            public string strAppName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_SVC_NAME + 1)]
            public string strServiceShortName;

            public RM_APP_TYPE ApplicationType;
            public uint AppStatus;
            public uint TSSessionId;
            [MarshalAs(UnmanagedType.Bool)]
            public bool bRestartable;
        }

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        static extern int RmRegisterResources(uint pSessionHandle,
                                              UInt32 nFiles,
                                              string[] rgsFilenames,
                                              UInt32 nApplications,
                                              [In] RM_UNIQUE_PROCESS[] rgApplications,
                                              UInt32 nServices,
                                              string[] rgsServiceNames);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
        static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

        [DllImport("rstrtmgr.dll")]
        static extern int RmEndSession(uint pSessionHandle);

        [DllImport("rstrtmgr.dll")]
        static extern int RmGetList(uint dwSessionHandle,
                                    out uint pnProcInfoNeeded,
                                    ref uint pnProcInfo,
                                    [In, Out] RM_PROCESS_INFO[] rgAffectedApps,
                                    ref uint lpdwRebootReasons);

    }
}