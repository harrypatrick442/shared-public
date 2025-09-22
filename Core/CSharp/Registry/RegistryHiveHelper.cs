using Microsoft.Win32;
using System;

namespace Core.Registry
{
    public static class RegistryHiveHelper
    {
        public static RegistryHive Parse(string hiveString)
        {
            string str = hiveString.ToLower().Replace("_", "").Replace(" ", "").Replace("-", "");
            switch (str)
            {
                case "hklm":
                case "hkeylocalmachine":
                    return RegistryHive.LocalMachine;
                case "hkcu":
                case "hkeycurrentuser":
                    return RegistryHive.CurrentUser;
                case "hku":
                case "hkusers":
                    return RegistryHive.Users;
                case "hkcc":
                case "hkeycurrentconfig":
                    return RegistryHive.CurrentConfig;
                case "hkcr":
                case "hkeyclassesroute":
                    return RegistryHive.ClassesRoot;
                default: throw new NotImplementedException();
            }
        }
    }
}
