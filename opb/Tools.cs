using System;
using System.Linq;

namespace opb
{
    /// <summary>
    /// Generic Functions
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Converts a base64 String to Hex
        /// </summary>
        /// <param name="Value">Base64 string</param>
        /// <returns>Hex string</returns>
        public static string ToHex(string Value)
        {
            var bytes = string.Concat(Convert.FromBase64String(Value).Select(m => m.ToString("X2")));
            return bytes;
        }

        /// <summary>
        /// Returns a long from a string
        /// </summary>
        /// <param name="Value">String to test</param>
        /// <returns>Parsed value, or -1 on error</returns>
        public static long LongOrDefault(string Value, long Default = -1)
        {
            long l = Default;
            return long.TryParse(Value, out l) ? l : Default;
        }

        /// <summary>
        /// Turns a size into readable units
        /// </summary>
        /// <param name="Size">Size in Bytes</param>
        /// <returns>Readable Size</returns>
        public static string Readable(long Size, int Decimals = 1)
        {
            const double FACTOR = 1024.0;
            var Sizes = "Bytes,KB,MB,GB,TB".Split(',');
            int CurrentFactor = 0;
            double NewSize = Size;
            while (CurrentFactor < Sizes.Length - 1 && NewSize >= FACTOR)
            {
                ++CurrentFactor;
                NewSize /= FACTOR;
            }
            return $"{Math.Round(NewSize, Decimals)} {Sizes[CurrentFactor]}";
        }
    }
}
