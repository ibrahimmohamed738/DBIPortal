using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class LinkAccount
    {
        public string MSISDN { get; set; }
        public string AccountNo { get; set; }
        public string AccountType { get; set; } 
        public string Currency { get; set; }
        public decimal NewDailyLimit { get; set; }
        public string Branch { get; set; } 
        public string EDahabType { get; set; }
        public string AccountHolder { get; set; }
        public string CreatedBy { get; set; }
        public decimal DailyLimit { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        [Display(Name = "Name")]
        public string EDahabName { get; set; } 
        public string Remarks { get; set; }
        public string AgentCode { get; set; }
    }
}