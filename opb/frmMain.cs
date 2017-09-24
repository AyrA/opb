using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace opb
{
    public partial class frmMain : Form
    {
        private SQLiteConnection conn;

        public frmMain(SQLiteConnection conn)
        {
            this.conn = conn;
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
                var Hash = lvResults.SelectedItems[0].SubItems[lvResults.SelectedItems[0].SubItems.Count - 1].Text;

                Process.Start($"magnet:?xt=urn:btih:{Hash}&dn={Uri.EscapeDataString(lvResults.SelectedItems[0].Text)}");
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
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyItems();
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

        private void CopyItems()
        {
            StringBuilder SB = new StringBuilder();
            foreach (var item in lvResults.SelectedItems)
            {
                var I = (ListViewItem)item;
                SB.AppendLine(I.SubItems[I.SubItems.Count - 1].Text);
            }
            try
            {
                Clipboard.SetText(SB.ToString());
            }
            catch
            {
                MessageBox.Show("Error copying Entries to clipboard");
            }
        }
    }
}
