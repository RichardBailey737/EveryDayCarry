using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ensur_Trigger_Builder
{
    public partial class EventEditor : Form
    {
        public EventEditor(string EditorText)
        {
            InitializeComponent();
            TriggerCriteria = Ensur.Core.Utilities.Triggers.Triggers.TriggerCriteria.FromJSON(EditorText);
            textBox1.Text = TriggerCriteria.Criteria;
            if (TriggerCriteria.GotoStepOnFailure.HasValue) { 
                gotostepTbx.Text = TriggerCriteria.GotoStepOnFailure.Value.ToString();
                nextStepCbx.Checked = false;
            }
            if (TriggerCriteria.GotoStepOnSuccess.HasValue)
            {
                successStepTbx.Text = TriggerCriteria.GotoStepOnSuccess.Value.ToString();
                successStepCbx.Checked = false;
            }
            terminateCbx.Checked = TriggerCriteria.TerminateExecution;
            propertyReference1.Initalize();
            propertyReference1.PropertyChosen +=PropertyReference1_PropertyChosen;
        }

        Ensur.Core.Utilities.Triggers.Triggers.TriggerCriteria TriggerCriteria;

        private void PropertyReference1_PropertyChosen(string propertyName)
        {
            textBox1.AppendText("{" + propertyName + "}");
        }

        TriggerEvent data = null;

  
        public string EventCriteria { get {
                if (!nextStepCbx.Checked && gotostepTbx.Text.Trim().Length > 0)
                {
                    TriggerCriteria.GotoStepOnFailure = Int32.Parse(gotostepTbx.Text.Trim());
                } else
                {
                    TriggerCriteria.GotoStepOnFailure = null;
                }

                if (!successStepCbx.Checked && successStepTbx.Text.Trim().Length > 0)
                {
                    TriggerCriteria.GotoStepOnSuccess = Int32.Parse(successStepTbx.Text.Trim());
                }
                else
                {
                    TriggerCriteria.GotoStepOnSuccess = null;
                }

                TriggerCriteria.TerminateExecution = terminateCbx.Checked;
                TriggerCriteria.Criteria = textBox1.Text;

                return TriggerCriteria.ToJSON(); 
            } }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void nextStepCbx_CheckedChanged(object sender, EventArgs e)
        {
            gotostepTbx.Enabled = !nextStepCbx.Checked;
        }

        private void successStepCbx_CheckedChanged(object sender, EventArgs e)
        {
            successStepTbx.Enabled = !successStepCbx.Checked;
        }
    }
}
