namespace Ensur_Trigger_Builder
{
    partial class IntelliTextBox
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("BigParser");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Stuff1", new System.Windows.Forms.TreeNode[] {
            treeNode1});
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("TreeChopper");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Stuff2", new System.Windows.Forms.TreeNode[] {
            treeNode3});
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("AddFile");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("DeleteFile");
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("RenameFile");
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("FileCreater", new System.Windows.Forms.TreeNode[] {
            treeNode5,
            treeNode6,
            treeNode7});
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("AddThing");
            System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("GraphicsEngine", new System.Windows.Forms.TreeNode[] {
            treeNode9});
            System.Windows.Forms.TreeNode treeNode11 = new System.Windows.Forms.TreeNode("Widgets", new System.Windows.Forms.TreeNode[] {
            treeNode8,
            treeNode10});
            System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("EvenMoreStuff", new System.Windows.Forms.TreeNode[] {
            treeNode11});
            System.Windows.Forms.TreeNode treeNode13 = new System.Windows.Forms.TreeNode("CodeProject", new System.Windows.Forms.TreeNode[] {
            treeNode2,
            treeNode4,
            treeNode12});
            this.textBoxTooltip = new System.Windows.Forms.TextBox();
            this.treeViewItems = new System.Windows.Forms.TreeView();
            this.isTbx = new System.Windows.Forms.RichTextBox();
            this.listBoxAutoComplete = new Ensur_Trigger_Builder.GListBox();
            this.SuspendLayout();
            // 
            // textBoxTooltip
            // 
            this.textBoxTooltip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(225)))));
            this.textBoxTooltip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxTooltip.Location = new System.Drawing.Point(184, 115);
            this.textBoxTooltip.Multiline = true;
            this.textBoxTooltip.Name = "textBoxTooltip";
            this.textBoxTooltip.ReadOnly = true;
            this.textBoxTooltip.Size = new System.Drawing.Size(100, 20);
            this.textBoxTooltip.TabIndex = 6;
            this.textBoxTooltip.Visible = false;
            // 
            // treeViewItems
            // 
            this.treeViewItems.Dock = System.Windows.Forms.DockStyle.Right;
            this.treeViewItems.FullRowSelect = true;
            this.treeViewItems.Location = new System.Drawing.Point(451, 0);
            this.treeViewItems.Name = "treeViewItems";
            treeNode1.Name = "";
            treeNode1.Text = "BigParser";
            treeNode2.Name = "";
            treeNode2.Text = "Stuff1";
            treeNode3.Name = "";
            treeNode3.Text = "TreeChopper";
            treeNode4.Name = "";
            treeNode4.Text = "Stuff2";
            treeNode5.Name = "";
            treeNode5.Text = "AddFile";
            treeNode6.Name = "";
            treeNode6.Text = "DeleteFile";
            treeNode7.Name = "";
            treeNode7.Text = "RenameFile";
            treeNode8.Name = "";
            treeNode8.Text = "FileCreater";
            treeNode9.Name = "";
            treeNode9.Text = "AddThing";
            treeNode10.Name = "";
            treeNode10.Text = "GraphicsEngine";
            treeNode11.Name = "";
            treeNode11.Text = "Widgets";
            treeNode12.Name = "";
            treeNode12.Text = "EvenMoreStuff";
            treeNode13.Name = "";
            treeNode13.Text = "CodeProject";
            this.treeViewItems.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode13});
            this.treeViewItems.PathSeparator = ".";
            this.treeViewItems.Size = new System.Drawing.Size(145, 303);
            this.treeViewItems.TabIndex = 8;
            // 
            // isTbx
            // 
            this.isTbx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.isTbx.Location = new System.Drawing.Point(0, 0);
            this.isTbx.Name = "isTbx";
            this.isTbx.Size = new System.Drawing.Size(451, 303);
            this.isTbx.TabIndex = 9;
            this.isTbx.Text = "";
            this.isTbx.KeyDown += new System.Windows.Forms.KeyEventHandler(this.isTbx_KeyDown);
            // 
            // listBoxAutoComplete
            // 
            this.listBoxAutoComplete.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxAutoComplete.FormattingEnabled = true;
            this.listBoxAutoComplete.ImageList = null;
            this.listBoxAutoComplete.Location = new System.Drawing.Point(79, 158);
            this.listBoxAutoComplete.Name = "listBoxAutoComplete";
            this.listBoxAutoComplete.Size = new System.Drawing.Size(205, 69);
            this.listBoxAutoComplete.TabIndex = 7;
            this.listBoxAutoComplete.Visible = false;
            // 
            // IntelliTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listBoxAutoComplete);
            this.Controls.Add(this.textBoxTooltip);
            this.Controls.Add(this.isTbx);
            this.Controls.Add(this.treeViewItems);
            this.Name = "IntelliTextBox";
            this.Size = new System.Drawing.Size(596, 303);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBoxTooltip;
        private GListBox listBoxAutoComplete;
        private System.Windows.Forms.TreeView treeViewItems;
        private System.Windows.Forms.RichTextBox isTbx;
    }
}
