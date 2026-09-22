using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Ensur.Core.Utilities.Triggers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using StringTokenFormatter;
using Wexman.Design;

namespace Ensur_Trigger_Builder
{
    public partial class APIEditor : Form
    {
        public APIEditor()
        {
            InitializeComponent();
            
            var privder = new AssociatedMetadataTypeTypeDescriptionProvider(typeof(APICall), typeof(APICallMetaData));
            TypeDescriptor.AddProvider(privder, typeof(APICall));

            apicall = new APICall();
            propertyGrid1.SelectedObject = apicall;

            testData = new APITestData();
            inputData.SelectedObject = testData;
            inputData.HelpVisible = false;
            inputData.PropertySort = PropertySort.Alphabetical;
            propertyReference1.PropertyChosen +=PropertyReference1_PropertyChosen;
        }

        private void PropertyReference1_PropertyChosen(string propertyName)
        {
            Clipboard.SetText("{" + propertyName + "}");
            MessageBox.Show("Property copied to clipboard");
        }

        APITestData testData = null;
        APICall apicall = null;

        public void Initalize(string JSON)
        {
            propertyReference1.Initalize();

            if (!string.IsNullOrEmpty(Properties.Settings.Default.TestData))
            {
                testData = JsonConvert.DeserializeObject<APITestData>(Properties.Settings.Default.TestData);
                inputData.SelectedObject = testData;
            }

            this.JSON = JSON;
            try
            {
                if (!string.IsNullOrEmpty(JSON))
                {
                    apicall = APICall.FromJSON(JSON);
                    propertyGrid1.SelectedObject = apicall;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error converting string to API Call: {ex.Message}");
            }
        }

        public string JSON { get; set; }


        private void okBtn_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.TestData = JsonConvert.SerializeObject(testData);
            Properties.Settings.Default.Save();
            this.JSON = apicall.ToJSON();
            this.DialogResult = DialogResult.OK;
            this.Close();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void testBtn_Click(object sender, EventArgs e)
        {
            //Properties get modified during execution.  Copy over the object first so they don't change the database properties
            APICall newCall = APICall.FromJSON(apicall.ToJSON());

            try
            {
                Log($"Call started -----{DateTime.Now.ToLongTimeString()}-----");


                var results = newCall.ExecuteCall(testData.TestData);
                Log($"Call completed -----{DateTime.Now.ToLongTimeString()}-----");
                string output = string.Join(Environment.NewLine, results.Select(key => $"{key.Key}={key.Value}").ToArray());
                Log(output);
            }
            catch (Exception ex)
            {
                Log($"Error executing API call: {ex.Message}");
               
            }
        }

        public void Log(string text)
        {
            testResultTbx.AppendText(text + Environment.NewLine);
            testResultTbx.SelectionStart = testResultTbx.TextLength;
            testResultTbx.ScrollToCaret();
        }


        public void RoughTest()
        {
            string RestURL = apicall.RestURL;
            var client = new RestClient(RestURL.FormatDictionary<string>(testData.TestData));
            Dictionary<string, string> resultData = new Dictionary<string, string>();
            

            var request = new RestRequest("", (Method)Enum.Parse(typeof(Method), apicall.CallMethod));

            request.RequestFormat = DataFormat.Json;

            if (apicall.Parameters != null && apicall.Parameters.Count > 0)
            {
                foreach (var Param in apicall.Parameters)
                {
                    request.AddParameter(Param.Key, Param.Value.FormatDictionary<string>(testData.TestData));
                }
            }

            if (apicall.Headers != null && apicall.Headers.Count > 0)
            {
                foreach (var header in apicall.Headers)
                {
                    request.AddHeader(header.Key, header.Value.FormatDictionary<string>(testData.TestData));
                }
            }

            //Option to upload more than one file at a time.
            //Pass in a data item with the format WantedFileName.doc|c:\pathtoFile\ActualFileName.doc,SecondWantedFileName.doc|c:\pathtofile\SecondActualFilename.doc
            if (testData.TestData.ContainsKey("FilesToPost"))
            {
                var splt = testData.TestData["FilesToPost"].Split(',');
                foreach (var file in splt)
                {
                    var fileNameSplit = file.Split('|');
                    request.Files.Add(new FileParameter
                    {
                        Name = fileNameSplit[0],
                        Writer = (s) =>
                        {
                            var stream = System.IO.File.OpenRead(fileNameSplit[1]);
                            stream.CopyTo(s);
                            stream.Dispose();
                        },
                        FileName = fileNameSplit[0],

                    });
                }

            }

            if (apicall.FilesToPost != null && apicall.FilesToPost.Count > 0)
            {
                foreach (var file in apicall.FilesToPost)
                {
                    var fi = new System.IO.FileInfo(file.Value.FormatDictionary<string>(testData.TestData));

                    //There is an easier (request.AddFile) way to do this, however it uses the name of the physical file
                    request.Files.Add(new FileParameter
                    {
                        Name = file.Key.FormatDictionary<string>(testData.TestData),
                        Writer = (s) =>
                        {
                            var stream = System.IO.File.OpenRead(file.Value.FormatDictionary<string>(testData.TestData));
                            stream.CopyTo(s);
                            stream.Dispose();
                        },
                        FileName = file.Key.FormatDictionary<string>(testData.TestData)
                        ,
                        ContentLength = fi.Length

                    });
                    //request.AddFile(file.Key.FormatDictionary<string>(Data), file.Value.FormatDictionary<string>(Data));
                }
            }

            if (request.RequestFormat == DataFormat.Json)
            {
                request.AddJsonBody(textBox1.Text);
                
            } else
            {
                request.AddXmlBody(textBox1.Text);
            }

            var rslts = client.Execute(request);

            if (rslts != null && rslts.IsSuccessful && rslts.Content.Length > 0 && rslts.ContentType.ToUpper().Trim() == "APPLICATION/JSON" && apicall.JsonResultQueries.Count > 0)
            {
                JToken obj;
                if (rslts.Content.First() == '[')
                {
                    obj = JArray.Parse(rslts.Content);
                }
                else
                {
                    obj = JObject.Parse(rslts.Content);
                }
                foreach (var jquery in apicall.JsonResultQueries)
                {
                    string val = (string)obj.SelectToken(jquery.Value);
                    resultData[jquery.Key] = val;
                }
            }

            Log($"Status: {rslts.StatusCode}");
            if (resultData.Count > 0)
            {
                Log("-----JSON Query Results-----");
                foreach (var rslt in resultData)
                {
                    Log(rslt.ToString());
                }
            }
            Log("-----Results-----");
            Log(rslts.Content);
        }

        private void roughTestBtn_Click(object sender, EventArgs e)
        {
            try
            {
                RoughTest();
            }
            catch (Exception ex)
            {
                Log($"Error during manual test: {ex.Message}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            JsonQueryTester tester = new JsonQueryTester();
            tester.Show();
        }
    }


    public class APITestData
    {
        public APITestData()
        {
            TestData = new Dictionary<string, string>();
        }

        [Editor(typeof(GenericDictionaryEditor<string, string>), typeof(UITypeEditor))]
        public Dictionary<string, string> TestData { get; set; }

    }

    public class APICallMetaData
    {

        [Editor(typeof( GenericDictionaryEditor<string, string>), typeof(UITypeEditor))]
        public Dictionary<string, string> Parameters { get; set; }


        [Editor(typeof(GenericDictionaryEditor<string, string>), typeof(UITypeEditor))]
        public Dictionary<string, string> Headers { get; set; }


        [Editor(typeof(GenericDictionaryEditor<string, string>), typeof(UITypeEditor))]
        public Dictionary<string, string> JsonResultQueries { get; set; }

        [Editor(typeof(GenericDictionaryEditor<string, string>), typeof(UITypeEditor))]
        public Dictionary<string, string> FilesToPost { get; set; }


        [Editor(typeof(StringEditor), typeof(UITypeEditor))]
        public object RequestBody { get; set;  }
    }

    class StringEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService svc = (IWindowsFormsEditorService)
                provider.GetService(typeof(IWindowsFormsEditorService));
            if (svc != null)
            {
                var str = new StrEdtr();
                str.strTbx.Text = value?.ToString();
                if (svc.ShowDialog(str) == DialogResult.OK)
                {

                    value = Regex.Replace(Regex.Replace(str.strTbx.Text, "[\r\n\t]+", " "), "\\s+", " ");
                }
            }
            return value;
        }
    }
}
