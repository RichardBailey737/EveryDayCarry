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
    public partial class SPEditor : Form
    {
        public SPEditor(string EditorText)
        {
            InitializeComponent();
            textBox1.Text = EditorText;
            propertyReference1.Initalize();
            propertyReference1.PropertyChosen +=PropertyReference1_PropertyChosen;
        }

        Ensur.Core.Utilities.Triggers.Triggers.TriggerCriteria TriggerCriteria;

        private void PropertyReference1_PropertyChosen(string propertyName)
        {
            textBox1.AppendText("{" + propertyName + "}");
        }


        public string EventCriteria
        {
            get
            {
                return textBox1.Text;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

    }
}
