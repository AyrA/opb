using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Linq;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace opb
{
    public partial class frmImport : Form
    {
        private SQLiteConnection conn;
        private string Filename;
        private bool Cont = false;
        private bool Working = false;

        public frmImport(SQLiteConnection conn, string Filename)
        {
            this.Filename = Filename;
            this.conn = conn;
            InitializeComponent();
        }

        private async Task<string[]> GetHashes()
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Hash from Torrents";
                using (var rdr = await cmd.ExecuteReaderAsync())
                {
                    var Ret = new List<string>();
                    while (await rdr.ReadAsync())
                    {
                        Ret.Add(rdr.GetString(0).ToLower());
                    }
                    return Ret.ToArray();
                }
            }
        }

        private async void Import()
        {
            Working = Cont = true;
            int imported = 0;
            int skipped = 0;
            int error = 0;
            int total = 0;

            var Regex = new Regex(Program.REGEX);
            var HashList = Enumerable.Range(0, 256).Select(m => new List<string>()).ToArray();
            lblStatus.Text = "Reading existing entries...";
            var ExistingHashes = await GetHashes();
            foreach (var H in ExistingHashes)
            {
                var L = HashList[byte.Parse(H.Substring(0, 2), NumberStyles.HexNumber)];
                L.Add(H.ToUpper());
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
                                var Line = (await rdr.ReadLineAsync()).Trim();
                                if (!Line.StartsWith("#"))
                                {
                                    var Match = Regex.Match(Line);
                                    if (Match != null && Match.Length > 0)
                                    {
                                        var Model = new TorrentModel()
                                        {
                                            UploadDate = DateTime.Parse(Match.Groups[1].Value),
                                            Hash = Tools.ToHex(Match.Groups[2].Value).ToUpper(),
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
            if (Cont)
            {
                Close();
            }
            Working = Cont = false;
        }

        private void ShowCount(int total, int imported, int skipped, int error)
        {
            lblStatus.Text = $"Total: {total} | Imported: {imported} | Skipped: {skipped} | Error: {error}";
        }

        private void frmImport_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Cont)
            {
                if (e.CloseReason != CloseReason.UserClosing || MessageBox.Show("Abort current import?", "Abort import", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    Cont = false;
                    /*
                    while (Working)
                    {
                        Thread.Sleep(100);
                    }
                    //*/
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frmImport_Shown(object sender, EventArgs e)
        {
            Import();
        }
    }
}
