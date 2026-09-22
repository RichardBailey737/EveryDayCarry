using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JsonPrettyPrinterPlus;

namespace Ensur_Trigger_Builder
{
    public partial class StrEdtr : Form
    {
        public StrEdtr()
        {
            InitializeComponent();
        }

        private void ppJsonBtn_Click(object sender, EventArgs e)
        {

            this.strTbx.Text = strTbx.Text.PrettyPrintJson();
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
