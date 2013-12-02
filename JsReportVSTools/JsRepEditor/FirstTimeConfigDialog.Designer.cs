namespace JsReportVSTools.JsRepEditor
{
    partial class FirstTimeConfigDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbServerUrl = new System.Windows.Forms.TextBox();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.lnkDownload = new System.Windows.Forms.LinkLabel();
            this.lblError = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(15, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(396, 56);
            this.label1.TabIndex = 0;
            this.label1.Text = "You need to fill jsreport server url. If you have not installed jsreport server y" +
    "et, you can download it ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "jsreport server url:";
            // 
            // tbServerUrl
            // 
            this.tbServerUrl.Location = new System.Drawing.Point(146, 80);
            this.tbServerUrl.Name = "tbServerUrl";
            this.tbServerUrl.Size = new System.Drawing.Size(278, 22);
            this.tbServerUrl.TabIndex = 2;
            this.tbServerUrl.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbServerUrl_KeyPress);
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(165, 147);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(80, 28);
            this.btnConfirm.TabIndex = 3;
            this.btnConfirm.Text = "Confirm";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // lnkDownload
            // 
            this.lnkDownload.AutoSize = true;
            this.lnkDownload.Location = new System.Drawing.Point(271, 41);
            this.lnkDownload.Name = "lnkDownload";
            this.lnkDownload.Size = new System.Drawing.Size(41, 17);
            this.lnkDownload.TabIndex = 4;
            this.lnkDownload.TabStop = true;
            this.lnkDownload.Text = "here.";
            this.lnkDownload.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkDownload_LinkClicked);
            // 
            // lblError
            // 
            this.lblError.AutoSize = true;
            this.lblError.ForeColor = System.Drawing.Color.Red;
            this.lblError.Location = new System.Drawing.Point(18, 111);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(234, 17);
            this.lblError.TabIndex = 5;
            this.lblError.Text = "Could not connect to jsreport server";
            // 
            // FirstTimeConfigDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(438, 189);
            this.Controls.Add(this.lblError);
            this.Controls.Add(this.lnkDownload);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.tbServerUrl);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FirstTimeConfigDialog";
            this.Padding = new System.Windows.Forms.Padding(12, 11, 12, 11);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure jsreport";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbServerUrl;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.LinkLabel lnkDownload;
        private System.Windows.Forms.Label lblError;

    }
}
