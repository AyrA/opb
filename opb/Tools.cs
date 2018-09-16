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
        /// <param name="value">Base64 string</param>
        /// <returns>Hex string</returns>
        public static string ToHex(string value)
        {
            var bytes = string.Concat(Convert.FromBase64String(value).Select(m => m.ToString("X2")));
            return bytes;
        }

        /// <summary>
        /// Returns a long from a string
        /// </summary>
        /// <param name="value">String to test</param>
        /// <returns>Parsed value, or -1 on error</returns>
        public static long LongOrDefault(string value)
        {
            long l = -1;
            return long.TryParse(value, out l) ? l : -1;
        }

        /// <summary>
        /// Turns a size into readable units
        /// </summary>
        /// <param name="size">Size in Bytes</param>
        /// <returns>Readable Size</returns>
        public static string Readable(long size)
        {
            const double FACTOR = 1024.0;
            var Sizes = "Bytes,KB,MB,GB,TB".Split(',');
            int current = 0;
            double NewSize = size;
            while (current < Sizes.Length - 1 && NewSize >= FACTOR)
            {
                ++current;
                NewSize /= FACTOR;
            }
            return $"{Math.Round(NewSize, 1)} {Sizes[current]}";
        }
    }
}
