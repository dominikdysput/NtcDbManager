namespace DbManager.View
{
    partial class DownloadSelectedVersion
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
            this.labelStatus = new System.Windows.Forms.Label();
            this.buttonPause = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelProcessing = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(34, 52);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(37, 13);
            this.labelStatus.TabIndex = 8;
            this.labelStatus.Text = "Status";
            // 
            // buttonPause
            // 
            this.buttonPause.Location = new System.Drawing.Point(37, 77);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(226, 23);
            this.buttonPause.TabIndex = 9;
            this.buttonPause.Text = "Stop";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(37, 26);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(226, 23);
            this.progressBar1.TabIndex = 10;
            // 
            // labelProcessing
            // 
            this.labelProcessing.AutoSize = true;
            this.labelProcessing.Location = new System.Drawing.Point(77, 52);
            this.labelProcessing.Name = "labelProcessing";
            this.labelProcessing.Size = new System.Drawing.Size(19, 13);
            this.labelProcessing.TabIndex = 11;
            this.labelProcessing.Text = "__";
            // 
            // DownloadSelectedVersion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(299, 128);
            this.Controls.Add(this.labelProcessing);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonPause);
            this.Controls.Add(this.labelStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DownloadSelectedVersion";
            this.ShowIcon = false;
            this.Text = "NTC DbManager";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button buttonPause;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelProcessing;
    }
}