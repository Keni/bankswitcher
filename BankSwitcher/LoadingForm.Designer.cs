namespace BankSwitcher
{
    partial class LoadingForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadingForm));
            this.labelLoadingText = new System.Windows.Forms.Label();
            this.pictureBoxLoading = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLoading)).BeginInit();
            this.SuspendLayout();
            // 
            // labelLoadingText
            // 
            this.labelLoadingText.AutoSize = true;
            this.labelLoadingText.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelLoadingText.Location = new System.Drawing.Point(68, 26);
            this.labelLoadingText.Name = "labelLoadingText";
            this.labelLoadingText.Size = new System.Drawing.Size(98, 19);
            this.labelLoadingText.TabIndex = 1;
            this.labelLoadingText.Text = "Подождите...";
            // 
            // pictureBoxLoading
            // 
            this.pictureBoxLoading.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxLoading.Image")));
            this.pictureBoxLoading.Location = new System.Drawing.Point(12, 12);
            this.pictureBoxLoading.Name = "pictureBoxLoading";
            this.pictureBoxLoading.Size = new System.Drawing.Size(50, 50);
            this.pictureBoxLoading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxLoading.TabIndex = 0;
            this.pictureBoxLoading.TabStop = false;
            // 
            // LoadingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 78);
            this.ControlBox = false;
            this.Controls.Add(this.labelLoadingText);
            this.Controls.Add(this.pictureBoxLoading);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "LoadingForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Загрузка...";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLoading)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxLoading;
        private System.Windows.Forms.Label labelLoadingText;
    }
}