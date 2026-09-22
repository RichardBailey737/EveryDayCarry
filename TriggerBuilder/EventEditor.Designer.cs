namespace Ensur_Trigger_Builder
{
    partial class EventEditor
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.propertyReference1 = new Ensur_Trigger_Builder.PropertyReference();
            this.terminateCbx = new System.Windows.Forms.CheckBox();
            this.gotostepTbx = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.nextStepCbx = new System.Windows.Forms.CheckBox();
            this.successStepCbx = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.successStepTbx = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBox1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBox1.Location = new System.Drawing.Point(12, 90);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(413, 263);
            this.textBox1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(12, 359);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(403, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Done";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // propertyReference1
            // 
            this.propertyReference1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyReference1.Location = new System.Drawing.Point(431, 12);
            this.propertyReference1.Name = "propertyReference1";
            this.propertyReference1.Size = new System.Drawing.Size(220, 370);
            this.propertyReference1.TabIndex = 2;
            // 
            // terminateCbx
            // 
            this.terminateCbx.AutoSize = true;
            this.terminateCbx.Location = new System.Drawing.Point(13, 13);
            this.terminateCbx.Name = "terminateCbx";
            this.terminateCbx.Size = new System.Drawing.Size(172, 17);
            this.terminateCbx.TabIndex = 3;
            this.terminateCbx.Text = "Terminate Execution on Failure";
            this.terminateCbx.UseVisualStyleBackColor = true;
            // 
            // gotostepTbx
            // 
            this.gotostepTbx.Enabled = false;
            this.gotostepTbx.Location = new System.Drawing.Point(148, 34);
            this.gotostepTbx.Name = "gotostepTbx";
            this.gotostepTbx.Size = new System.Drawing.Size(56, 20);
            this.gotostepTbx.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "On Failure go to event ID: ";
            // 
            // nextStepCbx
            // 
            this.nextStepCbx.AutoSize = true;
            this.nextStepCbx.Checked = true;
            this.nextStepCbx.CheckState = System.Windows.Forms.CheckState.Checked;
            this.nextStepCbx.Location = new System.Drawing.Point(211, 36);
            this.nextStepCbx.Name = "nextStepCbx";
            this.nextStepCbx.Size = new System.Drawing.Size(73, 17);
            this.nextStepCbx.TabIndex = 6;
            this.nextStepCbx.Text = "Next Step";
            this.nextStepCbx.UseVisualStyleBackColor = true;
            this.nextStepCbx.CheckedChanged += new System.EventHandler(this.nextStepCbx_CheckedChanged);
            // 
            // successStepCbx
            // 
            this.successStepCbx.AutoSize = true;
            this.successStepCbx.Checked = true;
            this.successStepCbx.CheckState = System.Windows.Forms.CheckState.Checked;
            this.successStepCbx.Location = new System.Drawing.Point(298, 66);
            this.successStepCbx.Name = "successStepCbx";
            this.successStepCbx.Size = new System.Drawing.Size(73, 17);
            this.successStepCbx.TabIndex = 9;
            this.successStepCbx.Text = "Next Step";
            this.successStepCbx.UseVisualStyleBackColor = true;
            this.successStepCbx.CheckedChanged += new System.EventHandler(this.successStepCbx_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(221, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "On Success (after execution) go to event ID: ";
            // 
            // successStepTbx
            // 
            this.successStepTbx.Enabled = false;
            this.successStepTbx.Location = new System.Drawing.Point(236, 64);
            this.successStepTbx.Name = "successStepTbx";
            this.successStepTbx.Size = new System.Drawing.Size(56, 20);
            this.successStepTbx.TabIndex = 7;
            // 
            // EventEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(658, 393);
            this.Controls.Add(this.successStepCbx);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.successStepTbx);
            this.Controls.Add(this.nextStepCbx);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.gotostepTbx);
            this.Controls.Add(this.terminateCbx);
            this.Controls.Add(this.propertyReference1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Name = "EventEditor";
            this.Text = "Text Editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private PropertyReference propertyReference1;
        private System.Windows.Forms.CheckBox terminateCbx;
        private System.Windows.Forms.TextBox gotostepTbx;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox nextStepCbx;
        private System.Windows.Forms.CheckBox successStepCbx;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox successStepTbx;
    }
}