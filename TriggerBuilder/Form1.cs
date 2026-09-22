using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ensur.Core.Utilities.Database;
using PetaPoco;

namespace Ensur_Trigger_Builder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            cStringTbx.Text = Properties.Settings.Default.LastConnectionString;
            EventTypeSrc.DataSource = EventTypes;
            CallTypesSrc.DataSource = CallTypes;
            eventDataGrd.AutoGenerateColumns = false;

        }

        private void cStringBtn_Click(object sender, EventArgs e)
        {
            string cstring = ShowDialog(this, cStringTbx.Text);

            if (!string.IsNullOrEmpty(cstring))
            {
                cStringTbx.Text =cstring;
                Properties.Settings.Default.LastConnectionString = cStringTbx.Text;
                Properties.Settings.Default.Save();
                eventTypes = null;
                callTypes = null;
            }
        }

        public void LoadData()
        {
            try
            {
                string cstring = string.Join(";", cStringTbx.Text.Split(";".ToCharArray()).Where(c => !c.ToUpper().StartsWith("PROVIDER")));
                db = new Database(new SqlConnection(cstring));
                db.Connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to the database: {ex.Message}");
                return;
            }
            var localdata = db.Fetch<TriggerEvent>();
            localdata.ForEach((datum) => { datum.TrackChanges = true; });
            EventTypeSrc.DataSource = EventTypes;
            CallTypesSrc.DataSource = CallTypes;
            data = new BindingList<TriggerEvent>(localdata.OrderBy(ld=>ld.SEQ).ToList());
            eventDataGrd.DataSource = data;


        }

        Database db = null;
        BindingList<TriggerEvent> data = null;


        private List<DCS_TRIGGER_EVENT_TYPES> eventTypes;
        public List<DCS_TRIGGER_EVENT_TYPES> EventTypes
        {
            get
            {
                if (db != null && eventTypes == null) eventTypes = db.Fetch<DCS_TRIGGER_EVENT_TYPES>();
                return eventTypes;
            }
        }

        private List<DCS_TRIGGER_EVENT_CALL_TYPES> callTypes;
        public List<DCS_TRIGGER_EVENT_CALL_TYPES> CallTypes
        {
            get
            {
                if (db!= null && callTypes == null) callTypes = db.Fetch<DCS_TRIGGER_EVENT_CALL_TYPES>();
                return callTypes;
            }
        }




        public static string ShowDialog(IWin32Window owner,
                                 string connectionString)
        {
            Type dlType = Type.GetTypeFromProgID("DataLinks", true);
            Type acType = Type.GetTypeFromProgID("ADODB.Connection", true);

            object form = Activator.CreateInstance(dlType);
            object connection = Activator.CreateInstance(acType);

            acType.InvokeMember(
                "ConnectionString",
                BindingFlags.Public | BindingFlags.SetProperty,
                null,
                connection,
                new object[] { connectionString }
                );
            object result =
            dlType.InvokeMember(
                "PromptEdit",
                BindingFlags.Public | BindingFlags.InvokeMethod,
                null,
                form,
                new object[] { connection }
                );
            if (result != null && (bool)result)
                return acType.InvokeMember(
                            "ConnectionString",
                            BindingFlags.Public | BindingFlags.GetProperty,
                            null,
                            connection,
                            new object[] { }) as string;

            return null;
        }

        private void loadBtn_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void eventDataGrd_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show($"Data Error on column {e.ColumnIndex} : {e.Exception.Message}");
        }

        private void updtBtn_Click(object sender, EventArgs e)
        {
            if (data.Any(d => d.isDirty))
            {
                if (MessageBox.Show($"Are you sure you want to update {data.Count(d=>d.isDirty)} record(s)?", "Update", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        eventDataGrd.DataSource = null;
                        data.Where(d => d.isDirty).ToList().ForEach(d => db.Save(d));
                        foreach (var d in data)
                        {
                            d.ResetChanges();
                        }
                        eventDataGrd.DataSource = data;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error on save: {ex.Message}");
                    }

                }
            }
        }

        private void eventDataGrd_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var sndr = (DataGridView)sender;
            var col = sndr.Columns[e.ColumnIndex];
            if (col is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                var data = sndr.Rows[e.RowIndex].DataBoundItem as TriggerEvent;
                if (col.Name == "EVENT_CALL")
                {
                    if (data.CALL_TYPE_ID == 4)
                    {
                        APIEditor ae = new APIEditor();
                        ae.Initalize(data.EVENT_CALL);
                        if (ae.ShowDialog() == DialogResult.OK)
                        {
                            data.EVENT_CALL = ae.JSON;
                        }
                    }
                    else
                    {
                        SPEditor ee = new SPEditor(data.EVENT_CALL);
                        if (ee.ShowDialog() == DialogResult.OK)
                        {
                            data.EVENT_CALL = ee.EventCriteria;
                        }
                    }
                } else
                {
                    EventEditor ee = new EventEditor(data.EVENT_CRITERIA);
                    if (ee.ShowDialog() == DialogResult.OK)
                    {
                        data.EVENT_CRITERIA = ee.EventCriteria;
                    }
                }
            }
        }

        private void addNewBtn_Click(object sender, EventArgs e)
        {
            eventDataGrd.DataSource = null;
            data.Add(new TriggerEvent() { EVENT_DESCRIPTION = "New Event", TrackChanges = true, EVENT_TYPE_ID = 1, CALL_TYPE_ID = 1 });
            eventDataGrd.DataSource = data;

        }

        private void triggerBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not implemented yet");

        }

        private void eventDataGrd_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            var sndr = (DataGridView)sender;
            var col = sndr.Columns[e.ColumnIndex];
            var data = sndr.Rows[e.RowIndex].DataBoundItem as TriggerEvent;
            if (data == null) data = new TriggerEvent();
            data.TrackChanges =true;
        }

        private void eventDataGrd_NewRowNeeded(object sender, DataGridViewRowEventArgs e)
        {
            data.AllowNew = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var sql = "";

            foreach (DataGridViewRow item in eventDataGrd.SelectedRows)
            {
                TriggerEvent te = (TriggerEvent)item.DataBoundItem;
                string seqNo = te.SEQ.ToString();
                if (te.ModifiedProperties.Any(p => p.FieldName == "SEQ")) seqNo = te.ModifiedProperties.Single(p => p.FieldName == "SEQ").OldValue;
                sql += $@" IF NOT EXISTS (SELECT TOP 1 1 FROM dbo.DCS_TRIGGER_EVENT dte WHERE SEQ = {te.SEQ} AND dte.CHAIN_ID = {te.CHAIN_ID}) 
                                INSERT INTO DCS_TRIGGER_EVENT(SEQ, CHAIN_ID, CALL_TYPE_ID) VALUES({te.SEQ}, {te.CHAIN_ID}, {te.CALL_TYPE_ID});
                          UPDATE DCS_TRIGGER_EVENT SET EVENT_TYPE_ID = {te.EVENT_TYPE_ID} , SEQ = {te.SEQ}, EVENT_DESCRIPTION = '{te.EVENT_DESCRIPTION.Replace("'", "''")}', EVENT_CALL = '{te.EVENT_CALL.Replace("'", "''")}', ACTIVE = {te.ACTIVE}, CALL_TYPE_ID = {te.CALL_TYPE_ID}, EVENT_CRITERIA = '{te.EVENT_CRITERIA?.Replace("'", "''")}', CHAIN_ID = {te.CHAIN_ID} WHERE CHAIN_ID = {te.CHAIN_ID} AND SEQ = {seqNo} ";
                sql = PoorMansTSqlFormatterRedux.SqlFormattingManager.DefaultFormat(sql);

            }

            Clipboard.SetText(sql);
            MessageBox.Show("Copied to clipboard");


            //PoorMansTSqlFormatterRedux.SqlFormattingManager.DefaultFormat()
        }
    }


}
