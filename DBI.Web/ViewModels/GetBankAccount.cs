using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class GetBankAccount
    {
        public string MSISDN { get; set; }
        public string EDahabName { get; set; }
        public string PIN { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string EDahabType { get; set; }
        public string Active { get; set; }
        public bool Verified { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string Remarks { get; set; }
        public string Email { get; set; }
        public string AgentCode { get; set; }
    }
}