
namespace SC_PLAM_GLBT_DLL
{
    partial class Foot
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Foot));
            this.axCapture1 = new AxCaptureLib.AxCapture();
            ((System.ComponentModel.ISupportInitialize)(this.axCapture1)).BeginInit();
            this.SuspendLayout();
            // 
            // axCapture1
            // 
            this.axCapture1.Enabled = true;
            this.axCapture1.Location = new System.Drawing.Point(0, 0);
            this.axCapture1.Name = "axCapture1";
            this.axCapture1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axCapture1.OcxState")));
            this.axCapture1.Size = new System.Drawing.Size(427, 608);
            this.axCapture1.TabIndex = 0;
            // 
            // Foot
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(428, 609);
            this.Controls.Add(this.axCapture1);
            this.Name = "Foot";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Foot";
            ((System.ComponentModel.ISupportInitialize)(this.axCapture1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxCaptureLib.AxCapture axCapture1;
    }
}