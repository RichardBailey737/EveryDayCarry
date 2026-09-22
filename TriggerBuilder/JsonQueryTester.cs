using JsonPrettyPrinterPlus;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ensur_Trigger_Builder
{
    public partial class JsonQueryTester : Form
    {
        public JsonQueryTester()
        {
            InitializeComponent();
        }
        string rslts = "";
        private void chooseBtn_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                jsonFileTbx.Text = ofd.FileName;
                rslts = File.ReadAllText(ofd.FileName);
                JsonFileTextTbx.Text = rslts.PrettyPrintJson();
            }
        }

        private void queryBtn_Click(object sender, EventArgs e)
        {
            try
            {
                JToken obj;
                if (rslts.First() == '[')
                {
                    obj = JArray.Parse(rslts);
                }
                else
                {
                    obj = JObject.Parse(rslts);
                }

                resultTbx.Text = obj.SelectToken(queryTbx.Text).ToString();

            }
            catch (Exception ex)
            {
                resultTbx.Text = ex.Message + " " + ex.StackTrace;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkLabel1.Text);
        }
    }
}
