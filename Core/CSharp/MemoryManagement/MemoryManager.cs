using System;
using System.Collections.Generic;
using Core.Maths;
using Shutdown;
using Timer = System.Timers.Timer;
using System.Timers;

namespace Core.MemoryManagement
{
    public class MemoryManager:IDisposable
    {
        private List<IMemoryManaged> _MemoryManageds;
        private const int INTERVAL_CHECK_MILLISECONDS = 60 * 1000;
        private const float LOWER_BOUND_HYSTERESIS_WINDOW_AS_PROPORTION = 0.9f;
        private const float UPPER_BOUND_HYSTERESIS_WINDOW_AS_PROPORTION = 0.97f;
        private static MemoryManager _Instance;
        public static void Initialize(long bytesMemoryAllowed) {
            if (_Instance != null) return;
            _Instance = new MemoryManager(bytesMemoryAllowed);
        }
        public static MemoryManager Instance { get { return _Instance; } }
        private HysteresisWindowLong _HysteresisWindowMemoryBytes;
        private Timer _Timer;
        public void Add(IMemoryManaged memoryManaged) {
            _MemoryManageds.Add(memoryManaged);
        }
        protected MemoryManager(long bytesMemoryAllowed, params IMemoryManaged [] memoryManageds) {
            _HysteresisWindowMemoryBytes = new HysteresisWindowLong((long)(bytesMemoryAllowed * LOWER_BOUND_HYSTERESIS_WINDOW_AS_PROPORTION), upperBound:(long)(bytesMemoryAllowed * UPPER_BOUND_HYSTERESIS_WINDOW_AS_PROPORTION));
            _MemoryManageds = new List<IMemoryManaged>(memoryManageds);
            _Timer = new Timer(INTERVAL_CHECK_MILLISECONDS);
            _Timer.Enabled = true;
            _Timer.AutoReset = true;
            _Timer.Elapsed += CheckMemoryUseAndOverflowDatabases;
            _Timer.Start();
            ShutdownManager.Instance.Add(this, ShutdownOrder.MemoryManagement);
        }
        private void CheckMemoryUseAndOverflowDatabases(object sender, ElapsedEventArgs e) {
            long currentBytesMemoryUse = GC.GetTotalMemory(true);
            if (currentBytesMemoryUse < _HysteresisWindowMemoryBytes.UpperBound) return;
            float proportionReductionInMemoryRequired =  1-(_HysteresisWindowMemoryBytes.LowerBound / currentBytesMemoryUse);
            foreach (IMemoryManaged memoryManaged in _MemoryManageds) {
                memoryManaged.ReduceMemoryFootprintByProportion(proportionReductionInMemoryRequired, ShutdownManager.Instance.CancellationToken);
                if (ShutdownManager.Instance.CancellationToken.IsCancellationRequested) return;
            }
        }

        public void Dispose()
        {
            _Timer.Stop();
            _Timer.Dispose();
        }
    }
}