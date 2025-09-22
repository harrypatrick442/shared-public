using Logging;
using Core.Exceptions;
using Core.Timing;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Core.MemoryManagement
{

    public sealed class ProcessorMetricsSource
    {
        private static ProcessorMetricsSource _Instance;
        public static ProcessorMetricsSource Initialize()
        {
            if (_Instance != null) throw new AlreadyInitializedException(nameof(ProcessorMetricsSource));
            _Instance = new ProcessorMetricsSource();
            return _Instance;
        }
        public static ProcessorMetricsSource Instance
        {
            get
            {
                if (_Instance == null) throw new NotInitializedException(nameof(ProcessorMetricsSource));
                return _Instance;
            }
        }
        private ProcessorMetrics _Latest;
        private CancellationTokenSource _CancellationTokenSourceDisposed = new CancellationTokenSource();
        private int _NDelays;
        private int _SubDelayMilliseconds;
        private ProcessorMetricsSource()
        {
            if (GlobalConstants.Delays.PROCESSOR_METRICS_MIN_DELAY_UPDATE_LATEST_MILLISECONDS <= GlobalConstants.Delays.MAX_SUB_DELAY_MILLISECONDS)
            {
                _NDelays = 1;
                _SubDelayMilliseconds = GlobalConstants.Delays.PROCESSOR_METRICS_MIN_DELAY_UPDATE_LATEST_MILLISECONDS;
            }
            else
            {
                _NDelays = (int)Math.Ceiling((double)GlobalConstants.Delays.PROCESSOR_METRICS_MIN_DELAY_UPDATE_LATEST_MILLISECONDS
                    / GlobalConstants.Delays.MAX_SUB_DELAY_MILLISECONDS);
                _SubDelayMilliseconds = GlobalConstants.Delays.PROCESSOR_METRICS_MIN_DELAY_UPDATE_LATEST_MILLISECONDS / _NDelays;
            }
            _Latest = new ProcessorMetrics(0, 50);
            StartUpdateLooper();
        }
        public ProcessorMetrics GetProcessorMetricsNow()
        {
            return new ProcessorMetrics(GetPercentCpuUsageByMe(),
                GetPercentCpuUsageByAllProcesses());
        }
        public ProcessorMetrics GetProcessorMetricsCached()
        {
            lock (this)
            {
                return _Latest;
            }
        }
        private void StartUpdateLooper()
        {
            new Thread(() =>
            {
                while (true)
                {
                    if (_CancellationTokenSourceDisposed.IsCancellationRequested)
                        return;
                    try
                    {
                        Update();
                    }
                    catch (Exception ex)
                    {
                        Logs.Default.Error(ex);
                    }
                    for (int i = 0; i < _NDelays; i++)
                    {
                        if (_CancellationTokenSourceDisposed.IsCancellationRequested)
                            return;
                        Thread.Sleep(_SubDelayMilliseconds);
                    }
                }
            }).Start();
        }
        private void Update() {
            try
            {
                ProcessorMetrics latest = GetProcessorMetricsNow();
                lock (this)
                {
                    _Latest = latest;
                }
            }
            catch (Exception ex)
            {
                Logs.Default.Error(ex);
            }
        }
        public double GetPercentCpuUsageByAllProcesses()
        {
            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
              //  return GetPercentCpuUsageByAllProcesses_Linux();
            return _GetCpuUsageForProcesses(() => Process.GetProcesses());
        }
        public int GetPercentCpuUsageByMe()
        {
            return _GetCpuUsageForProcesses(()=> new Process[] { Process.GetCurrentProcess() });
        }
        public int _GetCpuUsageForProcesses(Func<Process[]> getProcesses)
        {
            // refer to https://stackoverflow.com/questions/59465212/net-core-cpu-usage-for-machine
            long startMilliseconds = TimeHelper.MillisecondsNow;
            var startCpuUsedMilliseconds = getProcesses().Sum(a =>
            {
                try
                {
                    return a.TotalProcessorTime.TotalMilliseconds;
                }
                catch { return 0; }
            });

            System.Threading.Thread.Sleep(GlobalConstants.Delays.PROCESSOR_METRICS_DELAY_TOTAL_PROCESSOR_TIME_MILLISECONDS);

            long endMilliseconds = TimeHelper.MillisecondsNow;
            double endCpuUsedMilliseconds = getProcesses().Sum(a =>
            {
                try
                {
                    return a.TotalProcessorTime.TotalMilliseconds;
                }
                catch { return 0; }
            });

            double cpuUsedMilliseconds = endCpuUsedMilliseconds - startCpuUsedMilliseconds;
            double totalMillisecondsPassed = endMilliseconds - startMilliseconds;
            double cpuUsageTotal = cpuUsedMilliseconds / (Environment.ProcessorCount * totalMillisecondsPassed);

            return (int)Math.Round(cpuUsageTotal * 100);
        }
        public double GetPercentCpuUsageByAllProcesses_Windows() {
            return 0;
        }
        public static double GetPercentCpuUsageByAllProcesses_Linux()
        {
            var output = "";

            var info = new ProcessStartInfo("top -n 1");
            info.FileName = "/bin/bash";
            info.Arguments = "-c \"top  -n 1\"";// "-c \"top\"";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }
            var lines = output.Split("\n");
            string entryInLIne =  lines[2]
                .Split(new char[] { ':', ','},
                StringSplitOptions.RemoveEmptyEntries)[4];
            string percentIdle = entryInLIne.Split(" ")[0];
            string percentString = percentIdle.Trim();
            return double.Parse(percentString);
        }
    }
}