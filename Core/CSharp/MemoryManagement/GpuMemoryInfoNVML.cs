
using ManagedCuda.Nvml;
using Shutdown;
using Initialization.Exceptions;
namespace Core.MemoryManagement

{
    public sealed class GpuMemoryInfoNVML
    {
        private static GpuMemoryInfoNVML _Instance;
        private nvmlDevice _DeviceHandle;
        private readonly object _LockObjectDispose = new object();
        private bool _Disposed = false;
        public static GpuMemoryInfoNVML Initialize() {
            if (_Instance != null) throw new AlreadyInitializedException(nameof(GpuMemoryInfoNVML));
            _Instance = new GpuMemoryInfoNVML();
            return _Instance;
        }
        public static GpuMemoryInfoNVML Instance { get {
                if (_Instance == null) throw new NotInitializedException(nameof(GPUMemoryMetrics));
                return _Instance;
            } }
        private GpuMemoryInfoNVML()
        {
            NvmlNativeMethods.nvmlInit();
            _DeviceHandle = new nvmlDevice();
            NvmlNativeMethods.nvmlDeviceGetHandleByIndex(0, ref _DeviceHandle);
            ShutdownManager.Instance.Add(Dispose, ShutdownOrder.Dispatcher);
        }
        public GPUMemoryMetrics GetGpuMemoryInfo()
        {
            nvmlMemory memoryInfo = new nvmlMemory();
            NvmlNativeMethods.nvmlDeviceGetMemoryInfo(_DeviceHandle, ref memoryInfo);
            return new GPUMemoryMetrics(memoryInfo.total, memoryInfo.free);
        }
        ~GpuMemoryInfoNVML()
        {
            Dispose();
        }
        public void Dispose()
        {
            lock (_LockObjectDispose)
            {
                if (_Disposed) return;
                _Disposed = true;
                NvmlNativeMethods.nvmlShutdown();
            }
        }
    }
}