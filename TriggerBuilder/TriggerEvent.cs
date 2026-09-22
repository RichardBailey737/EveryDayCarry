using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensur_Trigger_Builder
{
    [TableName("DCS_TRIGGER_EVENT")]
    [PrimaryKey("EVENT_ID")]
    public class TriggerEvent : EnsurPocoBase
    {

        [Column("EVENT_ID")]
        public Int32 EVENT_ID { get { return _event_id; } set { SetField(ref _event_id, value, "EVENT_ID"); } }
        private Int32 _event_id;

        [Column("EVENT_TYPE_ID")]
        public Int32? EVENT_TYPE_ID { get { return _event_type_id; } set { SetField(ref _event_type_id, value, "EVENT_TYPE_ID"); } }
        private Int32? _event_type_id;

        [Column("SEQ")]
        public Int32? SEQ { get { return _seq; } set { SetField(ref _seq, value, "SEQ"); } }
        private Int32? _seq;

        [Column("EVENT_DESCRIPTION")]
        public String EVENT_DESCRIPTION { get { return _event_description; } set { SetField(ref _event_description, value, "EVENT_DESCRIPTION"); } }
        private String _event_description;

        [Column("EVENT_CALL")]
        public String EVENT_CALL { get { return _event_call; } set { SetField(ref _event_call, value, "EVENT_CALL"); } }
        private String _event_call;

        [Column("ACTIVE")]
        public Int32? ACTIVE { get { return _active; } set { SetField(ref _active, value, "ACTIVE"); } }
        private Int32? _active;

        [Column("CALL_TYPE_ID")]
        public Int32? CALL_TYPE_ID { get { return _call_type_id; } set { SetField(ref _call_type_id, value, "CALL_TYPE_ID"); } }
        private Int32? _call_type_id;

        [Column("EVENT_CRITERIA")]
        public String EVENT_CRITERIA { get { return _event_criteria; } set { SetField(ref _event_criteria, value, "EVENT_CRITERIA"); } }
        private String _event_criteria;

        [Column("CHAIN_ID")]
        public String CHAIN_ID { get { return _chain_id; } set { SetField(ref _chain_id, value, "CHAIN_ID"); } }
        private String _chain_id;

    }

}
