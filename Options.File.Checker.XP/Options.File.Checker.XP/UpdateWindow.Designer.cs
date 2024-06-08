namespace Options.File.Checker.XP
{
    partial class UpdateWindow
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
            this.DownloadButton = new System.Windows.Forms.Button();
            this.UpdateTextBlock = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // DownloadButton
            // 
            this.DownloadButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.DownloadButton.Location = new System.Drawing.Point(165, 180);
            this.DownloadButton.Name = "DownloadButton";
            this.DownloadButton.Size = new System.Drawing.Size(101, 23);
            this.DownloadButton.TabIndex = 1;
            this.DownloadButton.Text = "Download";
            this.DownloadButton.UseVisualStyleBackColor = true;
            // 
            // UpdateTextBlock
            // 
            this.UpdateTextBlock.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UpdateTextBlock.BackColor = System.Drawing.SystemColors.MenuBar;
            this.UpdateTextBlock.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.UpdateTextBlock.Location = new System.Drawing.Point(12, 12);
            this.UpdateTextBlock.Multiline = true;
            this.UpdateTextBlock.Name = "UpdateTextBlock";
            this.UpdateTextBlock.ReadOnly = true;
            this.UpdateTextBlock.Size = new System.Drawing.Size(417, 162);
            this.UpdateTextBlock.TabIndex = 2;
            this.UpdateTextBlock.Text = "things";
            this.UpdateTextBlock.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // UpdateWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(441, 215);
            this.Controls.Add(this.UpdateTextBlock);
            this.Controls.Add(this.DownloadButton);
            this.Name = "UpdateWindow";
            this.Text = "Check for Updates";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button DownloadButton;
        private System.Windows.Forms.TextBox UpdateTextBlock;
    }
}