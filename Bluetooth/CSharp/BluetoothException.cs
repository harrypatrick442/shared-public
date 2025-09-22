namespace Bluetooth
{
    public class BluetoothException:Exception
    {
        public BluetoothFailedReason FailedReason { get; }
        public BluetoothException(BluetoothFailedReason failedReason)
            : this(failedReason, $"failedReason for reason {failedReason}")
        { 
            
        }
        public BluetoothException(BluetoothFailedReason failedReason, string message) : base(message)
        {
            FailedReason = failedReason;
        }
        public BluetoothException(BluetoothFailedReason failedReason, string message, Exception innerException) : base(message, innerException)
        {
            FailedReason = failedReason;
        }
    }
}
