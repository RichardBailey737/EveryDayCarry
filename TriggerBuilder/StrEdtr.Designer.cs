namespace Ensur_Trigger_Builder
{
    partial class StrEdtr
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
            this.okBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.strTbx = new System.Windows.Forms.TextBox();
            this.ppJsonBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // okBtn
            // 
            this.okBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okBtn.Location = new System.Drawing.Point(12, 221);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(75, 23);
            this.okBtn.TabIndex = 0;
            this.okBtn.Text = "Ok";
            this.okBtn.UseVisualStyleBackColor = true;
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cancelBtn.Location = new System.Drawing.Point(485, 221);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 1;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // strTbx
            // 
            this.strTbx.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.strTbx.Location = new System.Drawing.Point(12, 12);
            this.strTbx.Multiline = true;
            this.strTbx.Name = "strTbx";
            this.strTbx.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.strTbx.Size = new System.Drawing.Size(548, 203);
            this.strTbx.TabIndex = 2;
            this.strTbx.WordWrap = false;
            // 
            // ppJsonBtn
            // 
            this.ppJsonBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ppJsonBtn.Location = new System.Drawing.Point(237, 221);
            this.ppJsonBtn.Name = "ppJsonBtn";
            this.ppJsonBtn.Size = new System.Drawing.Size(75, 23);
            this.ppJsonBtn.TabIndex = 3;
            this.ppJsonBtn.Text = "Pretty print json";
            this.ppJsonBtn.UseVisualStyleBackColor = true;
            this.ppJsonBtn.Click += new System.EventHandler(this.ppJsonBtn_Click);
            // 
            // StrEdtr
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(572, 279);
            this.ControlBox = false;
            this.Controls.Add(this.ppJsonBtn);
            this.Controls.Add(this.strTbx);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.okBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(235, 235);
            this.Name = "StrEdtr";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "String Editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Button cancelBtn;
        public System.Windows.Forms.TextBox strTbx;
        private System.Windows.Forms.Button ppJsonBtn;
    }
}