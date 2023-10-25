namespace Options.File.Checker
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            LicenseFileLocationTextBox = new TextBox();
            OptionsFileLocationTextBox = new TextBox();
            LicenseFileBrowseButton = new Button();
            OptionsFileBrowseButton = new Button();
            AnalyzerButton = new Button();
            LicenseFileLocationLabel = new Label();
            OptionsFileLocationLabel = new Label();
            OutputTextBox = new TextBox();
            SaveOutputButton = new Button();
            SuspendLayout();
            // 
            // LicenseFileLocationTextBox
            // 
            LicenseFileLocationTextBox.Location = new Point(19, 33);
            LicenseFileLocationTextBox.Margin = new Padding(2, 1, 2, 1);
            LicenseFileLocationTextBox.Name = "LicenseFileLocationTextBox";
            LicenseFileLocationTextBox.Size = new Size(286, 23);
            LicenseFileLocationTextBox.TabIndex = 0;
            LicenseFileLocationTextBox.TextChanged += LicenseFileLocationTextBox_TextChanged;
            // 
            // OptionsFileLocationTextBox
            // 
            OptionsFileLocationTextBox.Location = new Point(19, 90);
            OptionsFileLocationTextBox.Margin = new Padding(2, 1, 2, 1);
            OptionsFileLocationTextBox.Name = "OptionsFileLocationTextBox";
            OptionsFileLocationTextBox.Size = new Size(286, 23);
            OptionsFileLocationTextBox.TabIndex = 2;
            OptionsFileLocationTextBox.TextChanged += OptionsFileLocationTextBox_TextChanged;
            // 
            // LicenseFileBrowseButton
            // 
            LicenseFileBrowseButton.Location = new Point(309, 34);
            LicenseFileBrowseButton.Margin = new Padding(2, 1, 2, 1);
            LicenseFileBrowseButton.Name = "LicenseFileBrowseButton";
            LicenseFileBrowseButton.Size = new Size(68, 22);
            LicenseFileBrowseButton.TabIndex = 1;
            LicenseFileBrowseButton.Text = "Browse";
            LicenseFileBrowseButton.UseVisualStyleBackColor = true;
            LicenseFileBrowseButton.Click += LicenseFileBrowseButton_Click;
            // 
            // OptionsFileBrowseButton
            // 
            OptionsFileBrowseButton.Location = new Point(309, 91);
            OptionsFileBrowseButton.Margin = new Padding(2, 1, 2, 1);
            OptionsFileBrowseButton.Name = "OptionsFileBrowseButton";
            OptionsFileBrowseButton.Size = new Size(68, 22);
            OptionsFileBrowseButton.TabIndex = 3;
            OptionsFileBrowseButton.Text = "Browse";
            OptionsFileBrowseButton.UseVisualStyleBackColor = true;
            OptionsFileBrowseButton.Click += OptionsFileBrowseButton_Click;
            // 
            // AnalyzerButton
            // 
            AnalyzerButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            AnalyzerButton.Enabled = false;
            AnalyzerButton.Location = new Point(142, 175);
            AnalyzerButton.Margin = new Padding(2, 1, 2, 1);
            AnalyzerButton.Name = "AnalyzerButton";
            AnalyzerButton.Size = new Size(81, 26);
            AnalyzerButton.TabIndex = 4;
            AnalyzerButton.Text = "Analyze";
            AnalyzerButton.UseVisualStyleBackColor = true;
            AnalyzerButton.Click += AnalyzerButton_Click;
            // 
            // LicenseFileLocationLabel
            // 
            LicenseFileLocationLabel.AutoSize = true;
            LicenseFileLocationLabel.Location = new Point(16, 16);
            LicenseFileLocationLabel.Margin = new Padding(2, 0, 2, 0);
            LicenseFileLocationLabel.Name = "LicenseFileLocationLabel";
            LicenseFileLocationLabel.Size = new Size(94, 15);
            LicenseFileLocationLabel.TabIndex = 5;
            LicenseFileLocationLabel.Text = "License File Path";
            // 
            // OptionsFileLocationLabel
            // 
            OptionsFileLocationLabel.AutoSize = true;
            OptionsFileLocationLabel.Location = new Point(16, 74);
            OptionsFileLocationLabel.Margin = new Padding(2, 0, 2, 0);
            OptionsFileLocationLabel.Name = "OptionsFileLocationLabel";
            OptionsFileLocationLabel.Size = new Size(97, 15);
            OptionsFileLocationLabel.TabIndex = 6;
            OptionsFileLocationLabel.Text = "Options File Path";
            // 
            // OutputTextBox
            // 
            OutputTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            OutputTextBox.Location = new Point(404, 16);
            OutputTextBox.Margin = new Padding(2, 1, 2, 1);
            OutputTextBox.Multiline = true;
            OutputTextBox.Name = "OutputTextBox";
            OutputTextBox.ReadOnly = true;
            OutputTextBox.ScrollBars = ScrollBars.Both;
            OutputTextBox.Size = new Size(265, 145);
            OutputTextBox.TabIndex = 7;
            OutputTextBox.Text = "there are things here. read what I have to say.";
            // 
            // SaveOutputButton
            // 
            SaveOutputButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            SaveOutputButton.Location = new Point(573, 175);
            SaveOutputButton.Margin = new Padding(2, 1, 2, 1);
            SaveOutputButton.Name = "SaveOutputButton";
            SaveOutputButton.Size = new Size(94, 26);
            SaveOutputButton.TabIndex = 8;
            SaveOutputButton.Text = "Save Output";
            SaveOutputButton.UseVisualStyleBackColor = true;
            SaveOutputButton.Click += SaveOutputButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoValidate = AutoValidate.EnablePreventFocusChange;
            ClientSize = new Size(687, 211);
            Controls.Add(SaveOutputButton);
            Controls.Add(OutputTextBox);
            Controls.Add(OptionsFileLocationLabel);
            Controls.Add(LicenseFileLocationLabel);
            Controls.Add(AnalyzerButton);
            Controls.Add(OptionsFileBrowseButton);
            Controls.Add(LicenseFileBrowseButton);
            Controls.Add(OptionsFileLocationTextBox);
            Controls.Add(LicenseFileLocationTextBox);
            Margin = new Padding(2, 1, 2, 1);
            MaximizeBox = false;
            MaximumSize = new Size(2000, 1000);
            MinimumSize = new Size(703, 250);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Options File Checker";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox LicenseFileLocationTextBox;
        private TextBox OptionsFileLocationTextBox;
        private Button LicenseFileBrowseButton;
        private Button OptionsFileBrowseButton;
        private Button AnalyzerButton;
        private Label LicenseFileLocationLabel;
        private Label OptionsFileLocationLabel;
        private TextBox OutputTextBox;
        private Button SaveOutputButton;
    }
}