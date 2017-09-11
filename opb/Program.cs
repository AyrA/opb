using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace opb
{
    class Program
    {
        //Regex to match one line
        public const string REGEX = "^(.*);(.*);\"(.*)\";(\\d*)(;\\d*)?$";
        public const string DBNAME = "OPB.db3";
        public const string SPLITCHARS = "\t \r\n,.-'\"(){}[]/\\+%";

        [STAThread]
        static void Main(string[] args)
        {
#if DEBUG
            args = new string[] { "/S", "GTA", "vice" };
#endif
            if (args.Contains("/?") || args.Contains("--help") || args.Contains("-?"))
            {
                ShowHelp();
            }
            else
            {
                using (var conn = new SQLiteConnection($"Data Source={DBNAME}"))
                {
                    conn.Open();
                    TorrentModel.CreateTable(conn);
                    if (args.Length > 0)
                    {
                        //CLI Mode
                        CLI.AttachConsole();
                        //Import a file
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
