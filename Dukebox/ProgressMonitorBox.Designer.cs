namespace Dukebox
{
    partial class ProgressMonitorBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 
        /// </summary>
        public int ProgressBarMaximum
        {
            get
            {
                return prgBarImportProgress.Maximum;
            }

            set
            {
                Invoke(new ValueUpdateDelegate(() => prgBarImportProgress.Maximum = value));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int ProgressBarValue
        {
            get
            {
                return prgBarImportProgress.Value;
            }

            set
            {
               Invoke(new ValueUpdateDelegate(()=> prgBarImportProgress.Value = value));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ProgressBarCompleted
        {
            get
            {
                return prgBarImportProgress.Value == prgBarImportProgress.Maximum;
            }
        }

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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void ResetProgressBar()
        {
            if (prgBarImportProgress.Value == prgBarImportProgress.Maximum)
            {
                prgBarImportProgress.Value = 0;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void ImportProgressBarStep()
        {
            prgBarImportProgress.PerformStep();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void NotifcationLabelUpdate(string text)
        {
            Invoke(new ValueUpdateDelegate(() => lblNotification.Text = text));
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.prgBarImportProgress = new System.Windows.Forms.ProgressBar();
            this.lblNotification = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // prgBarImportProgress
            // 
            this.prgBarImportProgress.Location = new System.Drawing.Point(12, 12);
            this.prgBarImportProgress.Name = "prgBarImportProgress";
            this.prgBarImportProgress.Size = new System.Drawing.Size(311, 23);
            this.prgBarImportProgress.Step = 1;
            this.prgBarImportProgress.TabIndex = 0;
            // 
            // lblNotification
            // 
            this.lblNotification.AutoSize = true;
            this.lblNotification.Location = new System.Drawing.Point(12, 53);
            this.lblNotification.Name = "lblNotification";
            this.lblNotification.Size = new System.Drawing.Size(0, 13);
            this.lblNotification.TabIndex = 1;
            // 
            // LibraryImportMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 90);
            this.Controls.Add(this.lblNotification);
            this.Controls.Add(this.prgBarImportProgress);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LibraryImportMonitor";
            this.Text = "Jukebox - Library Import Monitor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar prgBarImportProgress;
        private System.Windows.Forms.Label lblNotification;
    }
}