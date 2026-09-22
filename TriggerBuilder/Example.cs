using Ensur.Core.Utilities.Database;
using PetaPoco;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace Ensur_Trigger_Builder
{
    public partial class Example : Form
    {
        public Example()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var cstring = Properties.Settings.Default.LastConnectionString;
            cstring = string.Join(";", cstring.Split(";".ToCharArray()).Where(c => !c.ToUpper().StartsWith("PROVIDER")));
            var db = new Database(new SqlConnection(cstring));
            db.Connection.Open();
            var data = db.Fetch<DCS_TRIGGER_EVENT>();
            dataGridView1.DataSource = data;
        }
    }
}
