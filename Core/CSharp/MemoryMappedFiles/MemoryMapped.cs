using System;
using System.Runtime.InteropServices;

namespace Core.MemoryMappedFiles
{
    public class MemoryMapped<TStructContent>:MemoryMappedBase where TStructContent:struct{
        public MemoryMapped(string path) :base(size: Marshal.SizeOf(typeof(TStructContent)),path){
        }
        public TStructContent Read() {
            if (_Disposed) throw new ObjectDisposedException(nameof(MemoryMapped<TStructContent>));
            TStructContent tStructContent;
            _MemoryMappedViewAccessor.Read<TStructContent>(0, out tStructContent);
            return tStructContent;
        }
        public void Write(TStructContent tStructContent)
        {
            if (_Disposed) throw new ObjectDisposedException(nameof(MemoryMapped));
            _MemoryMappedViewAccessor.Write<TStructContent>(0, ref tStructContent);
        }
    }

    public class MemoryMapped:MemoryMappedBase
    {
        public MemoryMapped(string path, int size):base(size: size,path)
        {
        }
        public byte[] Read()
        {
            if (_Disposed) throw new ObjectDisposedException(nameof(MemoryMapped));
            byte[] bytes = new byte[_Size];
            _MemoryMappedViewAccessor.ReadArray(0, bytes, 0, _Size);
            return bytes;
        }
        public void Write(byte[] bytes){
            if (_Disposed) throw new ObjectDisposedException(nameof(MemoryMapped));
            _MemoryMappedViewAccessor.WriteArray(0, bytes, 0, _Size);
        }
    }
}
