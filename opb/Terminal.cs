using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;

namespace AyrA.IO
{
    /// <summary>
    /// Provides functions to work with consoles
    /// </summary>
    public static class Terminal
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        /// <summary>
        /// For AttachToConsole, to attach to the parent process console
        /// </summary>
        public const int PARENT = -1;

        /// <summary>
        /// Returns all console colors sorted from 0x0 to 0xF
        /// </summary>
        public static ConsoleColor[] ColorRow
        {
            get
            {
                ConsoleColor[] c = new ConsoleColor[16];
                for (int i = 0; i < 16; i++)
                {
                    c[i] = (ConsoleColor)i;
                }
                return c;
            }
        }

        /// <summary>
        /// creates a new console, an application can only have one
        /// </summary>
        /// <returns>true, if created</returns>
        public static bool CreateConsole()
        {
            return AllocConsole();
        }

        /// <summary>
        /// Removes your application from the console handle.
        /// If you are the last application to have a handle,
        /// the console is closed
        /// </summary>
        /// <returns>true, on success</returns>
        public static bool RemoveConsole()
        {
            return FreeConsole();
        }

        /// <summary>
        /// Attaches your application to a given processes console
        /// </summary>
        /// <param name="P">Process to attach to. null to attach to parent process</param>
        /// <returns>true, if success</returns>
        public static bool AttachToConsole(Process P)
        {
            return AttachToConsole(P == null ? PARENT : P.Id);
        }

        /// <summary>
        /// Attach to a console of the given process ID
        /// </summary>
        /// <param name="ID">process ID, or PARENT constant</param>
        /// <returns>true, on success</returns>
        public static bool AttachToConsole(int ID)
        {
            return AttachConsole(ID);
        }

        /// <summary>
        /// Writes a line with colored text and adds a CRLF
        /// </summary>
        /// <param name="text">Text to be written</param>
        /// <param name="mapF">Foreground color map</param>
        /// <param name="mapB">Background color Map</param>
        public static void printColorL(string text, string mapF, string mapB)
        {
            printColor(text, mapF, mapB);
            System.Console.WriteLine();
        }

        /// <summary>
        /// Writes a single line with colored text
        /// </summary>
        /// <param name="text">Line to write</param>
        /// <param name="mapF">Foreground color map</param>
        /// <param name="mapB">Background color Map</param>
        public static void printColor(string text, string mapF, string mapB)
        {
            int color = 0;

            mapF = mapF.ToUpper().Replace(' ', '_');
            mapB = mapB.ToUpper().Replace(' ', '_');

            if (text.Length != mapF.Length || mapF.Length != mapB.Length)
            {
                throw new Exception("All params must be of the same length");
            }

            for (int i = 0; i < text.Length; i++)
            {
                if (mapF[i] != '_')
                {
                    if (!int.TryParse(mapF.Substring(i, 1), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out color))
                    {
                        throw new Exception(string.Format("Foreground color at pos {0} is invalid. Is: '{1}'", i, mapF[i]));
                    }
                }
                if (mapB[i] != '_')
                {
                    if (!int.TryParse(mapB.Substring(i, 1), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out color))
                    {
                        throw new Exception(string.Format("Background color at pos {0} is invalid. Is: '{1}'", i, mapB[i]));
                    }
                }
            }


            ConsoleColor[] C = new ConsoleColor[] { System.Console.ForegroundColor, System.Console.BackgroundColor };
            for (int i = 0; i < text.Length; i++)
            {
                if (mapF[i] != '_')
                {
                    System.Console.ForegroundColor = (ConsoleColor)int.Parse(mapF.Substring(i, 1), NumberStyles.HexNumber);
                }
                if (mapB[i] != '_')
                {
                    System.Console.BackgroundColor = (ConsoleColor)int.Parse(mapB.Substring(i, 1), NumberStyles.HexNumber);
                }
                System.Console.Write(text[i]);
            }
            System.Console.ForegroundColor = C[0];
            System.Console.BackgroundColor = C[1];
        }
    }
}
