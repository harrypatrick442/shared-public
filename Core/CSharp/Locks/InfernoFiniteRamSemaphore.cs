using Core.MemoryManagement;
using Core.Strings;
using InfernoDispatcher.Locking;
using Logging;
using System;
namespace Core.Locks
{
    public class InfernoFiniteRamSemaphore : InfernoFiniteResourceSemaphore
    {
        private double _ProportionFreeRamToUse;
        public InfernoFiniteRamSemaphore(double proportionFreeRamToUse)
            : base((long)(proportionFreeRamToUse * MemoryHelper.GetMemoryMetricsNow().Free)) {
            _ProportionFreeRamToUse = proportionFreeRamToUse;
        }
        protected override void OnException(Exception ex, long nResourceThis)
        {
            /*lock (_LockObject) {
                MemoryMetrics memoryMetrics = MemoryHelper.GetMemoryMetricsNow();
                long nActuallyTaken = _NTaken - nResourceThis;
                if(nActuallyTaken<0)nActuallyTaken = 0;//Just incase
                long nLeftToTake = _MaxN - nActuallyTaken;
                long freeRamCanTake = memoryMetrics.Free;
                if (nLeftToTake > freeRamCanTake) { 
                    long newMaxN = nActuallyTaken+ freeRamCanTake;
                    if (newMaxN <= 0) {
                        Logs.Default.Error($"Something went very wrong in {nameof(InfernoFiniteRamSemaphore)} recalculating {nameof(_MaxN)}");
                        return;
                    }
                    _MaxN = newMaxN;
                }
            }*/
        }
    }
}