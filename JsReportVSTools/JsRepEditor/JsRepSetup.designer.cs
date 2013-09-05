﻿namespace JsReportVSTools
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
            this.tbTemplatingEngine = new System.Windows.Forms.TextBox();
            this.tbTimeout = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblSchemaFilePath = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.schemaDialog = new System.Windows.Forms.OpenFileDialog();
            this.btnPreview = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOpenSchemaDialog
            // 
            this.btnOpenSchemaDialog.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnOpenSchemaDialog.Location = new System.Drawing.Point(268, 92);
            this.btnOpenSchemaDialog.Name = "btnOpenSchemaDialog";
            this.btnOpenSchemaDialog.Size = new System.Drawing.Size(37, 28);
            this.btnOpenSchemaDialog.TabIndex = 1;
            this.btnOpenSchemaDialog.Text = "....";
            this.btnOpenSchemaDialog.UseVisualStyleBackColor = true;
            this.btnOpenSchemaDialog.Click += new System.EventHandler(this.openSchemaDialog_click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(43, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(286, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "Choose javascript Templating Engine";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnPreview);
            this.panel1.Controls.Add(this.tbTemplatingEngine);
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
            // tbTemplatingEngine
            // 
            this.tbTemplatingEngine.Location = new System.Drawing.Point(335, 39);
            this.tbTemplatingEngine.Name = "tbTemplatingEngine";
            this.tbTemplatingEngine.Size = new System.Drawing.Size(100, 22);
            this.tbTemplatingEngine.TabIndex = 9;
            this.tbTemplatingEngine.TextChanged += new System.EventHandler(this.tbTemplatingEngine_TextChanged);
            // 
            // tbTimeout
            // 
            this.tbTimeout.Location = new System.Drawing.Point(130, 139);
            this.tbTimeout.Name = "tbTimeout";
            this.tbTimeout.Size = new System.Drawing.Size(100, 22);
            this.tbTimeout.TabIndex = 8;
            this.tbTimeout.TextChanged += new System.EventHandler(this.tbTimeout_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label3.Location = new System.Drawing.Point(43, 142);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 20);
            this.label3.TabIndex = 7;
            this.label3.Text = "Timeout";
            // 
            // lblSchemaFilePath
            // 
            this.lblSchemaFilePath.AutoSize = true;
            this.lblSchemaFilePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblSchemaFilePath.Location = new System.Drawing.Point(311, 96);
            this.lblSchemaFilePath.Name = "lblSchemaFilePath";
            this.lblSchemaFilePath.Size = new System.Drawing.Size(102, 20);
            this.lblSchemaFilePath.TabIndex = 6;
            this.lblSchemaFilePath.Text = "Schema File";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.Location = new System.Drawing.Point(43, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(219, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "Chose json file with schema";
            // 
            // btnPreview
            // 
            this.btnPreview.Location = new System.Drawing.Point(47, 223);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(108, 23);
            this.btnPreview.TabIndex = 10;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
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
        private System.Windows.Forms.TextBox tbTemplatingEngine;
        private System.Windows.Forms.Button btnPreview;


    }
}
