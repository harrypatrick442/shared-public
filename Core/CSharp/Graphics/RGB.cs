using System;
using System.Runtime.InteropServices;

namespace Core.Graphics
{
    public class RGB
    {
        private byte _R, _G, _B;
        public RGB(byte r, byte g, byte b) {
            _R = r;
            _G = g;
            _B = b;
        }
        public RGB(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("Colour hash cannot be null or empty", nameof(hash));

            // Remove leading '#' if present
            if (hash.StartsWith("#"))
                hash = hash.Substring(1);

            // Only allow 3 or 6 hex characters
            if (hash.Length != 3 && hash.Length != 6)
                throw new ArgumentException(
                    $"Invalid colour hash length '{hash.Length}'. Expected 3 or 6 characters.",
                    nameof(hash));

            // Validate hex characters only
            for (int i = 0; i < hash.Length; i++)
            {
                char c = hash[i];
                bool isHex = (c >= '0' && c <= '9') ||
                             (c >= 'A' && c <= 'F') ||
                             (c >= 'a' && c <= 'f');
                if (!isHex)
                    throw new ArgumentException(
                        $"Invalid character '{c}' in colour hash. Must be hexadecimal.",
                        nameof(hash));
            }

            if (hash.Length == 3)
            {
                // Expand short form #RGB → #RRGGBB
                string rr = new string(hash[0], 2);
                string gg = new string(hash[1], 2);
                string bb = new string(hash[2], 2);

                _R = Convert.ToByte(rr, 16);
                _G = Convert.ToByte(gg, 16);
                _B = Convert.ToByte(bb, 16);
            }
            else // length == 6
            {
                _R = Convert.ToByte(hash.Substring(0, 2), 16);
                _G = Convert.ToByte(hash.Substring(2, 2), 16);
                _B = Convert.ToByte(hash.Substring(4, 2), 16);
            }
        }

        public UInt32 ToUInt32()
        {
            return (UInt32)(_R | (_G << 8) | (_B << 16));
        }
        public static RGB FromUInt32(UInt32 pixel)
        {
            byte r = (byte)(pixel & 0xFF);
            byte g = (byte)((pixel >> 8) & 0xFF);
            byte b = (byte)((pixel >> 16) & 0xFF);
            return new RGB(r, g, b);
        }
    }
}
