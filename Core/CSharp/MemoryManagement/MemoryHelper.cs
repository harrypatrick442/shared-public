using Core.Timing;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace Core.MemoryManagement
{

    public static class MemoryHelper
    {
        private const int DELAY_UPDATE_LATEST_MILLISECONDS= 10000;
        private static readonly object _LockObjectLatest = new object();
        private static MemoryMetrics _Latest;
        private static long _NextUpdateLatestMilliseconds;
        public static MemoryMetrics GetMemoryMetricsNow()
        {
            if (IsUnix())
                return GetUnixMetrics();
            return GetWindowsMetrics();
        }
        public static MemoryMetrics GetMemoryMetricsCached() {
            long now = TimeHelper.MillisecondsNow;
            lock (_LockObjectLatest)
            {
                if (_Latest != null&&(now < _NextUpdateLatestMilliseconds))
                    return _Latest;
                _Latest = GetMemoryMetricsNow();
                _NextUpdateLatestMilliseconds = now + DELAY_UPDATE_LATEST_MILLISECONDS;
                return _Latest;
            }
        }
        private static bool IsUnix()
        {
            var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            return isUnix;
        }

        private static MemoryMetrics GetWindowsMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo();
            info.FileName = "wmic";
            info.Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            var lines = output.Trim().Split("\n");
            var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
            var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

            int total = (int)Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);
            int free = (int)Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0);
            int used = total - free;
            return new MemoryMetrics(total, used, free);
        }

        private static MemoryMetrics GetUnixMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo("free -m");
            info.FileName = "/bin/bash";
            info.Arguments = "-c \"free -m\"";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
            }

            var lines = output.Split("\n");
            var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

            int total = int.Parse(memory[1]);
            int used = int.Parse(memory[2]);
            int free = int.Parse(memory[3]);
            return new MemoryMetrics(total, used, free);
        }
        public static GPUMemoryMetrics GetGPUMemoryMetrics()
        {
            return GpuMemoryInfoNVML.Instance.GetGpuMemoryInfo();
        }
    }
}