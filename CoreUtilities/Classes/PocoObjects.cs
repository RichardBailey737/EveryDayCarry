using Ensur.Core.Utilities.Database;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensur.Core.Utilities.Classes
{


    [TableName("dbo.DCS_TRIGGER_EVENT")]
    [PrimaryKey("EVENT_ID")]
    [ExplicitColumns]
    public partial class DCS_TRIGGER_EVENT : PocoDB.Record<DCS_TRIGGER_EVENT>
    {
        [Column] public int EVENT_ID { get; set; }
        [Column] public int? EVENT_TYPE_ID { get; set; }
        [Column] public int? SEQ { get; set; }
        [Column] public string EVENT_DESCRIPTION { get; set; }
        [Column] public string EVENT_CALL { get; set; }
        [Column] public int? ACTIVE { get; set; }
        [Column] public int? CALL_TYPE_ID { get; set; }
        [Column] public string EVENT_CRITERIA { get; set; }
        [Column] public int? CHAIN_ID { get; set; }
    }


}
