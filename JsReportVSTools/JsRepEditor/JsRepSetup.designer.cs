namespace JsReportVSTools
{
    public partial class JsRepSetup
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOpenSchemaDialog = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pnlPreview = new System.Windows.Forms.Panel();
            this.lblPreview = new System.Windows.Forms.Label();
            this.pbPreview = new System.Windows.Forms.PictureBox();
            this.label6 = new System.Windows.Forms.Label();
            this.lnkHtml = new System.Windows.Forms.LinkLabel();
            this.lnkHelpers = new System.Windows.Forms.LinkLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cbRecipe = new System.Windows.Forms.ComboBox();
            this.cbEngine = new System.Windows.Forms.ComboBox();
            this.tbTimeout = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblSchemaFilePath = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.schemaDialog = new System.Windows.Forms.OpenFileDialog();
            this.panel1.SuspendLayout();
            this.pnlPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOpenSchemaDialog
            // 
            this.btnOpenSchemaDialog.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnOpenSchemaDialog.Location = new System.Drawing.Point(250, 225);
            this.btnOpenSchemaDialog.Name = "btnOpenSchemaDialog";
            this.btnOpenSchemaDialog.Size = new System.Drawing.Size(42, 26);
            this.btnOpenSchemaDialog.TabIndex = 1;
            this.btnOpenSchemaDialog.Text = "....";
            this.btnOpenSchemaDialog.UseVisualStyleBackColor = true;
            this.btnOpenSchemaDialog.Click += new System.EventHandler(this.openSchemaDialog_click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(30, 179);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(196, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "JavaScript Templating Engine";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pnlPreview);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.lnkHtml);
            this.panel1.Controls.Add(this.lnkHelpers);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.cbRecipe);
            this.panel1.Controls.Add(this.cbEngine);
            this.panel1.Controls.Add(this.tbTimeout);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.lblSchemaFilePath);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btnOpenSchemaDialog);
            this.panel1.Location = new System.Drawing.Point(20, 17);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(617, 395);
            this.panel1.TabIndex = 5;
            // 
            // pnlPreview
            // 
            this.pnlPreview.Controls.Add(this.lblPreview);
            this.pnlPreview.Controls.Add(this.pbPreview);
            this.pnlPreview.Location = new System.Drawing.Point(19, 14);
            this.pnlPreview.Name = "pnlPreview";
            this.pnlPreview.Size = new System.Drawing.Size(100, 32);
            this.pnlPreview.TabIndex = 23;
            this.pnlPreview.Click += new System.EventHandler(this.pnlPreview_Click);
            this.pnlPreview.MouseEnter += new System.EventHandler(this.pnlPreview_MouseEnter);
            this.pnlPreview.MouseLeave += new System.EventHandler(this.pnlPreview_MouseLeave);
            // 
            // lblPreview
            // 
            this.lblPreview.AutoSize = true;
            this.lblPreview.Location = new System.Drawing.Point(40, 7);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(57, 17);
            this.lblPreview.TabIndex = 22;
            this.lblPreview.Text = "Preview";
            this.lblPreview.Click += new System.EventHandler(this.lblPreview_Click);
            // 
            // pbPreview
            // 
            this.pbPreview.Location = new System.Drawing.Point(6, 2);
            this.pbPreview.Name = "pbPreview";
            this.pbPreview.Size = new System.Drawing.Size(28, 28);
            this.pbPreview.TabIndex = 21;
            this.pbPreview.TabStop = false;
            this.pbPreview.Click += new System.EventHandler(this.pbPreview_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label6.Location = new System.Drawing.Point(28, 136);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(175, 17);
            this.label6.TabIndex = 20;
            this.label6.Text = "Template configuration";
            // 
            // lnkHtml
            // 
            this.lnkHtml.AutoSize = true;
            this.lnkHtml.Location = new System.Drawing.Point(130, 86);
            this.lnkHtml.Name = "lnkHtml";
            this.lnkHtml.Size = new System.Drawing.Size(73, 17);
            this.lnkHtml.TabIndex = 19;
            this.lnkHtml.TabStop = true;
            this.lnkHtml.Text = "Go to html";
            this.lnkHtml.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHtml_LinkClicked);
            // 
            // lnkHelpers
            // 
            this.lnkHelpers.AutoSize = true;
            this.lnkHelpers.Location = new System.Drawing.Point(30, 86);
            this.lnkHelpers.Name = "lnkHelpers";
            this.lnkHelpers.Size = new System.Drawing.Size(94, 17);
            this.lnkHelpers.TabIndex = 18;
            this.lnkHelpers.TabStop = true;
            this.lnkHelpers.Text = "Go to helpers";
            this.lnkHelpers.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHelpers_LinkClicked);
            // 
            // label5
            // 
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label5.Location = new System.Drawing.Point(3, 120);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(560, 2);
            this.label5.TabIndex = 17;
            // 
            // label4
            // 
            this.label4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label4.Location = new System.Drawing.Point(3, 61);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(559, 2);
            this.label4.TabIndex = 16;
            // 
            // cbRecipe
            // 
            this.cbRecipe.FormattingEnabled = true;
            this.cbRecipe.Location = new System.Drawing.Point(125, 18);
            this.cbRecipe.Name = "cbRecipe";
            this.cbRecipe.Size = new System.Drawing.Size(140, 24);
            this.cbRecipe.TabIndex = 15;
            // 
            // cbEngine
            // 
            this.cbEngine.FormattingEnabled = true;
            this.cbEngine.Location = new System.Drawing.Point(232, 176);
            this.cbEngine.Name = "cbEngine";
            this.cbEngine.Size = new System.Drawing.Size(121, 24);
            this.cbEngine.TabIndex = 11;
            this.cbEngine.SelectedValueChanged += new System.EventHandler(this.cbEngine_SelectedValueChanged);
            // 
            // tbTimeout
            // 
            this.tbTimeout.Location = new System.Drawing.Point(133, 270);
            this.tbTimeout.Name = "tbTimeout";
            this.tbTimeout.Size = new System.Drawing.Size(100, 22);
            this.tbTimeout.TabIndex = 8;
            this.tbTimeout.TextChanged += new System.EventHandler(this.tbTimeout_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label3.Location = new System.Drawing.Point(30, 273);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 17);
            this.label3.TabIndex = 7;
            this.label3.Text = "Timeout in ms";
            // 
            // lblSchemaFilePath
            // 
            this.lblSchemaFilePath.AutoSize = true;
            this.lblSchemaFilePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblSchemaFilePath.Location = new System.Drawing.Point(298, 230);
            this.lblSchemaFilePath.Name = "lblSchemaFilePath";
            this.lblSchemaFilePath.Size = new System.Drawing.Size(85, 17);
            this.lblSchemaFilePath.TabIndex = 6;
            this.lblSchemaFilePath.Text = "Schema File";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.Location = new System.Drawing.Point(30, 230);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(214, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Json file with schema for preview";
            // 
            // JsRepSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "JsRepSetup";
            this.Size = new System.Drawing.Size(656, 427);
            this.Load += new System.EventHandler(this.JsRepSetup_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.pnlPreview.ResumeLayout(false);
            this.pnlPreview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPreview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOpenSchemaDialog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblSchemaFilePath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.OpenFileDialog schemaDialog;
        private System.Windows.Forms.TextBox tbTimeout;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbEngine;
        private System.Windows.Forms.ComboBox cbRecipe;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.LinkLabel lnkHelpers;
        private System.Windows.Forms.LinkLabel lnkHtml;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.PictureBox pbPreview;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.Panel pnlPreview;


    }
}
