using AyrA.IO;
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
    public static class CLI
    {
        private static volatile bool Cont;

        public static void AttachConsole()
        {
            if (!Terminal.AttachToConsole(Terminal.PARENT))
            {
                Terminal.CreateConsole();
            }
        }

        public static void DetachConsole()
        {
            Terminal.RemoveConsole();
        }

        public static void AbortImport()
        {
            Cont = false;
        }

        public static void Import(SQLiteConnection conn, string Filename)
        {
            Cont = true;
            int imported = 0;
            int skipped = 0;
            int error = 0;
            int total = 0;

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
                using (var GZS = new GZipStream(FS, CompressionMode.Decompress))
                {
                    using (var rdr = new StreamReader(GZS))
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
            }
            ShowCount(total, imported, skipped, error);
            Cont = false;
        }

        private static void ShowCount(int total, int imported, int skipped, int error)
        {
            var Pos = new {
                Left = Console.CursorLeft,
                Top = Console.CursorTop
            };
            Console.Error.WriteLine($"Total: {total}\nImported: {imported}\nSkipped: {skipped}\nError: {error}");
            Console.SetCursorPosition(Pos.Left, Pos.Top);
        }

        public static TorrentModel[] Search(SQLiteConnection conn, string SearchTerms)
        {
            return TorrentModel.SearchAsync(conn, TorrentModel.SearchType.ALL, SearchTerms).Result;
        }
    }
}
