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

        private void ImportFile(object o)
        {
            string FileName = (string)o;
            Cont = true;
            int imported = 0;
            int skipped = 0;
            int error = 0;
            int total = 0;
            bool compressed = false;
            var Regex = new Regex(Program.REGEX);
            var HashList = Enumerable.Range(0, 256).Select(m => new List<string>()).ToArray();
            SetStatus("Reading existing entries...", 0.0);
            var ExistingHashes = GetHashes().Result;
            foreach (var H in ExistingHashes)
            {
                var L = HashList[byte.Parse(H.Substring(0, 2), NumberStyles.HexNumber)];
                L.Add(H.ToUpper());
            }
            ShowCount(total, imported, skipped, error, 0.0);

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
                            ++total;
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
                                    if (Model.Size >= 0 && Model.Hash.Length == 40)
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
#if DEBUG
                                        throw new Exception($"Invalid Model: {Model.Hash} ({Match.Groups[2].Value})");
#endif
                                    }
                                    if (total % 500 == 0)
                                    {
                                        ShowCount(total, imported, skipped, error, FS.Position * 1.0 / FS.Length);
                                    }
                                }
                                else
                                {
                                    ++error;
                                }
                            }
                        }
                        Transaction.Commit();
                    }
                }
            }
            if (Cont)
            {
                SetStatus("Import complete", 1.0);
                Invoke((MethodInvoker)delegate ()
                {
                    Cont = false;
                    MessageBox.Show($"Import is complete. Result:\r\n{total} processed Entries\r\n{imported} new Torrents", "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnCancel.Enabled = false;
                    Close();
                });
            }
            Cont = false;
        }

        private void ShowCount(int TotalEntries, int ImportedEntries, int SkippedEntries, int ErrorCount, double PercFactor)
        {
            SetStatus($"Total: {TotalEntries} | Imported: {ImportedEntries} | Skipped: {SkippedEntries} | Error: {ErrorCount}", PercFactor);
        }

        private void SetStatus(string Message, double PercFactor)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    SetStatus(Message, PercFactor);
                });
            }
            else
            {
                lblStatus.Text = Message;
                PercFactor = Math.Min(Math.Max(PercFactor, 0.0), 1.0);
                pbStatus.Value = (int)(PercFactor * pbStatus.Maximum);
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
                    lblStatus.Text = "Aborting...";
                    btnCancel.Enabled = false;
                    Cont = false;
                    if (!T.Join(5000))
                    {
                        try
                        {
                            T.Abort();
                        }
                        catch
                        {
                            MessageBox.Show("Thread refused to exit and was forcefully aborted!\r\nAn Application restart is recommended.", "Thread not responding", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
