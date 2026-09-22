using Ensur.Core.Utilities;
using Ensur.Core.Utilities.Database;
using Newtonsoft.Json;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Ensur_Trigger_Builder
{
    public class EnsurPocoBase 
    {
        public EnsurPocoBase()
        {
            ModifiedProperties = new List<AuditTrailEntry>();
            logger = NLog.LogManager.GetCurrentClassLogger();
            DoNotAudit = new List<string>();
            OnlyUpdateModified = false;
            TrackChanges = false;
        }

        List<string> DoNotAudit;
        private NLog.Logger logger;
        private bool? isnew = null;

        [Ignore]
        [XmlIgnore]
        [JsonIgnore]
        public List<AuditTrailEntry> ModifiedProperties { get; set; }

        [Ignore]
        [XmlIgnore]
        public bool isDirty { get; set; }

        [Ignore]
        [XmlIgnore]
        [JsonIgnore]
        public bool TrackChanges { get; set; }

        [Ignore]
        [XmlIgnore]
        public bool OnlyUpdateModified { get; set; }


        [Ignore]
        [XmlIgnore]
        [JsonIgnore]
        public int ModifiedBy { get; set; }

        /// <summary>
        /// Compares the modified properties in the modified properties list to the properties from the database object.  Any values tha tare different from the database are set as the old value.
        /// Intended to be run before updating to track changed values.
        /// </summary>
        /// <param name="DatabaseObject"></param>
        public void TrackDifferences(EnsurPocoBase DatabaseObject)
        {
            var props = DatabaseObject.GetType().GetProperties();
            foreach (var change in ModifiedProperties)
            {
                var prop = props.Single(p => p.Name.ToUpper() == change.FieldName.ToUpper());
                change.OldValue = prop.GetValue(DatabaseObject).ToString();
            }
        }

        public bool IsNew()
        {

            var pk = this.GetType().GetCustomAttributes(true).Where(attr => attr is PrimaryKeyAttribute).FirstOrDefault();

            if (pk != null)
            {
                string propname = (pk as PrimaryKeyAttribute).Value;

                var rprop = this.GetType().GetProperties().Where(prop => prop.Name.ToUpper() == propname.ToUpper()).FirstOrDefault();

                object dflt = null;

                if (rprop.PropertyType.IsValueType) dflt = Activator.CreateInstance(rprop.PropertyType);

                var obj = rprop.GetValue(this);
                return obj.Equals(dflt);
            }
            else
            {
                logger.Error("No primary key defined in object");
                throw new UserError("No primary key defined");
            }

            //output = $"Primary key value:{obj}({rprop.PropertyType.ToString()}) != default value:{dflt}";

        }

        public object PrimaryKeyValue()
        {
            var pk = this.GetType().GetCustomAttributes(true).Where(attr => attr is PrimaryKeyAttribute).FirstOrDefault();

            if (pk != null)
            {
                string propname = (pk as PrimaryKeyAttribute).Value;

                var rprop = this.GetType().GetProperties().Where(prop => prop.Name.ToUpper() == propname.ToUpper()).FirstOrDefault();


                return rprop.GetValue(this);
            }
            else
            {
                return null;
            }
        }

        public string PrimaryKeyName()
        {
            var pk = this.GetType().GetCustomAttributes(true).Where(attr => attr is PrimaryKeyAttribute).FirstOrDefault();
            if (pk != null)
            {
                return (pk as PrimaryKeyAttribute).Value;
            }
            else
            {
                return null;
            }
        }

        protected void SetField<T>(ref T field, T value, string propertyName)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                if (DoNotAudit.Contains(propertyName.ToUpper())) return;
                if (TrackChanges && !ModifiedProperties.Any(p => p.FieldName == propertyName.ToUpper()))
                {
                    ModifiedProperties.Add(new AuditTrailEntry() { FieldName = propertyName, NewValue = value?.ToString() });
                    isDirty = true;
                }
                field = value;
            }
        }


        /// <summary>
        /// For validation.  Determines if a field is going to be used or not.  Only matters during partial updates.  
        /// </summary>
        /// <param name="FieldName">The field name to check</param>
        /// <returns>bool - True = Field has been set or field is going to be used as part of the update.</returns>
        public bool IsFieldSet(string FieldName)
        {
            if (IsNew() || !OnlyUpdateModified) return true;
            return ModifiedProperties.Any(mp => mp.FieldName.ToUpper() == FieldName.ToUpper());
        }

       
        public void ResetChanges()
        {
            isDirty = false;
            ModifiedProperties.Clear();
        }


        public string ToJSONString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static EnsurPocoBase FromJSON(string json)
        {
            return JsonConvert.DeserializeObject<EnsurPocoBase>(json);
        }


        public class AuditTrailEntry
        {
            public string FieldName { get; set; }
            public string OldValue { get; set; }
            public string NewValue { get; set; }


        }
    }
}
