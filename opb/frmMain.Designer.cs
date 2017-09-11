﻿namespace opb
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbSearch = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.lvResults = new System.Windows.Forms.ListView();
            this.chName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chHash = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.bottomStrip = new System.Windows.Forms.StatusStrip();
            this.lblEntryCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.topStrip = new System.Windows.Forms.MenuStrip();
            this.fileItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportResultItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitItem = new System.Windows.Forms.ToolStripMenuItem();
            this.databaseItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearDatabaseItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importDatabaseItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OFD = new System.Windows.Forms.OpenFileDialog();
            this.vacuumItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.bottomStrip.SuspendLayout();
            this.topStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbSearch
            // 
            this.tbSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSearch.Location = new System.Drawing.Point(12, 29);
            this.tbSearch.Name = "tbSearch";
            this.tbSearch.Size = new System.Drawing.Size(841, 20);
            this.tbSearch.TabIndex = 0;
            this.tbSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbSearch_KeyDown);
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Location = new System.Drawing.Point(859, 27);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 1;
            this.btnSearch.Text = "&Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // lvResults
            // 
            this.lvResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chName,
            this.chSize,
            this.chDate,
            this.chHash});
            this.lvResults.FullRowSelect = true;
            this.lvResults.Location = new System.Drawing.Point(12, 56);
            this.lvResults.Name = "lvResults";
            this.lvResults.Size = new System.Drawing.Size(922, 506);
            this.lvResults.TabIndex = 2;
            this.lvResults.UseCompatibleStateImageBehavior = false;
            this.lvResults.View = System.Windows.Forms.View.Details;
            this.lvResults.DoubleClick += new System.EventHandler(this.lvResults_DoubleClick);
            // 
            // chName
            // 
            this.chName.Text = "Name";
            this.chName.Width = 400;
            // 
            // chSize
            // 
            this.chSize.Text = "Size";
            this.chSize.Width = 80;
            // 
            // chHash
            // 
            this.chHash.Text = "Hash";
            this.chHash.Width = 300;
            // 
            // bottomStrip
            // 
            this.bottomStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblEntryCount});
            this.bottomStrip.Location = new System.Drawing.Point(0, 577);
            this.bottomStrip.Name = "bottomStrip";
            this.bottomStrip.Size = new System.Drawing.Size(946, 22);
            this.bottomStrip.TabIndex = 3;
            this.bottomStrip.Text = "statusStrip1";
            // 
            // lblEntryCount
            // 
            this.lblEntryCount.Name = "lblEntryCount";
            this.lblEntryCount.Size = new System.Drawing.Size(51, 17);
            this.lblEntryCount.Text = "0 Entries";
            // 
            // topStrip
            // 
            this.topStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileItem,
            this.databaseItem,
            this.aboutItem});
            this.topStrip.Location = new System.Drawing.Point(0, 0);
            this.topStrip.Name = "topStrip";
            this.topStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.topStrip.Size = new System.Drawing.Size(946, 24);
            this.topStrip.TabIndex = 4;
            // 
            // fileItem
            // 
            this.fileItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportResultItem,
            this.toolStripSeparator1,
            this.exitItem});
            this.fileItem.Name = "fileItem";
            this.fileItem.Size = new System.Drawing.Size(37, 20);
            this.fileItem.Text = "&File";
            // 
            // exportResultItem
            // 
            this.exportResultItem.Name = "exportResultItem";
            this.exportResultItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.exportResultItem.Size = new System.Drawing.Size(182, 22);
            this.exportResultItem.Text = "&Export Result";
            this.exportResultItem.ToolTipText = "Exports all currently shown entries";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(179, 6);
            // 
            // exitItem
            // 
            this.exitItem.Name = "exitItem";
            this.exitItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.exitItem.Size = new System.Drawing.Size(182, 22);
            this.exitItem.Text = "&Exit";
            this.exitItem.ToolTipText = "Exits the application";
            this.exitItem.Click += new System.EventHandler(this.exitItem_Click);
            // 
            // databaseItem
            // 
            this.databaseItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importDatabaseItem,
            this.vacuumItem,
            this.clearDatabaseItem});
            this.databaseItem.Name = "databaseItem";
            this.databaseItem.Size = new System.Drawing.Size(67, 20);
            this.databaseItem.Text = "&Database";
            // 
            // clearDatabaseItem
            // 
            this.clearDatabaseItem.Name = "clearDatabaseItem";
            this.clearDatabaseItem.Size = new System.Drawing.Size(152, 22);
            this.clearDatabaseItem.Text = "&Clear";
            this.clearDatabaseItem.ToolTipText = "Completely empties the database";
            this.clearDatabaseItem.Click += new System.EventHandler(this.clearDatabaseItem_Click);
            // 
            // importDatabaseItem
            // 
            this.importDatabaseItem.Name = "importDatabaseItem";
            this.importDatabaseItem.Size = new System.Drawing.Size(152, 22);
            this.importDatabaseItem.Text = "&Import";
            this.importDatabaseItem.ToolTipText = "Imports new data into the database";
            this.importDatabaseItem.Click += new System.EventHandler(this.importDatabaseItem_Click);
            // 
            // aboutItem
            // 
            this.aboutItem.Name = "aboutItem";
            this.aboutItem.Size = new System.Drawing.Size(52, 20);
            this.aboutItem.Text = "&About";
            this.aboutItem.ToolTipText = "About this application";
            this.aboutItem.Click += new System.EventHandler(this.aboutItem_Click);
            // 
            // OFD
            // 
            this.OFD.DefaultExt = "csv.gz";
            this.OFD.Filter = "Compressed Directory|*.csv.gz";
            this.OFD.SupportMultiDottedExtensions = true;
            this.OFD.Title = "Select Dump to import";
            // 
            // vacuumItem
            // 
            this.vacuumItem.Name = "vacuumItem";
            this.vacuumItem.Size = new System.Drawing.Size(152, 22);
            this.vacuumItem.Text = "&Vacuum";
            this.vacuumItem.ToolTipText = "Consolidates database to optimize speed";
            this.vacuumItem.Click += new System.EventHandler(this.vacuumItem_Click);
            // 
            // chDate
            // 
            this.chDate.Text = "Date";
            this.chDate.Width = 120;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(946, 599);
            this.Controls.Add(this.bottomStrip);
            this.Controls.Add(this.topStrip);
            this.Controls.Add(this.lvResults);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.tbSearch);
            this.MainMenuStrip = this.topStrip;
            this.Name = "frmMain";
            this.Text = "Offline Pirate Bay";
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.bottomStrip.ResumeLayout(false);
            this.bottomStrip.PerformLayout();
            this.topStrip.ResumeLayout(false);
            this.topStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbSearch;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.ListView lvResults;
        private System.Windows.Forms.StatusStrip bottomStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblEntryCount;
        private System.Windows.Forms.MenuStrip topStrip;
        private System.Windows.Forms.ToolStripMenuItem fileItem;
        private System.Windows.Forms.ToolStripMenuItem exportResultItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitItem;
        private System.Windows.Forms.ToolStripMenuItem databaseItem;
        private System.Windows.Forms.ToolStripMenuItem clearDatabaseItem;
        private System.Windows.Forms.ToolStripMenuItem importDatabaseItem;
        private System.Windows.Forms.ColumnHeader chName;
        private System.Windows.Forms.ColumnHeader chSize;
        private System.Windows.Forms.ColumnHeader chHash;
        private System.Windows.Forms.ToolStripMenuItem aboutItem;
        private System.Windows.Forms.OpenFileDialog OFD;
        private System.Windows.Forms.ToolStripMenuItem vacuumItem;
        private System.Windows.Forms.ColumnHeader chDate;
    }
}