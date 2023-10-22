using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class GetRegistrationReport
    {
        public string EDahabName { get; set; }
        public string Msisdn { get; set; }
        public string AgentCode { get; set; }
        public string AccountNo { get; set; }
        public DateTime CreatedOn { get; set; }
        public string EDahabType { get; set; }
    }
}