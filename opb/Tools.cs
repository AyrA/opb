using System;
using System.Linq;
using System.Windows.Forms;

namespace opb
{
    public static class Tools
    {
        public static string ToHex(string value)
        {
            var bytes = string.Concat(Convert.FromBase64String(value).Select(m => m.ToString("X2")));
            return bytes;
        }

        public static long LongOrDefault(string value)
        {
            long l = -1;
            return long.TryParse(value, out l) ? l : -1;
        }

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
