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
            this.SaveOutputButton = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.VLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LicenseFileBrowseButton
            // 
            this.LicenseFileBrowseButton.Location = new System.Drawing.Point(411, 126);
            this.LicenseFileBrowseButton.Name = "LicenseFileBrowseButton";
            this.LicenseFileBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.LicenseFileBrowseButton.TabIndex = 0;
            this.LicenseFileBrowseButton.Text = "Browse";
            this.LicenseFileBrowseButton.UseVisualStyleBackColor = true;
            // 
            // OptionsFileBrowseButton
            // 
            this.OptionsFileBrowseButton.Location = new System.Drawing.Point(411, 194);
            this.OptionsFileBrowseButton.Name = "OptionsFileBrowseButton";
            this.OptionsFileBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.OptionsFileBrowseButton.TabIndex = 1;
            this.OptionsFileBrowseButton.Text = "Browse";
            this.OptionsFileBrowseButton.UseVisualStyleBackColor = true;
            // 
            // SaveOutputButton
            // 
            this.SaveOutputButton.Location = new System.Drawing.Point(550, 157);
            this.SaveOutputButton.Name = "SaveOutputButton";
            this.SaveOutputButton.Size = new System.Drawing.Size(88, 23);
            this.SaveOutputButton.TabIndex = 2;
            this.SaveOutputButton.Text = "Save Output";
            this.SaveOutputButton.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(37, 43);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 3;
            // 
            // VLabel
            // 
            this.VLabel.AutoSize = true;
            this.VLabel.Location = new System.Drawing.Point(26, 425);
            this.VLabel.Name = "VLabel";
            this.VLabel.Size = new System.Drawing.Size(13, 13);
            this.VLabel.TabIndex = 4;
            this.VLabel.Text = "v";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.VLabel);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.SaveOutputButton);
            this.Controls.Add(this.OptionsFileBrowseButton);
            this.Controls.Add(this.LicenseFileBrowseButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Options File Checker";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LicenseFileBrowseButton;
        private System.Windows.Forms.Button OptionsFileBrowseButton;
        private System.Windows.Forms.Button SaveOutputButton;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label VLabel;
    }
}

