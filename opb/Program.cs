using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace opb
{
    class Program
    {
        private static string _dbname;

        /// <summary>
        /// Chars we use to split search terms
        /// </summary>
        public const string SPLITCHARS = "\t \r\n,.-'\"(){}[]/\\+%";
        /// <summary>
        /// Regex to match one line of CSV
        /// </summary>
        /// <remarks>Individual Years have categories with their torrents which we ignore</remarks>
        public const string REGEX = "^(.*);(.*);\"(.*)\";(\\d*)(;\\d*)?$";
        /// <summary>
        /// Maximum number of results to return
        /// </summary>
        public const int MAXRESULTS = 1000;

        /// <summary>
        /// Name of the database file
        /// </summary>
        public static string DBNAME
        {
            get
            {
                if (string.IsNullOrEmpty(_dbname))
                {
                    using (var P = Process.GetCurrentProcess())
                    {
                        _dbname = Path.Combine(Path.GetDirectoryName(P.MainModule.FileName), "OPB.DB3");
                    }
                }
                return _dbname;
            }
        }

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <remarks>This switches to CLI mode if <paramref name="args"/> is not empty</remarks>
        [STAThread]
        static void Main(string[] args)
        {
            //Show help if requested
            if (args.Contains("/?") || args.Contains("--help") || args.Contains("-?"))
            {
                ShowHelp();
            }
            else
            {
                //Open the connection right on the start and keep open until we close the application.
                using (var conn = new SQLiteConnection($"Data Source={DBNAME}"))
                {
                    conn.Open();
                    //Create tables if not exist
                    TorrentModel.CreateTable(conn);
                    if (args.Length > 0)
                    {
                        //CLI Mode
                        CLI.AttachConsole();
                        //Import a file via CLI
                        if (args.Length == 2 && args[0].ToUpper() == "/I")
                        {
                            if (File.Exists(args[1]))
                            {
                                CLI.Import(conn, args[1]);
                            }
                            else
                            {
                                Console.Error.WriteLine("Import Failed: File does not exists");
                            }
                        }
                        //Search for Torrents via CLI
                        else if (args.Length > 1)
                        {
                            if (args[0].ToUpper() == "/S" || (args[0].ToUpper() == "/H" && args[1].ToUpper() == "/S"))
                            {
                                var Results = CLI.Search(conn,
                                    string.Join(" ", args.Skip(args[0].ToUpper() == "/S" ? 1 : 2).Select(m => m.Trim()).ToArray())
                                )
                                .Select(m => args[0].ToUpper() == "/S" ? string.Format("{1}\t{0}", m.Name, m.Hash) : m.Hash);
                                foreach (var Result in Results)
                                {
                                    Console.WriteLine(Result);
                                }
                            }
                            else
                            {
                                Console.Error.WriteLine("Invalid command line arguments. Use /? for Help");
                            }
                        }
#if DEBUG
                        Console.Error.WriteLine("#END");
                        Console.ReadKey(true);
#endif
                    }
                    else
                    {
                        //Launch in UI mode without Arguments
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.EnableVisualStyles();
                        Application.Run(new frmMain(conn));
                    }
                }
            }
        }

        /// <summary>
        /// Shows Help on CLI
        /// </summary>
        private static void ShowHelp()
        {
            Console.Error.WriteLine(@"{0} [/I <file> | [/H] /S <search>]

Offline Pirate Bay Console API

/I      - Imports the given .csv.gz file
File    - File name to read
/H      - Show hashes only, must be before /S
/S      - Searches for the given Keywords
Search  - Keywords to search. Surrounding with quotes is recommended", Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));
        }
    }
}
