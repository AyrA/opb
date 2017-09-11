using System;
using System.Data.SQLite;
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
            using (var Conn = new SQLiteConnection($"Data Source={DBNAME}"))
            {
                Conn.Open();
                TorrentModel.CreateTable(Conn);
                Application.SetCompatibleTextRenderingDefault(false);
                Application.EnableVisualStyles();
                Application.Run(new frmMain(Conn));
            }
        }
    }
}
