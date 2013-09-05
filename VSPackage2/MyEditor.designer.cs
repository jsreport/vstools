namespace Company.VSPackage2
{
    partial class MyEditor
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
            this.richTextBoxCtrl = new Company.VSPackage2.EditorTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBoxCtrl
            // 
            this.richTextBoxCtrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxCtrl.FilterMouseClickMessages = false;
            this.richTextBoxCtrl.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxCtrl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.richTextBoxCtrl.Name = "richTextBoxCtrl";
            this.richTextBoxCtrl.Size = new System.Drawing.Size(199, 184);
            this.richTextBoxCtrl.TabIndex = 0;
            this.richTextBoxCtrl.Text = "";
            this.richTextBoxCtrl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.richTextBoxCtrl_KeyDown);
            this.richTextBoxCtrl.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.richTextBoxCtrl_KeyPress);
            this.richTextBoxCtrl.MouseEnter += new System.EventHandler(this.richTextBoxCtrl_MouseEnter);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(20, 13);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // MyEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.richTextBoxCtrl);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "MyEditor";
            this.Size = new System.Drawing.Size(200, 185);
            this.ResumeLayout(false);

        }

        #endregion

        private EditorTextBox richTextBoxCtrl;
        private System.Windows.Forms.Button button1;


    }
}
