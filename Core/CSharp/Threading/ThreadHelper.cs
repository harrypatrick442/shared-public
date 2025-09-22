using Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
namespace Core.Threading {
    public static class ThreadHelper
    {

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();
        public static uint GetUnmanagedThreadId() {
            return GetCurrentThreadId();
        }
    }
}
