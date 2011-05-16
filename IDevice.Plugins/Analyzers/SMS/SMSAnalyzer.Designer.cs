namespace IDevice.Plugins.Analyzers.SMS
{
    partial class SMSAnalyzer
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
            this.smsListBox = new System.Windows.Forms.ListBox();
            this.smsTextBox = new System.Windows.Forms.RichTextBox();
            this.filterTextBox = new System.Windows.Forms.TextBox();
            this.filterButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // smsListBox
            // 
            this.smsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.smsListBox.FormattingEnabled = true;
            this.smsListBox.HorizontalScrollbar = true;
            this.smsListBox.Location = new System.Drawing.Point(12, 39);
            this.smsListBox.Name = "smsListBox";
            this.smsListBox.Size = new System.Drawing.Size(325, 342);
            this.smsListBox.TabIndex = 3;
            this.smsListBox.SelectedIndexChanged += new System.EventHandler(this.smsListBox_SelectedIndexChanged);
            // 
            // smsTextBox
            // 
            this.smsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.smsTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.smsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.smsTextBox.Location = new System.Drawing.Point(345, 39);
            this.smsTextBox.Name = "smsTextBox";
            this.smsTextBox.ReadOnly = true;
            this.smsTextBox.Size = new System.Drawing.Size(255, 149);
            this.smsTextBox.TabIndex = 5;
            this.smsTextBox.Text = "";
            // 
            // filterTextBox
            // 
            this.filterTextBox.Location = new System.Drawing.Point(12, 12);
            this.filterTextBox.Name = "filterTextBox";
            this.filterTextBox.Size = new System.Drawing.Size(244, 20);
            this.filterTextBox.TabIndex = 6;
            // 
            // filterButton
            // 
            this.filterButton.Location = new System.Drawing.Point(262, 12);
            this.filterButton.Name = "filterButton";
            this.filterButton.Size = new System.Drawing.Size(75, 23);
            this.filterButton.TabIndex = 7;
            this.filterButton.Text = "Filter";
            this.filterButton.UseVisualStyleBackColor = true;
            this.filterButton.Click += new System.EventHandler(this.filterButton_Click);
            // 
            // SMSAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(612, 398);
            this.Controls.Add(this.smsTextBox);
            this.Controls.Add(this.filterButton);
            this.Controls.Add(this.filterTextBox);
            this.Controls.Add(this.smsListBox);
            this.Name = "SMSAnalyzer";
            this.Text = "SMSAnalyzer";
            this.Load += new System.EventHandler(this.SMSAnalyzer_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox smsListBox;
        private System.Windows.Forms.RichTextBox smsTextBox;
        private System.Windows.Forms.TextBox filterTextBox;
        private System.Windows.Forms.Button filterButton;
    }
}