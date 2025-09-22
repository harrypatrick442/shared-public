using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.PInvoke
{
    using System;
    using System.Runtime.InteropServices;

    public static class Kernel32
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int GetLastError();

        public static void PrintLastError(string message)
        {
            int errorCode = GetLastError();
            Console.WriteLine($"{message} - Error Code: {errorCode} ({GetErrorMessage(errorCode)})");
        }

        public static string GetErrorMessage(int errorCode)
        {
            return errorCode switch
            {
                0 => "No error",
                2 => "File not found",
                5 => "Access denied",
                87 => "Invalid parameter",
                1062 => "Service not running",
                1168 => "Element not found",
                _ => $"Unknown error (Code {errorCode})"
            };
        }
    }

}
