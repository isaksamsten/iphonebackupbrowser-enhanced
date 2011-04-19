namespace IDevice
{
    partial class BackupBrowser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BackupBrowser));
            this.backupSelect = new System.Windows.Forms.ComboBox();
            this.fileList = new System.Windows.Forms.ListView();
            this.folderList = new System.Windows.Forms.ListView();
            this.openFolder = new System.Windows.Forms.Button();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolShowBtn = new System.Windows.Forms.ToolStripButton();
            this.toolExportBtn = new System.Windows.Forms.ToolStripButton();
            this.toolExportProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.searchBox = new System.Windows.Forms.ToolStripTextBox();
            this.sideLabelLbl = new System.Windows.Forms.Label();
            this.appSearchTxt = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.exportMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.analyzeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.locationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mediaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contactsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // backupSelect
            // 
            this.backupSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.backupSelect.FormattingEnabled = true;
            this.backupSelect.Location = new System.Drawing.Point(99, 27);
            this.backupSelect.Name = "backupSelect";
            this.backupSelect.Size = new System.Drawing.Size(275, 21);
            this.backupSelect.TabIndex = 0;
            this.backupSelect.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // fileList
            // 
            this.fileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.fileList.Location = new System.Drawing.Point(12, 279);
            this.fileList.Name = "fileList";
            this.fileList.Size = new System.Drawing.Size(670, 274);
            this.fileList.TabIndex = 6;
            this.fileList.UseCompatibleStateImageBehavior = false;
            this.fileList.View = System.Windows.Forms.View.Details;
            this.fileList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView2_ColumnClick);
            this.fileList.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listView2_ItemDrag);
            this.fileList.SelectedIndexChanged += new System.EventHandler(this.listView2_SelectedIndexChanged);
            this.fileList.DoubleClick += new System.EventHandler(this.toolShowBtn_Click);
            // 
            // folderList
            // 
            this.folderList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.folderList.FullRowSelect = true;
            this.folderList.Location = new System.Drawing.Point(12, 54);
            this.folderList.Name = "folderList";
            this.folderList.Size = new System.Drawing.Size(670, 190);
            this.folderList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.folderList.TabIndex = 7;
            this.folderList.UseCompatibleStateImageBehavior = false;
            this.folderList.View = System.Windows.Forms.View.Details;
            this.folderList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
            this.folderList.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // openFolder
            // 
            this.openFolder.Location = new System.Drawing.Point(380, 26);
            this.openFolder.Name = "openFolder";
            this.openFolder.Size = new System.Drawing.Size(75, 21);
            this.openFolder.TabIndex = 8;
            this.openFolder.Text = "Open Folder";
            this.openFolder.UseVisualStyleBackColor = true;
            this.openFolder.Click += new System.EventHandler(this.button1_Click);
            // 
            // toolStripContainer1
            // 
            this.toolStripContainer1.BottomToolStripPanelVisible = false;
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(670, 0);
            this.toolStripContainer1.LeftToolStripPanelVisible = false;
            this.toolStripContainer1.Location = new System.Drawing.Point(12, 250);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.RightToolStripPanelVisible = false;
            this.toolStripContainer1.Size = new System.Drawing.Size(670, 23);
            this.toolStripContainer1.TabIndex = 9;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolShowBtn,
            this.toolExportBtn,
            this.toolExportProgress,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.searchBox});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(670, 25);
            this.toolStrip1.Stretch = true;
            this.toolStrip1.TabIndex = 0;
            // 
            // toolShowBtn
            // 
            this.toolShowBtn.Enabled = false;
            this.toolShowBtn.Image = ((System.Drawing.Image)(resources.GetObject("toolShowBtn.Image")));
            this.toolShowBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolShowBtn.Name = "toolShowBtn";
            this.toolShowBtn.Size = new System.Drawing.Size(68, 22);
            this.toolShowBtn.Text = "View File";
            this.toolShowBtn.ToolTipText = "Show Selected";
            this.toolShowBtn.Click += new System.EventHandler(this.toolShowBtn_Click);
            // 
            // toolExportBtn
            // 
            this.toolExportBtn.Enabled = false;
            this.toolExportBtn.Image = ((System.Drawing.Image)(resources.GetObject("toolExportBtn.Image")));
            this.toolExportBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolExportBtn.Name = "toolExportBtn";
            this.toolExportBtn.Size = new System.Drawing.Size(103, 22);
            this.toolExportBtn.Text = "Export Selected";
            this.toolExportBtn.Click += new System.EventHandler(this.toolExportBtn_Click);
            // 
            // toolExportProgress
            // 
            this.toolExportProgress.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolExportProgress.Name = "toolExportProgress";
            this.toolExportProgress.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.toolExportProgress.Size = new System.Drawing.Size(100, 22);
            this.toolExportProgress.Step = 1;
            this.toolExportProgress.Visible = false;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(40, 22);
            this.toolStripLabel1.Text = "Search";
            // 
            // searchBox
            // 
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(200, 25);
            this.searchBox.TextChanged += new System.EventHandler(this.searchBox_TextChanged);
            // 
            // sideLabelLbl
            // 
            this.sideLabelLbl.AutoSize = true;
            this.sideLabelLbl.Location = new System.Drawing.Point(12, 30);
            this.sideLabelLbl.Name = "sideLabelLbl";
            this.sideLabelLbl.Size = new System.Drawing.Size(85, 13);
            this.sideLabelLbl.TabIndex = 10;
            this.sideLabelLbl.Text = "Select a backup";
            // 
            // appSearchTxt
            // 
            this.appSearchTxt.Location = new System.Drawing.Point(461, 27);
            this.appSearchTxt.Name = "appSearchTxt";
            this.appSearchTxt.Size = new System.Drawing.Size(221, 20);
            this.appSearchTxt.TabIndex = 11;
            this.appSearchTxt.TextChanged += new System.EventHandler(this.appSearchTxt_TextChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fuleToolStripMenuItem,
            this.editToolStripMenuItem,
            this.analyzeToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(694, 24);
            this.menuStrip1.TabIndex = 12;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fuleToolStripMenuItem
            // 
            this.fuleToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitToolStripMenuItem});
            this.fuleToolStripMenuItem.Name = "fuleToolStripMenuItem";
            this.fuleToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fuleToolStripMenuItem.Text = "File";
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showMenu,
            this.exportMenu});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // showMenu
            // 
            this.showMenu.Enabled = false;
            this.showMenu.Image = ((System.Drawing.Image)(resources.GetObject("showMenu.Image")));
            this.showMenu.Name = "showMenu";
            this.showMenu.Size = new System.Drawing.Size(152, 22);
            this.showMenu.Text = "View";
            this.showMenu.Click += new System.EventHandler(this.toolShowBtn_Click);
            // 
            // exportMenu
            // 
            this.exportMenu.Enabled = false;
            this.exportMenu.Image = ((System.Drawing.Image)(resources.GetObject("exportMenu.Image")));
            this.exportMenu.Name = "exportMenu";
            this.exportMenu.Size = new System.Drawing.Size(152, 22);
            this.exportMenu.Text = "Export";
            this.exportMenu.Click += new System.EventHandler(this.toolExportBtn_Click);
            // 
            // analyzeToolStripMenuItem
            // 
            this.analyzeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.locationsToolStripMenuItem,
            this.mediaToolStripMenuItem,
            this.contactsToolStripMenuItem});
            this.analyzeToolStripMenuItem.Name = "analyzeToolStripMenuItem";
            this.analyzeToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.analyzeToolStripMenuItem.Text = "Analyze";
            // 
            // locationsToolStripMenuItem
            // 
            this.locationsToolStripMenuItem.Name = "locationsToolStripMenuItem";
            this.locationsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.locationsToolStripMenuItem.Text = "Locations";
            // 
            // mediaToolStripMenuItem
            // 
            this.mediaToolStripMenuItem.Name = "mediaToolStripMenuItem";
            this.mediaToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.mediaToolStripMenuItem.Text = "Media";
            // 
            // contactsToolStripMenuItem
            // 
            this.contactsToolStripMenuItem.Name = "contactsToolStripMenuItem";
            this.contactsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.contactsToolStripMenuItem.Text = "Contacts";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // BackupBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(694, 565);
            this.Controls.Add(this.appSearchTxt);
            this.Controls.Add(this.sideLabelLbl);
            this.Controls.Add(this.toolStripContainer1);
            this.Controls.Add(this.openFolder);
            this.Controls.Add(this.folderList);
            this.Controls.Add(this.fileList);
            this.Controls.Add(this.backupSelect);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "BackupBrowser";
            this.Text = "iDevice Backup Browser";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox backupSelect;
        private System.Windows.Forms.ListView fileList;
        private System.Windows.Forms.ListView folderList;
        private System.Windows.Forms.Button openFolder;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolShowBtn;
        private System.Windows.Forms.ToolStripButton toolExportBtn;
        private System.Windows.Forms.ToolStripProgressBar toolExportProgress;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox searchBox;
        private System.Windows.Forms.Label sideLabelLbl;
        private System.Windows.Forms.TextBox appSearchTxt;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fuleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showMenu;
        private System.Windows.Forms.ToolStripMenuItem exportMenu;
        private System.Windows.Forms.ToolStripMenuItem analyzeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem locationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mediaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contactsToolStripMenuItem;
    }
}