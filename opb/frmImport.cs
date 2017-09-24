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
        private volatile SQLiteConnection conn;
        private volatile string Filename;
        private Thread T;
        private bool Cont = false;

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

        /*
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
        //*/

        private void ImportFile(object o)
        {
            string FileName = (string)o;
            Cont = true;
            int imported = 0;
            int skipped = 0;
            int error = 0;
            int total = 0;

            var Regex = new Regex(Program.REGEX);
            var HashList = Enumerable.Range(0, 256).Select(m => new List<string>()).ToArray();
            SetStatus("Reading existing entries...");
            var ExistingHashes = GetHashes().Result;
            foreach (var H in ExistingHashes)
            {
                var L = HashList[byte.Parse(H.Substring(0, 2), NumberStyles.HexNumber)];
                L.Add(H.ToUpper());
            }
            total = HashList.Sum(m => m.Count);
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
                                            Hash = Tools.ToHex(Match.Groups[2].Value).ToUpper(),
                                            Name = TorrentModel.HtmlDecode(Match.Groups[3].Value),
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
            if (Cont)
            {
                SetStatus("Import complete");
                Invoke((MethodInvoker)delegate () { btnCancel.Enabled = false; });
            }
            Cont = false;
        }

        private void ShowCount(int total, int imported, int skipped, int error)
        {
            SetStatus($"Total: {total} | Imported: {imported} | Skipped: {skipped} | Error: {error}");
        }

        private void SetStatus(string Message)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate {
                    SetStatus(Message);
                });
            }
            else
            {
                lblStatus.Text = Message;
            }
        }

        private void frmImport_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Cont && T.IsAlive)
            {
                if (e.CloseReason != CloseReason.UserClosing)
                {
                    Cont = false;
                    if (!T.Join(5000))
                    {
                        try
                        {
                            T.Abort();
                        }
                        catch
                        {
                        }
                    }
                }
                else if (MessageBox.Show("Abort current import?", "Abort import", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    Cont = false;
                    if (!T.Join(5000))
                    {
                        try
                        {
                            T.Abort();
                        }
                        catch
                        {
                            MessageBox.Show("Thread refused to exit and was forcefully aborted!");
                        }
                    }
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
            T = new Thread(ImportFile)
            {
                IsBackground = true,
                Name = "CSV Importer"
            };
            T.Start();
        }
    }
}
