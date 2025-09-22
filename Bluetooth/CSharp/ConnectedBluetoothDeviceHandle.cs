using InTheHand.Net.Sockets;
using Native.Messaging;
using System.Text;

namespace Bluetooth
{
    public class ConnectedBluetoothDeviceHandle
    {
        private CancellationTokenSource _CancellationTokenDisposed = new CancellationTokenSource();
        private BluetoothClient _BluetoothClient;
        private Stream _Stream;
        private StreamWriter _StreamWriter;
        private RegistrationMessageHandler _RegistrationMessageHandler;
        public ConnectedBluetoothDeviceHandle(
            BluetoothClient bluetoothClient,
            RegistrationMessageHandler registrationMessageHandler) {
            _BluetoothClient = bluetoothClient;
            _Stream = _BluetoothClient.GetStream();
            _StreamWriter = new StreamWriter(_Stream) { AutoFlush = true };
            _RegistrationMessageHandler = registrationMessageHandler;
            _RegistrationMessageHandler.SetSendRaw(SendRaw);
            StartReading(_Stream);
            Console.WriteLine("Connection closed.");
        }
        private void SendRaw(string message) {
            _StreamWriter.Write(message);
        }
        private async void StartReading(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

            while (!_CancellationTokenDisposed.IsCancellationRequested)
            {
                string? line;
                try
                {
                    line = await reader.ReadLineAsync().WaitAsync(_CancellationTokenDisposed.Token);
                }
                catch (OperationCanceledException)
                {
                    // Graceful exit
                    break;
                }
                catch (IOException ex)
                {
                    break;
                }
                if (line != null)
                {
                    Console.WriteLine("Received: " + line);
                    _RegistrationMessageHandler.HandleIncomingMessage(line);

                }
                else
                {
                    try
                    {
                        await Task.Delay(10, _CancellationTokenDisposed.Token); // Prevent tight loop on EOF
                    }
                    catch (TaskCanceledException) { 
                        
                    }
                }
            }
        }
    public string Read() {
            throw new NotImplementedException();
        }
        ~ConnectedBluetoothDeviceHandle()
        {
            Dispose();
        }
        public void Dispose() {
            if (_CancellationTokenDisposed.IsCancellationRequested) return;
            _CancellationTokenDisposed.Cancel();
            GC.SuppressFinalize(this);
            _Stream.Close();
            _BluetoothClient.Close();
            _BluetoothClient.Dispose();
        }
    }
}
