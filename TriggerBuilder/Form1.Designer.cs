namespace Ensur_Trigger_Builder
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.cStringTbx = new System.Windows.Forms.TextBox();
            this.cStringBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.eventDataGrd = new System.Windows.Forms.DataGridView();
            this.EVENT_ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CALL_TYPE_ID = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.CallTypesSrc = new System.Windows.Forms.BindingSource(this.components);
            this.EVENT_TYPE_ID = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.EventTypeSrc = new System.Windows.Forms.BindingSource(this.components);
            this.SEQ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EVENT_DESCRIPTION = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EVENT_CALL = new System.Windows.Forms.DataGridViewButtonColumn();
            this.EVENT_CRTIERIA = new System.Windows.Forms.DataGridViewButtonColumn();
            this.ACTIVE = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.CHAIN_ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.loadBtn = new System.Windows.Forms.Button();
            this.updtBtn = new System.Windows.Forms.Button();
            this.addNewBtn = new System.Windows.Forms.Button();
            this.triggerBtn = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.eventDataGrd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CallTypesSrc)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.EventTypeSrc)).BeginInit();
            this.SuspendLayout();
            // 
            // cStringTbx
            // 
            this.cStringTbx.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cStringTbx.Location = new System.Drawing.Point(109, 12);
            this.cStringTbx.Multiline = true;
            this.cStringTbx.Name = "cStringTbx";
            this.cStringTbx.Size = new System.Drawing.Size(901, 107);
            this.cStringTbx.TabIndex = 0;
            // 
            // cStringBtn
            // 
            this.cStringBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cStringBtn.Location = new System.Drawing.Point(1016, 15);
            this.cStringBtn.Name = "cStringBtn";
            this.cStringBtn.Size = new System.Drawing.Size(122, 23);
            this.cStringBtn.TabIndex = 1;
            this.cStringBtn.Text = "Build Connection";
            this.cStringBtn.UseVisualStyleBackColor = true;
            this.cStringBtn.Click += new System.EventHandler(this.cStringBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Connection String";
            // 
            // eventDataGrd
            // 
            this.eventDataGrd.AllowUserToAddRows = false;
            this.eventDataGrd.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.eventDataGrd.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.eventDataGrd.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.EVENT_ID,
            this.CALL_TYPE_ID,
            this.EVENT_TYPE_ID,
            this.SEQ,
            this.EVENT_DESCRIPTION,
            this.EVENT_CALL,
            this.EVENT_CRTIERIA,
            this.ACTIVE,
            this.CHAIN_ID});
            this.eventDataGrd.Location = new System.Drawing.Point(12, 131);
            this.eventDataGrd.Name = "eventDataGrd";
            this.eventDataGrd.Size = new System.Drawing.Size(1126, 582);
            this.eventDataGrd.TabIndex = 3;
            this.eventDataGrd.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.eventDataGrd_CellContentClick);
            this.eventDataGrd.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.eventDataGrd_DataError);
            this.eventDataGrd.NewRowNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.eventDataGrd_NewRowNeeded);
            this.eventDataGrd.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.eventDataGrd_RowEnter);
            // 
            // EVENT_ID
            // 
            this.EVENT_ID.DataPropertyName = "EVENT_ID";
            this.EVENT_ID.HeaderText = "Event ID";
            this.EVENT_ID.Name = "EVENT_ID";
            this.EVENT_ID.ReadOnly = true;
            this.EVENT_ID.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.EVENT_ID.Width = 50;
            // 
            // CALL_TYPE_ID
            // 
            this.CALL_TYPE_ID.DataPropertyName = "CALL_TYPE_ID";
            this.CALL_TYPE_ID.DataSource = this.CallTypesSrc;
            this.CALL_TYPE_ID.DisplayMember = "CALL_TYPE_NAME";
            this.CALL_TYPE_ID.HeaderText = "Call Type";
            this.CALL_TYPE_ID.Name = "CALL_TYPE_ID";
            this.CALL_TYPE_ID.ValueMember = "CALL_TYPE_ID";
            // 
            // CallTypesSrc
            // 
            this.CallTypesSrc.DataSource = typeof(Ensur.Core.Utilities.Database.DCS_TRIGGER_EVENT_CALL_TYPES);
            // 
            // EVENT_TYPE_ID
            // 
            this.EVENT_TYPE_ID.DataPropertyName = "EVENT_TYPE_ID";
            this.EVENT_TYPE_ID.DataSource = this.EventTypeSrc;
            this.EVENT_TYPE_ID.DisplayMember = "EVENT_TYPE";
            this.EVENT_TYPE_ID.HeaderText = "Event Type";
            this.EVENT_TYPE_ID.Name = "EVENT_TYPE_ID";
            this.EVENT_TYPE_ID.ValueMember = "EVENT_TYPE_ID";
            this.EVENT_TYPE_ID.Width = 250;
            // 
            // EventTypeSrc
            // 
            this.EventTypeSrc.AllowNew = false;
            this.EventTypeSrc.DataSource = typeof(Ensur.Core.Utilities.Database.DCS_TRIGGER_EVENT_TYPES);
            // 
            // SEQ
            // 
            this.SEQ.DataPropertyName = "SEQ";
            this.SEQ.HeaderText = "Event Sequence";
            this.SEQ.Name = "SEQ";
            this.SEQ.Width = 50;
            // 
            // EVENT_DESCRIPTION
            // 
            this.EVENT_DESCRIPTION.DataPropertyName = "EVENT_DESCRIPTION";
            this.EVENT_DESCRIPTION.HeaderText = "Event Description";
            this.EVENT_DESCRIPTION.Name = "EVENT_DESCRIPTION";
            this.EVENT_DESCRIPTION.Width = 300;
            // 
            // EVENT_CALL
            // 
            this.EVENT_CALL.HeaderText = "Event Call";
            this.EVENT_CALL.Name = "EVENT_CALL";
            this.EVENT_CALL.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.EVENT_CALL.Text = "View/Edit";
            this.EVENT_CALL.UseColumnTextForButtonValue = true;
            // 
            // EVENT_CRTIERIA
            // 
            this.EVENT_CRTIERIA.HeaderText = "Event Criteria";
            this.EVENT_CRTIERIA.Name = "EVENT_CRTIERIA";
            this.EVENT_CRTIERIA.Text = "View/Edit";
            this.EVENT_CRTIERIA.UseColumnTextForButtonValue = true;
            // 
            // ACTIVE
            // 
            this.ACTIVE.DataPropertyName = "ACTIVE";
            this.ACTIVE.HeaderText = "Active";
            this.ACTIVE.Name = "ACTIVE";
            this.ACTIVE.Width = 50;
            // 
            // CHAIN_ID
            // 
            this.CHAIN_ID.DataPropertyName = "CHAIN_ID";
            this.CHAIN_ID.HeaderText = "Event Chain";
            this.CHAIN_ID.Name = "CHAIN_ID";
            // 
            // loadBtn
            // 
            this.loadBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.loadBtn.Location = new System.Drawing.Point(1016, 44);
            this.loadBtn.Name = "loadBtn";
            this.loadBtn.Size = new System.Drawing.Size(122, 23);
            this.loadBtn.TabIndex = 4;
            this.loadBtn.Text = "Load Data";
            this.loadBtn.UseVisualStyleBackColor = true;
            this.loadBtn.Click += new System.EventHandler(this.loadBtn_Click);
            // 
            // updtBtn
            // 
            this.updtBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.updtBtn.Location = new System.Drawing.Point(381, 719);
            this.updtBtn.Name = "updtBtn";
            this.updtBtn.Size = new System.Drawing.Size(318, 41);
            this.updtBtn.TabIndex = 5;
            this.updtBtn.Text = "Save Changes";
            this.updtBtn.UseVisualStyleBackColor = true;
            this.updtBtn.Click += new System.EventHandler(this.updtBtn_Click);
            // 
            // addNewBtn
            // 
            this.addNewBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.addNewBtn.Location = new System.Drawing.Point(170, 719);
            this.addNewBtn.Name = "addNewBtn";
            this.addNewBtn.Size = new System.Drawing.Size(205, 41);
            this.addNewBtn.TabIndex = 6;
            this.addNewBtn.Text = "Add Record";
            this.addNewBtn.UseVisualStyleBackColor = true;
            this.addNewBtn.Click += new System.EventHandler(this.addNewBtn_Click);
            // 
            // triggerBtn
            // 
            this.triggerBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.triggerBtn.Location = new System.Drawing.Point(705, 719);
            this.triggerBtn.Name = "triggerBtn";
            this.triggerBtn.Size = new System.Drawing.Size(255, 41);
            this.triggerBtn.TabIndex = 7;
            this.triggerBtn.Text = "Trigger Event";
            this.triggerBtn.UseVisualStyleBackColor = true;
            this.triggerBtn.Click += new System.EventHandler(this.triggerBtn_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(966, 719);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(172, 41);
            this.button1.TabIndex = 8;
            this.button1.Text = "Generate SQL";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1150, 763);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.triggerBtn);
            this.Controls.Add(this.addNewBtn);
            this.Controls.Add(this.updtBtn);
            this.Controls.Add(this.loadBtn);
            this.Controls.Add(this.eventDataGrd);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cStringBtn);
            this.Controls.Add(this.cStringTbx);
            this.Name = "Form1";
            this.Text = "Event Editor";
            ((System.ComponentModel.ISupportInitialize)(this.eventDataGrd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CallTypesSrc)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.EventTypeSrc)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox cStringTbx;
        private System.Windows.Forms.Button cStringBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView eventDataGrd;
        private System.Windows.Forms.BindingSource EventTypeSrc;
        private System.Windows.Forms.BindingSource CallTypesSrc;
        private System.Windows.Forms.Button loadBtn;
        private System.Windows.Forms.Button updtBtn;
        private System.Windows.Forms.Button addNewBtn;
        private System.Windows.Forms.Button triggerBtn;
        private System.Windows.Forms.DataGridViewTextBoxColumn EVENT_ID;
        private System.Windows.Forms.DataGridViewComboBoxColumn CALL_TYPE_ID;
        private System.Windows.Forms.DataGridViewComboBoxColumn EVENT_TYPE_ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn SEQ;
        private System.Windows.Forms.DataGridViewTextBoxColumn EVENT_DESCRIPTION;
        private System.Windows.Forms.DataGridViewButtonColumn EVENT_CALL;
        private System.Windows.Forms.DataGridViewButtonColumn EVENT_CRTIERIA;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ACTIVE;
        private System.Windows.Forms.DataGridViewTextBoxColumn CHAIN_ID;
        private System.Windows.Forms.Button button1;
    }
}

