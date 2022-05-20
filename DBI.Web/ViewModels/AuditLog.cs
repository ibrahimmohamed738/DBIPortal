using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string ActivityType { get; set; }
        public DateTime ActivityTime { get; set; }
        public string Description { get; set; }
        public string AffectedParty { get; set; }
    }
}