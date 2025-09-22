
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Core.FileSystem
{

    public class InMemoryExecutableRunner
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAlloc(
            IntPtr lpAddress,
            uint dwSize,
            uint flAllocationType,
            uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFree(
            IntPtr lpAddress,
            uint dwSize,
            uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateThread(
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            out uint lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(
            IntPtr hHandle,
            uint dwMilliseconds);

        [StructLayout(LayoutKind.Sequential)]
        private struct ThreadParameters
        {
            public IntPtr MemoryAddress;
            public IntPtr ArgumentsAddress;
        }

        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RESERVE = 0x2000;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;
        private const uint WAIT_OBJECT_0 = 0x0;

        public static void Run(byte[] executableBytes, string arguments)
        {
            // Allocate memory for the executable
            IntPtr executableMemory = VirtualAlloc(IntPtr.Zero, (uint)executableBytes.Length, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (executableMemory == IntPtr.Zero)
            {
                throw new Exception("Failed to allocate memory for executable.");
            }

            // Allocate memory for the arguments
            byte[] argumentsBytes = System.Text.Encoding.UTF8.GetBytes(arguments + '\0'); // Null-terminated string
            IntPtr argumentsMemory = VirtualAlloc(IntPtr.Zero, (uint)argumentsBytes.Length, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (argumentsMemory == IntPtr.Zero)
            {
                VirtualFree(executableMemory, 0, 0x8000);
                throw new Exception("Failed to allocate memory for arguments.");
            }

            try
            {
                // Copy executable bytes and arguments into allocated memory
                Marshal.Copy(executableBytes, 0, executableMemory, executableBytes.Length);
                Marshal.Copy(argumentsBytes, 0, argumentsMemory, argumentsBytes.Length);

                // Prepare thread parameters
                ThreadParameters parameters = new ThreadParameters
                {
                    MemoryAddress = executableMemory,
                    ArgumentsAddress = argumentsMemory
                };

                IntPtr parametersPointer = Marshal.AllocHGlobal(Marshal.SizeOf<ThreadParameters>());
                Marshal.StructureToPtr(parameters, parametersPointer, false);

                // Create a thread to execute the memory with arguments
                uint threadId;
                IntPtr threadHandle = CreateThread(IntPtr.Zero, 0, executableMemory, parametersPointer, 0, out threadId);
                if (threadHandle == IntPtr.Zero)
                {
                    throw new Exception("Failed to create thread.");
                }

                // Wait for the thread to finish execution
                uint waitResult = WaitForSingleObject(threadHandle, 0xFFFFFFFF);
                if (waitResult != WAIT_OBJECT_0)
                {
                    throw new Exception("Thread execution failed.");
                }
            }
            finally
            {
                // Clean up
                VirtualFree(executableMemory, 0, 0x8000);
                VirtualFree(argumentsMemory, 0, 0x8000);
            }
        }
    }
}