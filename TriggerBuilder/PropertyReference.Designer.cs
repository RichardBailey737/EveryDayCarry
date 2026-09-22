namespace Ensur_Trigger_Builder
{
    partial class PropertyReference
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
            this.searchBox = new System.Windows.Forms.TextBox();
            this.propertyLbx = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // searchBox
            // 
            this.searchBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchBox.Location = new System.Drawing.Point(0, 0);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(171, 20);
            this.searchBox.TabIndex = 0;
            this.searchBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.searchBox_KeyDown);
            // 
            // propertyLbx
            // 
            this.propertyLbx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyLbx.FormattingEnabled = true;
            this.propertyLbx.Location = new System.Drawing.Point(0, 20);
            this.propertyLbx.Name = "propertyLbx";
            this.propertyLbx.Size = new System.Drawing.Size(171, 151);
            this.propertyLbx.TabIndex = 1;
            this.propertyLbx.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.propertyLbx_MouseDoubleClick);
            // 
            // PropertyReference
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.propertyLbx);
            this.Controls.Add(this.searchBox);
            this.Name = "PropertyReference";
            this.Size = new System.Drawing.Size(171, 171);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.ListBox propertyLbx;
    }
}
