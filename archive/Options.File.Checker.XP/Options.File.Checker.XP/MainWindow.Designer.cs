namespace Options.File.Checker.XP
{
    partial class MainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.LicenseFileBrowseButton = new System.Windows.Forms.Button();
            this.OptionsFileBrowseButton = new System.Windows.Forms.Button();
            this.LicenseFileLocationTextBox = new System.Windows.Forms.TextBox();
            this.OptionsFileLocationTextBox = new System.Windows.Forms.TextBox();
            this.OutputTextBlock = new System.Windows.Forms.RichTextBox();
            this.AnalyzerButton = new System.Windows.Forms.Button();
            this.SaveOutputButton = new System.Windows.Forms.Button();
            this.LicenseFileLocationLabel = new System.Windows.Forms.Label();
            this.OptionsFileLocationLabel = new System.Windows.Forms.Label();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.CheckForUpdateButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LicenseFileBrowseButton
            // 
            this.LicenseFileBrowseButton.Location = new System.Drawing.Point(345, 35);
            this.LicenseFileBrowseButton.Name = "LicenseFileBrowseButton";
            this.LicenseFileBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.LicenseFileBrowseButton.TabIndex = 0;
            this.LicenseFileBrowseButton.Text = "Browse";
            this.LicenseFileBrowseButton.UseVisualStyleBackColor = true;
            this.LicenseFileBrowseButton.Click += new System.EventHandler(this.LicenseFileBrowseButton_Click);
            // 
            // OptionsFileBrowseButton
            // 
            this.OptionsFileBrowseButton.Location = new System.Drawing.Point(345, 87);
            this.OptionsFileBrowseButton.Name = "OptionsFileBrowseButton";
            this.OptionsFileBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.OptionsFileBrowseButton.TabIndex = 1;
            this.OptionsFileBrowseButton.Text = "Browse";
            this.OptionsFileBrowseButton.UseVisualStyleBackColor = true;
            this.OptionsFileBrowseButton.Click += new System.EventHandler(this.OptionsFileBrowseButton_Click);
            // 
            // LicenseFileLocationTextBox
            // 
            this.LicenseFileLocationTextBox.Location = new System.Drawing.Point(21, 37);
            this.LicenseFileLocationTextBox.Name = "LicenseFileLocationTextBox";
            this.LicenseFileLocationTextBox.Size = new System.Drawing.Size(293, 20);
            this.LicenseFileLocationTextBox.TabIndex = 2;
            // 
            // OptionsFileLocationTextBox
            // 
            this.OptionsFileLocationTextBox.Location = new System.Drawing.Point(21, 87);
            this.OptionsFileLocationTextBox.Name = "OptionsFileLocationTextBox";
            this.OptionsFileLocationTextBox.Size = new System.Drawing.Size(293, 20);
            this.OptionsFileLocationTextBox.TabIndex = 3;
            // 
            // OutputTextBlock
            // 
            this.OutputTextBlock.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputTextBlock.Location = new System.Drawing.Point(21, 133);
            this.OutputTextBlock.Name = "OutputTextBlock";
            this.OutputTextBlock.ReadOnly = true;
            this.OutputTextBlock.Size = new System.Drawing.Size(695, 232);
            this.OutputTextBlock.TabIndex = 4;
            this.OutputTextBlock.Text = "Warning messages will appear here after analysis. Errors will appear in a pop-up " +
    "window.";
            // 
            // AnalyzerButton
            // 
            this.AnalyzerButton.Location = new System.Drawing.Point(457, 61);
            this.AnalyzerButton.Name = "AnalyzerButton";
            this.AnalyzerButton.Size = new System.Drawing.Size(75, 23);
            this.AnalyzerButton.TabIndex = 5;
            this.AnalyzerButton.Text = "Analyze";
            this.AnalyzerButton.UseVisualStyleBackColor = true;
            this.AnalyzerButton.Click += new System.EventHandler(this.AnalyzerButton_Click);
            // 
            // SaveOutputButton
            // 
            this.SaveOutputButton.Location = new System.Drawing.Point(569, 61);
            this.SaveOutputButton.Name = "SaveOutputButton";
            this.SaveOutputButton.Size = new System.Drawing.Size(90, 23);
            this.SaveOutputButton.TabIndex = 6;
            this.SaveOutputButton.Text = "Save Output";
            this.SaveOutputButton.UseVisualStyleBackColor = true;
            this.SaveOutputButton.Click += new System.EventHandler(this.SaveOutputButton_Click);
            // 
            // LicenseFileLocationLabel
            // 
            this.LicenseFileLocationLabel.AutoSize = true;
            this.LicenseFileLocationLabel.Location = new System.Drawing.Point(18, 20);
            this.LicenseFileLocationLabel.Name = "LicenseFileLocationLabel";
            this.LicenseFileLocationLabel.Size = new System.Drawing.Size(84, 13);
            this.LicenseFileLocationLabel.TabIndex = 7;
            this.LicenseFileLocationLabel.Text = "License file path";
            // 
            // OptionsFileLocationLabel
            // 
            this.OptionsFileLocationLabel.AutoSize = true;
            this.OptionsFileLocationLabel.Location = new System.Drawing.Point(18, 72);
            this.OptionsFileLocationLabel.Name = "OptionsFileLocationLabel";
            this.OptionsFileLocationLabel.Size = new System.Drawing.Size(83, 13);
            this.OptionsFileLocationLabel.TabIndex = 8;
            this.OptionsFileLocationLabel.Text = "Options file path";
            // 
            // VersionLabel
            // 
            this.VersionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Location = new System.Drawing.Point(3, 376);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(40, 13);
            this.VersionLabel.TabIndex = 10;
            this.VersionLabel.Text = "VLabel";
            // 
            // CheckForUpdateButton
            // 
            this.CheckForUpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CheckForUpdateButton.Location = new System.Drawing.Point(58, 370);
            this.CheckForUpdateButton.Name = "CheckForUpdateButton";
            this.CheckForUpdateButton.Size = new System.Drawing.Size(119, 23);
            this.CheckForUpdateButton.TabIndex = 11;
            this.CheckForUpdateButton.Text = "Check for updates";
            this.CheckForUpdateButton.UseVisualStyleBackColor = true;
            this.CheckForUpdateButton.Click += new System.EventHandler(this.CheckForUpdateButton_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(737, 396);
            this.Controls.Add(this.CheckForUpdateButton);
            this.Controls.Add(this.VersionLabel);
            this.Controls.Add(this.OptionsFileLocationLabel);
            this.Controls.Add(this.LicenseFileLocationLabel);
            this.Controls.Add(this.SaveOutputButton);
            this.Controls.Add(this.AnalyzerButton);
            this.Controls.Add(this.OutputTextBlock);
            this.Controls.Add(this.OptionsFileLocationTextBox);
            this.Controls.Add(this.LicenseFileLocationTextBox);
            this.Controls.Add(this.OptionsFileBrowseButton);
            this.Controls.Add(this.LicenseFileBrowseButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(745, 430);
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Options File Checker";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LicenseFileBrowseButton;
        private System.Windows.Forms.Button OptionsFileBrowseButton;
        private System.Windows.Forms.TextBox LicenseFileLocationTextBox;
        private System.Windows.Forms.TextBox OptionsFileLocationTextBox;
        private System.Windows.Forms.RichTextBox OutputTextBlock;
        private System.Windows.Forms.Button AnalyzerButton;
        private System.Windows.Forms.Button SaveOutputButton;
        private System.Windows.Forms.Label LicenseFileLocationLabel;
        private System.Windows.Forms.Label OptionsFileLocationLabel;
        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.Button CheckForUpdateButton;
    }
}

