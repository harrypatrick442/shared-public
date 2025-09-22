using Microsoft.Win32;
using System;
using System.Linq;
using Snippets.NativeExtensions;
using Core.Exceptions;
using Core.NativeExtensions;
using System.Runtime.InteropServices;
namespace Core.Registry
{
    public class RegistryHelper
    {
        private static bool _Is64bit = Environment.Is64BitOperatingSystem;
        private static void CheckIsWindows() {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new IllegalOperationException("Should not be trying to access the registry on a platform other than windows");
        }
        public static T Query<T>(RegistryPath registryPath)
        {
            CheckIsWindows();
            object value = Query(registryPath);
            if (value == null)
            {
                return default(T);
            }
            T converted;
            if (value.TryCast<T>(out converted))
                return converted;
            return default(T);
        }
        public static object Query(RegistryPath registryPath, bool throwOnException = false)
        {
            CheckIsWindows();
            try
            {
                RegistryView view = GetRegistryView();
                using (RegistryKey registryKeyHive = RegistryKey.OpenBaseKey(registryPath.Hive, view))
                {
                    if (registryKeyHive == null) return null;
                    using (RegistryKey registryKeyPath = registryKeyHive.OpenSubKey(registryPath.Path))
                    {
                        if (registryKeyPath == null) return null;
                        return registryKeyPath.GetValue(registryPath.Key);
                    }
                }
            }
            catch (Exception ex)
            {
                ex = new OperationFailedException($"Querying registry path \"{registryPath}\" failed", ex);
                if (throwOnException)
                    throw ex;
                return null;
            }
        }
        public static void Delete(RegistryPath registryPath)
        {
            CheckIsWindows();
            Delete(registryPath.Hive, registryPath.Path, registryPath.Key);
        }
        public static void DeleteValue(RegistryPath registryPath)
        {
            CheckIsWindows();
            DeleteValue(registryPath.Hive, registryPath.Path, registryPath.Key);
        }
        public static void DeleteValue(RegistryHive registryHive, string path, string key)
        {
            CheckIsWindows();
            if (key == null) throw new ArgumentNullException("Key cannot be null");
            Delete(registryHive, path, key);
        }
        public static void Delete(RegistryHive registryHive, string path, string key = null)
        {
            CheckIsWindows();
            RegistryView view = GetRegistryView();
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(registryHive, view))
            {
                if (key != null)
                {
                    using (RegistryKey subkeyPath = baseKey.OpenSubKey(path, true))
                    {
                        if (subkeyPath == null) return;
                        subkeyPath.DeleteValue(key, false);
                        return;
                    }
                }
                baseKey.DeleteSubKeyTree(path, false);
            }
        }
        public static string[] GetSubkeys(RegistryPath registryPath, bool throwOnException = false)
        {
            CheckIsWindows();
            return GetSubkeys(registryPath.Hive, registryPath.Key==null?registryPath.Path:JoinPath(registryPath.Path, registryPath.Key), throwOnException);
        }
        public static string[] GetSubkeys(RegistryHive registryHive, string registryPath, bool throwOnException = false)
        {
            CheckIsWindows();
            try
            {
                RegistryView view = GetRegistryView();
                using (RegistryKey registryKeyHive = RegistryKey.OpenBaseKey(registryHive, view))
                {
                    if (registryKeyHive == null) return null;
                    using (RegistryKey registryKeyPath = registryKeyHive.OpenSubKey(registryPath))
                    {
                        if (registryKeyPath == null) return null;
                        return registryKeyPath.GetSubKeyNames();
                    }
                }
            }
            catch (Exception ex)
            {
                ex = new OperationFailedException($"Getting subkeys for registry path \"{registryPath}\" failed", ex);
                if (throwOnException)
                    throw ex;
                return null;
            }
        }
        public static string[] GetValueNames(RegistryPath registryPath, bool throwOnException = false)
        {
            CheckIsWindows();
            return GetValueNames(registryPath.Hive, registryPath.Key==null?registryPath.Path:JoinPath(registryPath.Path, registryPath.Key));
        }
        public static string[] GetValueNames(RegistryHive registryHive, string registryPath, bool throwOnException = false)
        {
            CheckIsWindows();
            try
            {
                RegistryView view = GetRegistryView();
                using (RegistryKey registryKeyHive = RegistryKey.OpenBaseKey(registryHive, view))
                {
                    if (registryKeyHive == null) return null;
                    using (RegistryKey registryKeyPath = registryKeyHive.OpenSubKey(registryPath))
                    {
                        if (registryKeyPath == null) return null;
                        return registryKeyPath.GetValueNames();
                    }
                }
            }
            catch (Exception ex)
            {
                ex = new OperationFailedException($"Getting value names for registry path \"{registryPath}\" failed", ex);
                if (throwOnException)
                    throw ex;
                return null;
            }
        }
        public static bool PathExists(RegistryPath registryPath)
        {
            CheckIsWindows();
            try
            {
                RegistryView view = GetRegistryView();
                using (RegistryKey registryKeyHive = RegistryKey.OpenBaseKey(registryPath.Hive, view))
                {
                    if (registryKeyHive != null)
                    {
                        using (RegistryKey registryKeyPath = registryKeyHive.OpenSubKey(registryPath.Path))
                        {
                            bool registryPathExists = registryKeyPath != null;
                            if (!registryPathExists || registryPath.Key == null||registryPath.Key=="") return registryPathExists;
                            return registryKeyPath.GetValueNames()
                                .Select(subkeyName=>subkeyName.ToLower()).Contains(registryPath.Key.ToLower());
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }
        private static RegistryView GetRegistryView() {
            if (_Is64bit) { return RegistryView.Registry64; }
            return RegistryView.Registry32;
        }
        public static RegistryValueKind ParseToRegistryValueKind(string str)
        {
            CheckIsWindows();
            switch (str.ToLower().Replace("-", "").Replace("_", ""))
            {
                case "dword":
                case "regdword": return RegistryValueKind.DWord;
                case "binary":
                case "regbinary": return RegistryValueKind.Binary;
                case "expandsz":
                case "regexpandsz":
                case "expandstring":
                case "regexpandstring": return RegistryValueKind.ExpandString;
                case "sz":
                case "regsz":
                case "string":
                case "regstring": return RegistryValueKind.String;
                case "multisz":
                case "multistring":
                case "regmultistring":
                case "regmultisz": return RegistryValueKind.MultiString;
                case "qword":
                case "regqword": return RegistryValueKind.QWord;
                default:
                    return RegistryValueKind.DWord;

            }
        }
        public static void Set(RegistryPath registryPath, object value, RegistryValueKind registryValueKind, bool throwOnException = false)
        {
            CheckIsWindows();
            RegistryKey registryKey = null, registryKeySub = null;
            try
            {
                RegistryView view = RegistryView.Registry32;
                if (_Is64bit) { view = RegistryView.Registry64; }
                registryKey = RegistryKey.OpenBaseKey(registryPath.Hive, view);
                if (registryKey == null) throw new ArgumentException("Invalid registry hive");
                registryKeySub = registryKey.OpenSubKey(registryPath.Path);
                if (registryKeySub == null)
                    registryKeySub = registryKey.CreateSubKey(registryPath.Path);
                registryKeySub = registryKey.OpenSubKey(registryPath.Path, true);
                if (registryKeySub == null) throw new ArgumentException("Invalid registry path");
                registryKeySub.SetValue(registryPath.Key, value, registryValueKind);
            }
            catch (Exception ex)
            {
                ex = new OperationFailedException($"Setting registry path \"{registryPath}\" to value {value} failed", ex);
                if (throwOnException)
                    throw ex;
            }
            finally
            {
                if (registryKeySub != null)
                {
                    registryKeySub.Dispose();
                }
                if (registryKey != null)
                {
                    registryKey.Dispose();
                }
            }
        }
        public static string JoinPath(params string[] subPaths)
        {
            if (subPaths == null || subPaths.Length < 1) return null;
            string path = subPaths[0].SetEndSlashes(false, false);
            foreach (string subPath in subPaths.Skip(1))
            {
                path += subPath.SetEndSlashes(true, false);
            }
            return path;
        }
    }
    public static class RegistryExtensionMethods
    {
        public static string GetPathString(this RegistryHive registryHive)
        {
            switch (registryHive)
            {
                case RegistryHive.LocalMachine: return "HKEY_LOCAL_MACHINE";
                case RegistryHive.CurrentUser: return "HKEY_CURRENT_USER";
                case RegistryHive.ClassesRoot: return "HKEY_CLASSES_ROOT";
                case RegistryHive.CurrentConfig: return "HKEY_CURRENT_CONFIG";
                case RegistryHive.Users: return "HKEY_USERS";
            }
            return "";
        }
    }
}
