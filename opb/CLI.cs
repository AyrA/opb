﻿using AyrA.IO;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace opb
{
    /// <summary>
    /// Command line utilities
    /// </summary>
    public static class CLI
    {
        /// <summary>
        /// This is used to abort the import
        /// </summary>
        /// <remarks>Currently unused on CLI since it doesn't makes any threads</remarks>
        private static volatile bool Cont;

        /// <summary>
        /// Attaches or creates a Console Window
        /// </summary>
        public static void AttachConsole()
        {
            if (!Terminal.AttachToConsole(Terminal.PARENT))
            {
                Terminal.CreateConsole();
            }
        }

        /// <summary>
        /// Detaches from the Console
        /// </summary>
        public static void DetachConsole()
        {
            Terminal.RemoveConsole();
        }

        /// <summary>
        /// Aborts an Import
        /// </summary>
        public static void AbortImport()
        {
            Cont = false;
        }

        /// <summary>
        /// Imports a file into the Database
        /// </summary>
        /// <param name="conn">Connection</param>
        /// <param name="Filename">File</param>
        /// <remarks>The file must be a gzip compressed CSV</remarks>
        public static void Import(SQLiteConnection conn, string Filename)
        {
            Cont = true;
            int imported = 0;
            int skipped = 0;
            int error = 0;
            int total = 0;
            bool compressed = false;

            var Regex = new Regex(Program.REGEX);
            var HashList = Enumerable.Range(0, 256).Select(m => new List<string>()).ToArray();
            Console.Error.WriteLine("Reading Existing Entries...");
            var ExistingHashes = TorrentModel.GetHashesAsync(conn).Result;
            foreach (var H in ExistingHashes)
            {
                var L = HashList[byte.Parse(H.Substring(0, 2), NumberStyles.HexNumber)];
                L.Add(H);
                ++total;
            }
            ShowCount(total, imported, skipped, error);

            using (var FS = File.OpenRead(Filename))
            {
                //gzip header is 10 bytes long at least
                if (FS.Length > 9)
                {
                    byte[] Magic = new byte[] { (byte)FS.ReadByte(), (byte)FS.ReadByte() };
                    compressed = Magic[0] == 0x1F && Magic[1] == 0x8B;
                    FS.Position = 0;
                }
                Stream Inner;
                if (compressed)
                {
                    //Disposing the StreamReader will also dispose the referenced stream.
                    //No need for a dispose of this
                    Inner = new GZipStream(FS, CompressionMode.Decompress);
                }
                else
                {
                    Inner = FS;
                }
                using (var rdr = new StreamReader(Inner))
                {
                    using (var Transaction = conn.BeginTransaction())
                    {
                        while (!rdr.EndOfStream && Cont)
                        {
                            var Line = rdr.ReadLine().Trim();
                            if (!Line.StartsWith("#"))
                            {
                                var Match = Regex.Match(Line);
                                if (Match != null && Match.Length > 0)
                                {
                                    var Model = new TorrentModel()
                                    {
                                        UploadDate = DateTime.Parse(Match.Groups[1].Value),
                                        Hash = Tools.ToHex(Match.Groups[2].Value),
                                        Name = Match.Groups[3].Value,
                                        Size = Tools.LongOrDefault(Match.Groups[4].Value)
                                    };
                                    var L = HashList[byte.Parse(Model.Hash.Substring(0, 2), NumberStyles.HexNumber)];
                                    if (Model.Size >= 0)
                                    {
                                        if (!L.Contains(Model.Hash))
                                        {
                                            Model.Save(conn);
                                            L.Add(Model.Hash);
                                            ++imported;
                                        }
                                        else
                                        {
                                            ++skipped;
                                        }
                                    }
                                    else
                                    {
                                        ++error;
                                    }
                                    if (++total % 500 == 0)
                                    {
                                        ShowCount(total, imported, skipped, error);
                                    }
                                }
                            }
                        }
                        Transaction.Commit();
                    }
                }
            }
            ShowCount(total, imported, skipped, error);
            Cont = false;
        }

        /// <summary>
        /// Shows counts on the CLI
        /// </summary>
        /// <param name="total">Total records processed</param>
        /// <param name="imported">Records imported</param>
        /// <param name="skipped">Records skipped (duplicates)</param>
        /// <param name="error">Records failed to parse/import</param>
        private static void ShowCount(int total, int imported, int skipped, int error)
        {
            var Pos = new
            {
                Left = Console.CursorLeft,
                Top = Console.CursorTop
            };
            Console.Error.WriteLine($"Total: {total}\nImported: {imported}\nSkipped: {skipped}\nError: {error}");
            Console.SetCursorPosition(Pos.Left, Pos.Top);
        }

        /// <summary>
        /// Searches for Torrents
        /// </summary>
        /// <param name="conn">Connections</param>
        /// <param name="SearchTerms">ALL/OR Search</param>
        /// <returns>List of Torrents found</returns>
        public static TorrentModel[] Search(SQLiteConnection conn, string SearchTerms)
        {
            return TorrentModel.SearchAsync(conn, TorrentModel.SearchType.ALL, SearchTerms).Result;
        }
    }
}
