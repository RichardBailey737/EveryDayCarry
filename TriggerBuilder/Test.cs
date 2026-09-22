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
    public partial class Test : Form
    {
        public Test()
        {
            InitializeComponent();
            propertyReference1.PropertyChosen +=PropertyReference1_PropertyChosen;
            propertyReference1.Initalize();
        }

        private void PropertyReference1_PropertyChosen(string propertyName)
        {
            textBox1.Text = "{" + propertyName + "}";
        }


    }
}
