namespace Dukebox.Desktop
{
    partial class SplashScreenView
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
            this.imgSplashBackground = new System.Windows.Forms.PictureBox();
            this.progBarLoading = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.imgSplashBackground)).BeginInit();
            this.SuspendLayout();
            // 
            // imgSplashBackground
            // 
            this.imgSplashBackground.BackgroundImage = global::Dukebox.Desktop.Properties.Resources.splash;
            this.imgSplashBackground.ErrorImage = global::Dukebox.Desktop.Properties.Resources.splash;
            this.imgSplashBackground.Image = global::Dukebox.Desktop.Properties.Resources.splash;
            this.imgSplashBackground.InitialImage = global::Dukebox.Desktop.Properties.Resources.splash;
            this.imgSplashBackground.Location = new System.Drawing.Point(0, 0);
            this.imgSplashBackground.MaximumSize = new System.Drawing.Size(600, 180);
            this.imgSplashBackground.MinimumSize = new System.Drawing.Size(600, 180);
            this.imgSplashBackground.Name = "imgSplashBackground";
            this.imgSplashBackground.Size = new System.Drawing.Size(600, 180);
            this.imgSplashBackground.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgSplashBackground.TabIndex = 0;
            this.imgSplashBackground.TabStop = false;
            // 
            // progBarLoading
            // 
            this.progBarLoading.Location = new System.Drawing.Point(0, 180);
            this.progBarLoading.MaximumSize = new System.Drawing.Size(600, 20);
            this.progBarLoading.MinimumSize = new System.Drawing.Size(600, 20);
            this.progBarLoading.Name = "progBarLoading";
            this.progBarLoading.Size = new System.Drawing.Size(600, 20);
            this.progBarLoading.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progBarLoading.TabIndex = 1;
            // 
            // SplashScreenView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 200);
            this.ControlBox = false;
            this.Controls.Add(this.progBarLoading);
            this.Controls.Add(this.imgSplashBackground);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(600, 200);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 200);
            this.Name = "SplashScreenView";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SplashScreenView";
            this.Load += new System.EventHandler(this.SplashScreenView_Load);
            ((System.ComponentModel.ISupportInitialize)(this.imgSplashBackground)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox imgSplashBackground;
        private System.Windows.Forms.ProgressBar progBarLoading;
    }
}