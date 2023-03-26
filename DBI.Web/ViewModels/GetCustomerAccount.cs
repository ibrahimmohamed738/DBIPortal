using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBI_eDahab.Web.ViewModels
{
    public class GetCustomerAccount
    {
        public string MSISDN { get; set; }
        public string EDahabName { get; set; }
        public string Active { get; set; }
        public string EDahabType { get; set; }
        public string AccountHolder { get; set; }
        public decimal DailyLimit { get; set; }
        public decimal NewDailyLimit { get; set; }
        public string AccountNo { get; set; }
        public string AccountType { get; set; }
        public string Currency { get; set; }
        public bool Verified { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string Remarks { get; set; }
    }
}