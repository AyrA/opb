using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace opb
{
    public partial class frmMain : Form
    {
        private SQLiteConnection conn;

        private string QuickTorrent;

        public frmMain(SQLiteConnection conn)
        {
            this.conn = conn;
            using (var P = Process.GetCurrentProcess())
            {
                QuickTorrent = Path.GetDirectoryName(P.MainModule.FileName) + @"\QuickTorrent.exe";
            }
            InitializeComponent();
        }

        private void exitItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutItem_Click(object sender, EventArgs e)
        {
            (new frmAbout()).ShowDialog();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            Enabled = false;
            Search(tbSearch.Text);
            Enabled = true;
        }

        private void tbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && e.Modifiers == 0)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                Enabled = false;
                Search(tbSearch.Text);
                Enabled = true;
            }
        }

        private async void importDatabaseItem_Click(object sender, EventArgs e)
        {
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                (new frmImport(conn, OFD.FileName)).ShowDialog();
            }
            await GetCount();
        }

        private async void clearDatabaseItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Really delete all data from the database?", "Empty Database", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                TorrentModel.CreateTable(conn, true);
            }
            await GetCount();
        }

        private async void vacuumItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clean up and consolidate database? This can help speed up database functions after a large import.", "Vacuum Database", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "VACUUM";
                    Enabled = false;
                    await cmd.ExecuteNonQueryAsync();
                    Enabled = true;
                }
            }
        }

        private async void frmMain_Shown(object sender, EventArgs e)
        {
            await GetCount();
        }

        private void lvResults_DoubleClick(object sender, EventArgs e)
        {
            if (lvResults.SelectedItems.Count > 0)
            {
                var Item = ((TorrentModel)lvResults.SelectedItems[0].Tag);
                Launch(Item);
            }
        }

        private void lvResults_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.C:
                        e.Handled = e.SuppressKeyPress = true;
                        CopyItems();
                        break;
                    case Keys.A:
                        e.Handled = e.SuppressKeyPress = true;
                        lvResults.BeginUpdate();
                        lvResults.SuspendLayout();
                        lvResults.Items.OfType<ListViewItem>().ToList().ForEach(m => m.Selected = true);
                        lvResults.ResumeLayout();
                        lvResults.EndUpdate();
                        break;
                }
            }
            else if (e.KeyCode == Keys.Enter && lvResults.SelectedItems.Count > 0)
            {
                foreach (var Item in lvResults.SelectedItems.OfType<ListViewItem>().Select(m => (TorrentModel)m.Tag))
                {
                    Launch(Item);
                }
            }
        }

        private void copyItem_Click(object sender, EventArgs e)
        {
            CopyItems();
        }

        private void exportAllItem_Click(object sender, EventArgs e)
        {
            SetClipboard(
                string.Join("\r\n",
                lvResults.Items.OfType<ListViewItem>().Select(m => ((TorrentModel)m.Tag).Export())));
        }

        private void exportSelectedItem_Click(object sender, EventArgs e)
        {
            SetClipboard(
                string.Join("\r\n",
                lvResults.SelectedItems.OfType<ListViewItem>().Select(m => ((TorrentModel)m.Tag).Export())));
        }

        private async Task<int> GetCount()
        {
            lblEntryCount.Text = "Counting Entries";
            var Count = await TorrentModel.CountEntriesAsync(conn);
            if (Count >= 0)
            {
                lblEntryCount.Text = $"{Count} Entries";
            }
            else
            {
                MessageBox.Show($"The database is corrupt. Please delete {Program.DBNAME} and import your data again", "Database corruption", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            return Count;
        }

        private async void Search(string Content)
        {
            if (Content.Trim(Program.SPLITCHARS.ToCharArray()).Length > 0 && Content.Trim(Program.SPLITCHARS.ToCharArray()).Split(Program.SPLITCHARS.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length > 0)
            {
                var Items = await TorrentModel.SearchAsync(conn, TorrentModel.SearchType.ALL, Content);
                lvResults.BeginUpdate();
                lvResults.Items.Clear();
                foreach (var Entry in Items)
                {
                    var Item = lvResults.Items.Add(Entry.Name);
                    Item.Tag = Entry;
                    Item.SubItems.Add(Tools.Readable(Entry.Size));
                    Item.SubItems.Add(Entry.UploadDate.ToShortDateString());
                    Item.SubItems.Add(Entry.Hash);
                }
                lvResults.EndUpdate();
                lvResults.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                if (Items.Length > Program.MAXRESULTS)
                {
                    lblEntryCount.Text = $"More than {Program.MAXRESULTS} Entries found. List is truncated";
                }
                else
                {
                    lblEntryCount.Text = $"{Items.Length} Entries found";
                }
                lvResults.Select();
            }
        }

        private void Launch(TorrentModel Item)
        {
            //Try to send to an open QuickTorrent process first
            if (!SendViaPipe(Item.Hash))
            {
                if (File.Exists(QuickTorrent))
                {
                    using (var Proc = Process.Start(QuickTorrent, Item.Hash))
                    {
                        try
                        {
                            Proc.WaitForInputIdle(1000);
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    using (var Proc = Process.Start(Item.MagnetLink))
                    {
                        try
                        {
                            Proc.WaitForInputIdle(1000);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private void CopyItems()
        {
            SetClipboard(string.Join("\r\n", lvResults
                .SelectedItems
                .OfType<ListViewItem>()
                .Select(m => ((TorrentModel)m.Tag)
                .Hash)));
        }

        private void SetClipboard(string Text)
        {
            if (Text != null && Text.Trim() != string.Empty)
            {
                try
                {
                    Clipboard.SetText(Text);
                }
                catch
                {
                    MessageBox.Show("Error copying Entries to clipboard");
                }
            }
        }

        private bool SendViaPipe(string Content)
        {
            var Data = Encoding.UTF8.GetBytes(Content);
            try
            {
                using (var Sender = new NamedPipeClientStream(".", "QuickTorrent_AddHash", PipeDirection.Out))
                {
                    Sender.Connect(3000);
                    Sender.Write(BitConverter.GetBytes(Data.Length), 0, 4);
                    Sender.Write(Data, 0, Data.Length);
                    Sender.WaitForPipeDrain();
                    Sender.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "OPB_PIPE");
                return false;
            }
        }
    }
}
