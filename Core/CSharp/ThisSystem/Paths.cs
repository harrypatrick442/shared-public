using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Core.Exceptions;
using Core.Strings;
using Initialization.Exceptions;

namespace Core.ThisSystem
{
    public class Paths{
        private static object _LockObjectDefault = new object();
        private static Paths _Instance;
        private string _InstallationLocation, _CachedManifestFileLocation, _InstalledManifestFile,
            _AppDatabaseFile, _ConfigCsv, _UpdateFolder, _ResourceFolder,
            _UIExec, _CoreExec, _ServiceExec, _SystemDrive,
            _ElevatedServiceInput, _ElevatedServiceOutput, _ElevatedServiceError, _ServiceFiles, _LogFolder, _LanguageFolder,
            _CoreExecWhileDebuggingFreshlyCompiled, _LanguageFilesDirectory;
        private static string _AppDataConfigJson, _CacheFile, _UICacheFile, _CacheDir, _ProgramData, _DirectoryRunningIn,
            _RunningExecFile;
        public static Paths Default
        {
            get
            {
                lock (_LockObjectDefault)
                {
                    if (_Instance == null) 
                        throw NotInitializedException.ForThis();
                    return _Instance;
                }
            }
        }
        public static Paths InitializeDefault(string installationLocation) {
            lock (_LockObjectDefault)
            {
                if (_Instance != null&&_Instance._InstallationLocation!=installationLocation) throw new AlreadyInitializedException($"Attempted to initialize again with a different {nameof(installationLocation)}");
                { _Instance = new Paths(installationLocation); }
                return _Instance;
            }
        }
        private static string ProgramFilesDirectoryForX86 { get {
                return Path.Combine(Environment.GetFolderPath(Environment.Is64BitOperatingSystem
                        ?
                        Environment.SpecialFolder.ProgramFilesX86
                        :
                        Environment.SpecialFolder.ProgramFiles)
                );
            } 
        }

        public Paths(string installationLocation) {
            _InstallationLocation = installationLocation;
        }
        public string InstallationLocation{
            get{
                return _InstallationLocation;
            }
        }
        public string UpdateFolder{
            get{
                if(_UpdateFolder == null)
                {
                    _UpdateFolder = Path.Combine(InstallationLocation, "Update");
                }
                return _UpdateFolder;
            }
        }
        public string ConfigCsv
        {
            get
            {
                if (_ConfigCsv == null)
                {
                    _ConfigCsv = Path.Combine(InstallationLocation, "config.csv");
                }
                return _ConfigCsv;
            }
        }
        public static string SysWow64Directory
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64");
            }
        }
        public static string System32Directory
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.System);
            }
        }
        public string ResourceFolder {
          get{
            if(_ResourceFolder == null){
              _ResourceFolder = Path.Combine(InstallationLocation, "Resources");
            }
            return _ResourceFolder;
          }
        }
        public static string CacheDir {
          get{
            if(_CacheDir == null){
              _CacheDir = Path.Combine(ProgramData, "Cache");
            }
            return _CacheDir;
          }
        }
        public static string CacheFile
        {
            get
            {
                if (_CacheFile == null)
                {
                    _CacheFile = Path.Combine(CacheDir, "cache.json");
                }
                return _CacheFile;
            }
        }
        public string SystemDrive {
          get{
            if(_SystemDrive == null){
                _SystemDrive = Environment.GetEnvironmentVariable("SystemDrive");
            }
            return _SystemDrive;
          }
        }
        public static string ProgramData
        {
            get
            {
                if (_ProgramData == null)
                {
                    _ProgramData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Snippets");
                }
                return _ProgramData;
            }
        }
        public static string RunningExeFile
        {
            get
            {
                if (_RunningExecFile == null)
                {
                    _RunningExecFile = Assembly.GetEntryAssembly().Location;
                }
                return _RunningExecFile;
            }
        }
        public static string DirectoryRunningIn
        {
            get
            {
                if (_DirectoryRunningIn == null)
                {
                    _DirectoryRunningIn = Path.GetDirectoryName(RunningExeFile);
                }
                return _DirectoryRunningIn;
            }
        }
    }
}