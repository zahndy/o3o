namespace o3o
{
    partial class GetOAuthForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetOAuthForm));
            this.Browserform = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // Browserform
            // 
            this.Browserform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Browserform.Location = new System.Drawing.Point(0, 0);
            this.Browserform.MinimumSize = new System.Drawing.Size(20, 20);
            this.Browserform.Name = "Browserform";
            this.Browserform.Size = new System.Drawing.Size(701, 473);
            this.Browserform.TabIndex = 0;
            // 
            // GetOAuthForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(701, 473);
            this.Controls.Add(this.Browserform);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GetOAuthForm";
            this.Text = "Authenticate o3o";
            this.Load += new System.EventHandler(this.GetOAuthForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser Browserform;
    }
}