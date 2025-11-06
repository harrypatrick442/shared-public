using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Checksums
{
    public static class Crc32
    {
        private const uint Poly = 0xEDB88320u;
        private const uint Init = 0xFFFFFFFFu;
        private const uint XorOut = 0xFFFFFFFFu;

        // Lazy-initialized table
        private static readonly Lazy<uint[]> s_table = new Lazy<uint[]>(MakeTable, isThreadSafe: true);

        private static uint[] MakeTable()
        {
            var t = new uint[256];
            for (uint i = 0; i < 256; ++i)
            {
                uint c = i;
                for (int k = 0; k < 8; ++k)
                {
                    c = ((c & 1u) != 0) ? (Poly ^ (c >> 1)) : (c >> 1);
                }
                t[i] = c;
            }
            return t;
        }

        /// <summary>Compute CRC32 for a ReadOnlySpan&lt;byte&gt;.</summary>
        public static uint Compute(ReadOnlySpan<byte> data, uint seed = Init)
        {
            uint crc = seed;
            var table = s_table.Value;
            foreach (byte b in data)
            {
                uint idx = (crc ^ b) & 0xFFu;
                crc = (crc >> 8) ^ table[idx];
            }
            return crc ^ XorOut;
        }

        /// <summary>Compute CRC32 for a byte[]</summary>
        public static uint Compute(byte[] data, uint seed = Init)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            return Compute((ReadOnlySpan<byte>)data, seed);
        }

        /// <summary>Compute CRC32 for an unmanaged struct/pod (works for blittable/unmanaged types).</summary>
        /// <remarks>Requires T to be unmanaged (C# 7.3+). This treats the struct's in-memory bytes as the input.</remarks>
        public static uint Compute<T>(in T value, uint seed = Init) where T : unmanaged
        {
            // Create a span over the struct bytes and compute CRC on that
            ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in value), 1));
            return Compute(bytes, seed);
        }

        /// <summary>Compute CRC32 for a string using UTF8 encoding (useful for textual config).</summary>
        public static uint ComputeUtf8(string s, uint seed = Init)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var bytes = System.Text.Encoding.UTF8.GetBytes(s);
            return Compute(bytes, seed);
        }
    }
}